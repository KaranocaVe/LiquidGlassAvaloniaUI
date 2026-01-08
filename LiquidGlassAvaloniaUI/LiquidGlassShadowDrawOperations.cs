using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace LiquidGlassAvaloniaUI
{
    internal sealed class LiquidGlassShadowDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _controlBounds;
        private readonly Rect _operationBounds;
        private readonly LiquidGlassDrawParameters _parameters;

        public LiquidGlassShadowDrawOperation(Rect controlBounds, LiquidGlassDrawParameters parameters)
        {
            _controlBounds = controlBounds;
            _parameters = parameters;

            var radius = Math.Max(0.0, parameters.ShadowRadius);
            var offset = parameters.ShadowOffset;
            var pad = radius * 2.0;

            var leftPad = pad + Math.Max(0.0, -offset.X);
            var topPad = pad + Math.Max(0.0, -offset.Y);
            var rightPad = pad + Math.Max(0.0, offset.X);
            var bottomPad = pad + Math.Max(0.0, offset.Y);

            _operationBounds = new Rect(
                -leftPad,
                -topPad,
                controlBounds.Width + leftPad + rightPad,
                controlBounds.Height + topPad + bottomPad);
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        public Rect Bounds => _operationBounds;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            var size = new SKSize((float)_controlBounds.Width, (float)_controlBounds.Height);
            if (size.Width <= 0 || size.Height <= 0)
                return;

            var radius = (float)Clamp(_parameters.ShadowRadius, 0.0, 512.0);
            if (radius <= 0.001f)
                return;

            var opacity = (float)Clamp(_parameters.ShadowOpacity, 0.0, 1.0);
            var color = _parameters.ShadowColor;
            var alpha = (byte)Clamp(color.A * opacity, 0.0, 255.0);
            if (alpha <= 0)
                return;

            var offsetX = (float)_parameters.ShadowOffset.X;
            var offsetY = (float)_parameters.ShadowOffset.Y;

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = LiquidGlassPathUtils.GetCornerRadii(_parameters.CornerRadius, maxRadius);
            var rect = SKRect.Create(0, 0, size.Width, size.Height);

            using var path = LiquidGlassPathUtils.CreateRoundRectPath(rect, cornerRadii);
            using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, radius);

            using var shadowPaint = new SKPaint
            {
                Color = new SKColor(color.R, color.G, color.B, alpha),
                MaskFilter = blur,
                IsAntialias = true
            };

            using var clearPaint = new SKPaint
            {
                BlendMode = SKBlendMode.Clear,
                IsAntialias = true
            };

            var pad = radius * 2.0f;
            var layerBounds = SKRect.Create(-pad, -pad, size.Width + pad * 2.0f, size.Height + pad * 2.0f);

            canvas.SaveLayer(layerBounds, null);

            canvas.Save();
            canvas.Translate(offsetX, offsetY);
            canvas.DrawPath(path, shadowPaint);
            canvas.Restore();

            canvas.DrawPath(path, clearPaint);

            canvas.Restore();
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    internal sealed class LiquidGlassInnerShadowDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly LiquidGlassDrawParameters _parameters;

        public LiquidGlassInnerShadowDrawOperation(Rect bounds, LiquidGlassDrawParameters parameters)
        {
            _bounds = bounds;
            _parameters = parameters;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        public Rect Bounds => _bounds;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
            if (size.Width <= 0 || size.Height <= 0)
                return;

            var radius = (float)Clamp(_parameters.InnerShadowRadius, 0.0, 512.0);
            if (radius <= 0.001f)
                return;

            var opacity = (float)Clamp(_parameters.InnerShadowOpacity, 0.0, 1.0);
            var color = _parameters.InnerShadowColor;
            var alpha = (byte)Clamp(color.A * opacity, 0.0, 255.0);
            if (alpha <= 0)
                return;

            var offsetX = (float)_parameters.InnerShadowOffset.X;
            var offsetY = (float)_parameters.InnerShadowOffset.Y;

            var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
            var cornerRadii = LiquidGlassPathUtils.GetCornerRadii(_parameters.CornerRadius, maxRadius);
            var rect = SKRect.Create(0, 0, size.Width, size.Height);

            using var path = LiquidGlassPathUtils.CreateRoundRectPath(rect, cornerRadii);

            using var blur = SKImageFilter.CreateBlur(radius, radius, SKShaderTileMode.Decal, null, rect);
            using var layerPaint = new SKPaint { ImageFilter = blur };

            using var fillPaint = new SKPaint
            {
                Color = new SKColor(color.R, color.G, color.B, alpha),
                IsAntialias = true
            };

            using var clearPaint = new SKPaint
            {
                BlendMode = SKBlendMode.Clear,
                IsAntialias = true
            };

            canvas.Save();
            canvas.ClipPath(path, SKClipOperation.Intersect, true);

            canvas.SaveLayer(rect, layerPaint);
            canvas.DrawPath(path, fillPaint);
            canvas.Translate(offsetX, offsetY);
            canvas.DrawPath(path, clearPaint);
            canvas.Translate(-offsetX, -offsetY);
            canvas.Restore();

            canvas.Restore();
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
