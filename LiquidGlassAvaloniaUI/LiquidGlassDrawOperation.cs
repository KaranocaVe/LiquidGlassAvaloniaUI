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
        private static SKRuntimeEffect? s_blurEffect;
        private static SKRuntimeEffect? s_vibrancyEffect;
        private static SKRuntimeEffect? s_interactiveHighlightEffect;
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
            s_blurEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassBlur.sksl");
            s_vibrancyEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassVibrancy.sksl");
            s_interactiveHighlightEffect = LoadRuntimeEffect("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassInteractiveHighlight.sksl");
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

            using var backdropShader = SKShader.CreateImage(
                _backdropSnapshot.Image,
                SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp,
                WithPixelOrigin(currentInvertedTransform, _backdropSnapshot.OriginInPixels));

            var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
            if (size.Width <= 0 || size.Height <= 0)
                return;

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = GetCornerRadii(_parameters.CornerRadius, maxRadius);

            // AndroidLiquidGlass pipeline order:
            //   vibrancy (saturation) -> blur -> lens
            var vibrancy = (float)Clamp(_parameters.Vibrancy, 0.0, 3.0);
            using var vibrancyShader = CreateVibrancyShader(backdropShader, vibrancy);
            var preBlur = (SKShader?)vibrancyShader ?? backdropShader;

            var blurRadius = (float)Clamp(_parameters.BlurRadius, 0.0, 100.0);
            using var blurredShader = CreateBlurShader(preBlur, blurRadius);
            var lensInput = (SKShader?)blurredShader ?? preBlur;

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

            using var paint = new SKPaint
            {
                Shader = lensShader ?? lensInput,
                IsAntialias = true
            };

            var rect = SKRect.Create(0, 0, size.Width, size.Height);
            using var clipPath = CreateRoundRectPath(rect, cornerRadii);

            canvas.Save();
            canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
            canvas.DrawRect(rect, paint);

            DrawSurfaceOverlay(canvas, rect);

            canvas.Restore();

            lensShader?.Dispose();
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

            // Mirror AndroidLiquidGlass' highlight GraphicsLayer "safeSize + translate" approach.
            // This avoids edge artifacts when the visual is transformed and/or rasterized into an intermediate surface.
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

        private static SKShader? CreateBlurShader(SKShader input, float radius)
        {
            if (radius <= 0.001f)
                return null;
            if (s_blurEffect is null)
                return null;

            using var uniforms = new SKRuntimeEffectUniforms(s_blurEffect);
            uniforms["radius"] = radius;
            using var children = new SKRuntimeEffectChildren(s_blurEffect);
            children["content"] = input;
            return s_blurEffect.ToShader(uniforms, children);
        }

        private static SKShader? CreateVibrancyShader(SKShader input, float saturation)
        {
            if (Math.Abs(saturation - 1.0f) < 0.0001f)
                return null;
            if (s_vibrancyEffect is null)
                return null;

            using var uniforms = new SKRuntimeEffectUniforms(s_vibrancyEffect);
            uniforms["saturation"] = saturation;
            using var children = new SKRuntimeEffectChildren(s_vibrancyEffect);
            children["content"] = input;
            return s_vibrancyEffect.ToShader(uniforms, children);
        }

        private void DrawSurfaceOverlay(SKCanvas canvas, SKRect rect)
        {
            // AndroidLiquidGlass draws optional tint/surface fills via onDrawSurface.
            // If TintColor is specified, it draws it twice: Hue blend + alpha fill.
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
