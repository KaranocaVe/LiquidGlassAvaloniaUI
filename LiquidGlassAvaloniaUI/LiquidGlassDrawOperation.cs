using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace LiquidGlassAvaloniaUI
{
    internal class LiquidGlassDrawOperation : ICustomDrawOperation
    {
        private static SKRuntimeEffect? s_lensEffect;
        private static SKRuntimeEffect? s_highlightEffect;
        private static SKRuntimeEffect? s_gammaEffect;
        private static SKRuntimeEffect? s_interactiveHighlightEffect;
        private static SKRuntimeEffect? s_progressiveMaskEffect;
        private static SKRuntimeEffect? s_backdropTransformEffect;
        private static bool s_loaded;

        private readonly Rect _bounds;
        private readonly LiquidGlassDrawParameters _parameters;
        private readonly LiquidGlassBackdropSnapshot? _backdropSnapshot;
        private readonly LiquidGlassDrawPass _pass;

        public LiquidGlassDrawOperation(
            Rect bounds,
            LiquidGlassDrawParameters parameters,
            LiquidGlassBackdropSnapshot? snapshot,
            LiquidGlassDrawPass pass)
        {
            _bounds = bounds;
            _parameters = parameters;
            _backdropSnapshot = snapshot is not null && snapshot.TryAddLease() ? snapshot : null;
            _pass = pass;
        }

        public void Dispose()
        {
            _backdropSnapshot?.ReleaseLease();
        }

        public bool HitTest(Point p) => false;

        public Rect Bounds => _bounds;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
                return;

            LoadShaders();

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            switch (_pass)
            {
                case LiquidGlassDrawPass.Lens:
                    RenderLens(canvas);
                    break;
                case LiquidGlassDrawPass.InteractiveHighlight:
                    RenderInteractiveHighlight(canvas);
                    break;
                case LiquidGlassDrawPass.Highlight:
                    RenderHighlight(canvas);
                    break;
            }
        }

        private static void LoadShaders()
        {
            if (s_loaded)
                return;
            s_loaded = true;

            s_lensEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassShader.sksl");
            s_highlightEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassHighlight.sksl");
            s_gammaEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassGamma.sksl");
            s_interactiveHighlightEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassInteractiveHighlight.sksl");
            s_progressiveMaskEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassProgressiveMask.sksl");
            s_backdropTransformEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassBackdropTransform.sksl");
        }

        private static SKRuntimeEffect? LoadRuntimeEffect(string assetUriString)
        {
            try
            {
                var assetUri = new Uri(assetUriString);
                using var stream = AssetLoader.Open(assetUri);
                using var reader = new StreamReader(stream);
                var shaderCode = reader.ReadToEnd();

                var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
                if (effect == null)
                    Console.WriteLine($"[LiquidGlass] Failed to create SKRuntimeEffect ({assetUriString}): {errorText}");

                return effect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LiquidGlass] Exception while loading shader ({assetUriString}): {ex.Message}");
                return null;
            }
        }

        private void RenderLens(SKCanvas canvas)
        {
            if (s_lensEffect is null)
            {
                DrawErrorHint(canvas);
                return;
            }

            if (_backdropSnapshot is null)
            {
                DrawBackdropNotReady(canvas);
                return;
            }

            if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                return;

            var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
            if (size.Width <= 0 || size.Height <= 0)
                return;

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = GetCornerRadii(_parameters.CornerRadius, maxRadius);

            // Pipeline order:
            //   color controls (brightness/contrast/vibrancy) -> blur -> lens -> (optional gamma)
            //
            // In Avalonia we approximate the RenderEffect chain by applying an SKImageFilter pipeline to the
            // captured backdrop snapshot, then sampling the filtered image in the lens runtime shader.
            var filtered = GetOrCreateFilteredBackdrop(_backdropSnapshot, _parameters);

            using var backdropShader = SKShader.CreateImage(
                filtered.Image,
                SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp,
                WithPixelOrigin(currentInvertedTransform, filtered.OriginInPixels));

            var lensInput = (SKShader)backdropShader;

            SKShader? backdropTransformShader = null;
            try
            {
                var zoomValue = _parameters.BackdropZoom;
                if (zoomValue <= 0.0005 || double.IsNaN(zoomValue) || double.IsInfinity(zoomValue))
                    zoomValue = 1.0;

                var zoom = (float)Clamp(zoomValue, 0.1, 10.0);
                var offset = _parameters.BackdropOffset;
                var needsTransform =
                    Math.Abs(zoom - 1.0f) > 0.0005f
                    || Math.Abs(offset.X) > 0.0005
                    || Math.Abs(offset.Y) > 0.0005;

                if (needsTransform && s_backdropTransformEffect is not null)
                {
                    using var uniforms = new SKRuntimeEffectUniforms(s_backdropTransformEffect);
                    uniforms["size"] = new[] { size.Width, size.Height };
                    uniforms["zoom"] = zoom;
                    uniforms["offset"] = new[] { (float)offset.X, (float)offset.Y };

                    using var children = new SKRuntimeEffectChildren(s_backdropTransformEffect);
                    children["content"] = lensInput;

                    backdropTransformShader = s_backdropTransformEffect.ToShader(uniforms, children);
                    if (backdropTransformShader is not null)
                        lensInput = backdropTransformShader;
                }

                var refractionHeight = (float)Clamp(_parameters.RefractionHeight, 0.0, Math.Min(size.Width, size.Height) * 0.5);
                var refractionAmount = (float)_parameters.RefractionAmount;
                var applyLens = refractionHeight > 0.001f && Math.Abs(refractionAmount) > 0.001f;

                SKShader? lensShader = null;
                if (applyLens)
                {
                    using var lensUniforms = new SKRuntimeEffectUniforms(s_lensEffect);
                    lensUniforms["size"] = new[] { size.Width, size.Height };
                    lensUniforms["cornerRadii"] = cornerRadii;
                    lensUniforms["refractionHeight"] = refractionHeight;
                    lensUniforms["refractionAmount"] = -refractionAmount; // Android uses negative refraction amount
                    lensUniforms["depthEffect"] = _parameters.DepthEffect ? 1.0f : 0.0f;
                    lensUniforms["chromaticAberration"] = _parameters.ChromaticAberration ? 1.0f : 0.0f;

                    using var lensChildren = new SKRuntimeEffectChildren(s_lensEffect);
                    lensChildren["content"] = lensInput;

                    lensShader = s_lensEffect.ToShader(lensUniforms, lensChildren);
                }

                var baseShader = lensShader ?? lensInput;

                SKShader? progressiveMaskShader = null;
                try
                {
                    if (_parameters.ProgressiveBlurEnabled
                        && s_progressiveMaskEffect is not null)
                    {
                        using var uniforms = new SKRuntimeEffectUniforms(s_progressiveMaskEffect);
                        uniforms["size"] = new[] { size.Width, size.Height };
                        uniforms["start"] = (float)Clamp(_parameters.ProgressiveBlurStart, 0.0, 1.0);
                        uniforms["end"] = (float)Clamp(_parameters.ProgressiveBlurEnd, 0.0, 1.0);

                        var tint = _parameters.ProgressiveTintColor;
                        uniforms["tint"] = new[]
                        {
                            tint.R / 255f,
                            tint.G / 255f,
                            tint.B / 255f,
                            tint.A / 255f
                        };

                        var tintIntensity = tint.A > 0
                            ? (float)Clamp(_parameters.ProgressiveTintIntensity, 0.0, 1.0)
                            : 0.0f;
                        uniforms["tintIntensity"] = tintIntensity;

                        using var children = new SKRuntimeEffectChildren(s_progressiveMaskEffect);
                        children["content"] = baseShader;

                        progressiveMaskShader = s_progressiveMaskEffect.ToShader(uniforms, children);
                        if (progressiveMaskShader is not null)
                            baseShader = progressiveMaskShader;
                    }

                    SKShader? gammaShader = null;
                    try
                    {
                        var gammaPower = (float)Clamp(_parameters.GammaPower, 0.0, 10.0);
                        if (s_gammaEffect is not null && Math.Abs(gammaPower - 1.0f) > 0.0005f)
                        {
                            using var uniforms = new SKRuntimeEffectUniforms(s_gammaEffect);
                            uniforms["power"] = gammaPower;
                            using var children = new SKRuntimeEffectChildren(s_gammaEffect);
                            children["content"] = baseShader;
                            gammaShader = s_gammaEffect.ToShader(uniforms, children);
                        }

                        using var paint = new SKPaint
                        {
                            Shader = gammaShader ?? baseShader,
                            IsAntialias = true
                        };

                        var rect = SKRect.Create(0, 0, size.Width, size.Height);
                        using var clipPath = CreateRoundRectPath(rect, cornerRadii);

                        canvas.Save();
                        canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
                        canvas.DrawRect(rect, paint);

                        DrawSurfaceOverlay(canvas, rect);

                        canvas.Restore();
                    }
                    finally
                    {
                        gammaShader?.Dispose();
                    }
                }
                finally
                {
                    progressiveMaskShader?.Dispose();
                    lensShader?.Dispose();
                }
            }
            finally
            {
                backdropTransformShader?.Dispose();
            }

        }

        private static LiquidGlassBackdropSnapshot.FilteredResult GetOrCreateFilteredBackdrop(
            LiquidGlassBackdropSnapshot snapshot,
            LiquidGlassDrawParameters parameters)
        {
            var brightness = (float)Clamp(parameters.Brightness, -1.0, 1.0);
            var contrast = (float)Clamp(parameters.Contrast, 0.0, 4.0);
            var saturation = (float)Clamp(parameters.Vibrancy, 0.0, 4.0);
            var exposureEv = (float)Clamp(parameters.ExposureEv, -8.0, 8.0);
            var opacity = (float)Clamp(parameters.BackdropOpacity, 0.0, 1.0);

            var blurSigmaPx = (float)(Clamp(parameters.BlurRadius, 0.0, 256.0) * snapshot.Scaling);

            var isIdentity =
                Math.Abs(brightness) < 0.0005f
                && Math.Abs(contrast - 1.0f) < 0.0005f
                && Math.Abs(saturation - 1.0f) < 0.0005f
                && Math.Abs(exposureEv) < 0.0005f
                && Math.Abs(opacity - 1.0f) < 0.0005f
                && blurSigmaPx <= 0.0005f;

            if (isIdentity)
                return new LiquidGlassBackdropSnapshot.FilteredResult(snapshot.Image, snapshot.OriginInPixels);

            // Quantize to keep the cache size stable during slider drags.
            const float q = 1000.0f;
            var key = new LiquidGlassBackdropSnapshot.FilteredKey(
                brightnessQ: (int)Math.Round(brightness * q),
                contrastQ: (int)Math.Round(contrast * q),
                saturationQ: (int)Math.Round(saturation * q),
                exposureEvQ: (int)Math.Round(exposureEv * q),
                opacityQ: (int)Math.Round(opacity * q),
                blurSigmaPxQ: (int)Math.Round(blurSigmaPx * q));

            if (snapshot.TryGetFiltered(key, out var cached))
                return cached;

            var filtered = CreateFilteredBackdrop(snapshot, brightness, contrast, saturation, exposureEv, opacity, blurSigmaPx)
                           ?? new LiquidGlassBackdropSnapshot.FilteredResult(snapshot.Image, snapshot.OriginInPixels);

            // Never cache the unfiltered snapshot image (it is owned/disposed separately).
            if (!ReferenceEquals(filtered.Image, snapshot.Image))
                snapshot.StoreFiltered(key, filtered);

            return filtered;
        }

        private static LiquidGlassBackdropSnapshot.FilteredResult? CreateFilteredBackdrop(
            LiquidGlassBackdropSnapshot snapshot,
            float brightness,
            float contrast,
            float saturation,
            float exposureEv,
            float opacity,
            float blurSigmaPx)
        {
            var source = snapshot.Image;

            SKImageFilter? filter = null;

            var needsColorControls =
                Math.Abs(brightness) > 0.0005f
                || Math.Abs(contrast - 1.0f) > 0.0005f
                || Math.Abs(saturation - 1.0f) > 0.0005f;

            if (needsColorControls)
            {
                using var colorFilter = SKColorFilter.CreateColorMatrix(CreateColorControlsColorMatrix(brightness, contrast, saturation));
                filter = SKImageFilter.CreateColorFilter(colorFilter, filter);
            }

            if (Math.Abs(exposureEv) > 0.0005f)
            {
                using var colorFilter = SKColorFilter.CreateColorMatrix(CreateExposureColorMatrix(exposureEv));
                filter = SKImageFilter.CreateColorFilter(colorFilter, filter);
            }

            if (Math.Abs(opacity - 1.0f) > 0.0005f)
            {
                using var colorFilter = SKColorFilter.CreateColorMatrix(CreateOpacityColorMatrix(opacity));
                filter = SKImageFilter.CreateColorFilter(colorFilter, filter);
            }

            if (blurSigmaPx > 0.0005f)
            {
                // Use TileMode.Clamp: Skia's default tile mode can introduce alpha falloff near image edges (kDecal),
                // which shows up as darkened borders when the snapshot is clipped by the window bounds.
                var cropRect = new SKRect(0, 0, source.Width, source.Height);
                filter = SKImageFilter.CreateBlur(blurSigmaPx, blurSigmaPx, SKShaderTileMode.Clamp, filter, cropRect);
            }

            if (filter is null)
                return null;

            using (filter)
            {
                // ApplyImageFilter() can return a varying offset/subset for blur radii, which can cause the
                // sampled backdrop to appear to "drift" while dragging the blur slider. Instead, render the
                // filtered snapshot into an explicit same-size surface at (0,0) so the origin remains stable.
                var info = new SKImageInfo(source.Width, source.Height, source.ColorType, source.AlphaType, source.ColorSpace);
                using var surface = SKSurface.Create(info);
                if (surface is null)
                    return null;

                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                using var paint = new SKPaint
                {
                    ImageFilter = filter,
                    BlendMode = SKBlendMode.Src
                };

                canvas.DrawImage(source, 0, 0, paint);
                canvas.Flush();

                var filteredImage = surface.Snapshot();
                return new LiquidGlassBackdropSnapshot.FilteredResult(filteredImage, snapshot.OriginInPixels);
            }
        }

        private static float[] CreateColorControlsColorMatrix(float brightness, float contrast, float saturation)
        {
            // Color-controls matrix (brightness/contrast/saturation).
            // Note: Skia's color matrix operates on normalized (0..1) colors.
            var invSat = 1f - saturation;
            var r = 0.213f * invSat;
            var g = 0.715f * invSat;
            var b = 0.072f * invSat;

            var c = contrast;
            // Translation terms must also be normalized.
            var t = (0.5f - c * 0.5f + brightness);
            var s = saturation;

            var cr = c * r;
            var cg = c * g;
            var cb = c * b;
            var cs = c * s;

            return new[]
            {
                cr + cs, cg, cb, 0f, t,
                cr, cg + cs, cb, 0f, t,
                cr, cg, cb + cs, 0f, t,
                0f, 0f, 0f, 1f, 0f
            };
        }

        private static float[] CreateExposureColorMatrix(float ev)
        {
            var scale = (float)Math.Pow(2.0, ev / 2.2);
            return new[]
            {
                scale, 0f, 0f, 0f, 0f,
                0f, scale, 0f, 0f, 0f,
                0f, 0f, scale, 0f, 0f,
                0f, 0f, 0f, 1f, 0f
            };
        }

        private static float[] CreateOpacityColorMatrix(float alpha)
        {
            return new[]
            {
                1f, 0f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f, 0f,
                0f, 0f, 1f, 0f, 0f,
                0f, 0f, 0f, alpha, 0f
            };
        }

        private void RenderInteractiveHighlight(SKCanvas canvas)
        {
            if (s_interactiveHighlightEffect is null)
                return;

            var progress = (float)Clamp(_parameters.InteractiveProgress, 0.0, 1.0);
            if (progress <= 0.001f)
                return;

            var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
            if (size.Width <= 0 || size.Height <= 0)
                return;

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = GetCornerRadii(_parameters.CornerRadius, maxRadius);

            var rect = SKRect.Create(0, 0, size.Width, size.Height);
            using var clipPath = CreateRoundRectPath(rect, cornerRadii);

            canvas.Save();
            canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);

            using (var basePaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(Clamp(0.08f * progress * 255f, 0f, 255f))),
                BlendMode = SKBlendMode.Plus,
                IsAntialias = true
            })
            {
                canvas.DrawRect(rect, basePaint);
            }

            using var uniforms = new SKRuntimeEffectUniforms(s_interactiveHighlightEffect);
            uniforms["size"] = new[] { size.Width, size.Height };
            uniforms["color"] = new[] { 1.0f, 1.0f, 1.0f, (float)Clamp(0.15 * progress, 0.0, 1.0) };
            uniforms["radius"] = Math.Min(size.Width, size.Height) * 1.5f;
            uniforms["position"] = new[]
            {
                (float)Clamp(_parameters.InteractivePosition.X, 0.0, size.Width),
                (float)Clamp(_parameters.InteractivePosition.Y, 0.0, size.Height)
            };

            using var children = new SKRuntimeEffectChildren(s_interactiveHighlightEffect);
            using var shader = s_interactiveHighlightEffect.ToShader(uniforms, children);

            if (shader is not null)
            {
                using var paint = new SKPaint
                {
                    Shader = shader,
                    BlendMode = SKBlendMode.Plus,
                    IsAntialias = true
                };
                canvas.DrawRect(rect, paint);
            }

            canvas.Restore();
        }

        private void RenderHighlight(SKCanvas canvas)
        {
            if (s_highlightEffect is null)
                return;

            if (_parameters.HighlightOpacity <= 0.001 || _parameters.HighlightWidth <= 0.001)
                return;

            var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
            if (size.Width <= 0 || size.Height <= 0)
                return;

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = GetCornerRadii(_parameters.CornerRadius, maxRadius);

            using var uniforms = new SKRuntimeEffectUniforms(s_highlightEffect);
            uniforms["size"] = new[] { size.Width, size.Height };
            uniforms["cornerRadii"] = cornerRadii;

            var alpha = (float)Clamp(_parameters.HighlightOpacity, 0.0, 1.0);
            uniforms["color"] = new[] { 1.0f, 1.0f, 1.0f, alpha };

            var angleRad = (float)(_parameters.HighlightAngleDegrees * (Math.PI / 180.0));
            uniforms["angle"] = angleRad;
            uniforms["falloff"] = (float)Clamp(_parameters.HighlightFalloff, 0.0, 8.0);

            using var children = new SKRuntimeEffectChildren(s_highlightEffect);
            using var shader = s_highlightEffect.ToShader(uniforms, children);
            if (shader is null)
                return;

            var blurRadius = (float)Clamp(_parameters.HighlightBlurRadius, 0.0, 20.0);
            using var maskFilter = blurRadius > 0.001f
                ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius)
                : null;

            var strokeWidth = (float)(Math.Ceiling(Clamp(_parameters.HighlightWidth, 0.0, 100.0)) * 2.0);

            using var paint = new SKPaint
            {
                Shader = shader,
                IsAntialias = true,
                BlendMode = SKBlendMode.Plus,
                Style = SKPaintStyle.Stroke,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round,
                StrokeWidth = Math.Max(0.5f, strokeWidth),
                MaskFilter = maskFilter
            };

            var rect = SKRect.Create(0, 0, size.Width, size.Height);
            using var path = CreateRoundRectPath(rect, cornerRadii);

            // Pad the highlight layer to avoid edge artifacts when transformed and/or rasterized into an intermediate surface.
            const float safePad = 1.0f;

            canvas.Save();
            canvas.Translate(-safePad, -safePad);
            var layerBounds = SKRect.Create(0, 0, size.Width + safePad * 2.0f, size.Height + safePad * 2.0f);
            canvas.SaveLayer(layerBounds, null);

            canvas.Translate(safePad, safePad);
            canvas.ClipPath(path, SKClipOperation.Intersect, true);
            canvas.DrawPath(path, paint);

            canvas.Restore();
            canvas.Restore();
        }

        private void DrawSurfaceOverlay(SKCanvas canvas, SKRect rect)
        {
            // Optional tint/surface overlays. If TintColor is specified, it draws it twice: Hue blend + alpha fill.
            if (_parameters.TintColor.A > 0)
            {
                var tint = _parameters.TintColor;
                using var huePaint = new SKPaint
                {
                    Color = new SKColor(tint.R, tint.G, tint.B, 255),
                    IsAntialias = true,
                    BlendMode = SKBlendMode.Hue
                };
                canvas.DrawRect(rect, huePaint);

                using var fillPaint = new SKPaint
                {
                    Color = new SKColor(tint.R, tint.G, tint.B, (byte)Clamp(tint.A * 0.75, 0.0, 255.0)),
                    IsAntialias = true,
                    BlendMode = SKBlendMode.SrcOver
                };
                canvas.DrawRect(rect, fillPaint);
            }

            if (_parameters.SurfaceColor.A > 0)
            {
                var surface = _parameters.SurfaceColor;
                using var paint = new SKPaint
                {
                    Color = new SKColor(surface.R, surface.G, surface.B, surface.A),
                    IsAntialias = true,
                    BlendMode = SKBlendMode.SrcOver
                };
                canvas.DrawRect(rect, paint);
            }
        }

        private static float[] GetCornerRadii(CornerRadius cornerRadius, float maxRadius)
        {
            var tl = (float)Clamp(cornerRadius.TopLeft, 0.0, maxRadius);
            var tr = (float)Clamp(cornerRadius.TopRight, 0.0, maxRadius);
            var br = (float)Clamp(cornerRadius.BottomRight, 0.0, maxRadius);
            var bl = (float)Clamp(cornerRadius.BottomLeft, 0.0, maxRadius);
            return new[] { tl, tr, br, bl };
        }

        private static SKPath CreateRoundRectPath(SKRect rect, float[] cornerRadii)
        {
            using var rr = new SKRoundRect();
            rr.SetRectRadii(rect, new[]
            {
                new SKPoint(cornerRadii[0], cornerRadii[0]),
                new SKPoint(cornerRadii[1], cornerRadii[1]),
                new SKPoint(cornerRadii[2], cornerRadii[2]),
                new SKPoint(cornerRadii[3], cornerRadii[3]),
            });

            var path = new SKPath();
            path.AddRoundRect(rr, SKPathDirection.Clockwise);
            return path;
        }

        private void DrawErrorHint(SKCanvas canvas)
        {
            using var errorPaint = new SKPaint
            {
                Color = new SKColor(255, 0, 0, 120),
                Style = SKPaintStyle.Fill
            };

            canvas.DrawRect(SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height), errorPaint);
        }

        private void DrawBackdropNotReady(SKCanvas canvas)
        {
            var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
            var rect = SKRect.Create(0, 0, size.Width, size.Height);

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = GetCornerRadii(_parameters.CornerRadius, maxRadius);
            using var path = CreateRoundRectPath(rect, cornerRadii);

            using var paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 32),
                IsAntialias = true
            };

            canvas.Save();
            canvas.ClipPath(path, SKClipOperation.Intersect, true);
            canvas.DrawRect(rect, paint);
            canvas.Restore();
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static SKMatrix WithPixelOrigin(SKMatrix invertedTransform, PixelPoint originInPixels)
        {
            // localMatrix = invertedTransform * Translate(+originInPixels)
            //
            // We first cancel the current canvas transform (so shader coordinates become device pixels),
            // then shift into the clipped snapshot's coordinate system.
            var ox = (float)originInPixels.X;
            var oy = (float)originInPixels.Y;
            invertedTransform.TransX = invertedTransform.TransX + invertedTransform.ScaleX * ox + invertedTransform.SkewX * oy;
            invertedTransform.TransY = invertedTransform.TransY + invertedTransform.SkewY * ox + invertedTransform.ScaleY * oy;
            return invertedTransform;
        }
    }
}
