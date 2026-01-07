using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// A composited “liquid glass” surface inspired by AndroidLiquidGlass’ backdrop pipeline:
    /// vibrancy + blur + lens refraction + edge highlight.
    /// </summary>
    public class LiquidGlassSurface : Decorator
    {
        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner<LiquidGlassSurface>();

        public static readonly StyledProperty<double> RefractionHeightProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(RefractionHeight), 12.0);

        public static readonly StyledProperty<double> RefractionAmountProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(RefractionAmount), 24.0);

        public static readonly StyledProperty<bool> DepthEffectProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(DepthEffect), false);

        public static readonly StyledProperty<bool> ChromaticAberrationProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(ChromaticAberration), false);

        public static readonly StyledProperty<double> BlurRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(BlurRadius), 2.0);

        public static readonly StyledProperty<double> VibrancyProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(Vibrancy), 1.5);

        public static readonly StyledProperty<Color> TintColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(TintColor), Colors.Transparent);

        public static readonly StyledProperty<Color> SurfaceColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(SurfaceColor), Colors.Transparent);

        public static readonly StyledProperty<bool> HighlightEnabledProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(HighlightEnabled), true);

        public static readonly StyledProperty<double> HighlightWidthProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(HighlightWidth), 0.5);

        public static readonly StyledProperty<double> HighlightBlurRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(HighlightBlurRadius), 0.25);

        public static readonly StyledProperty<double> HighlightOpacityProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(HighlightOpacity), 0.5);

        public static readonly StyledProperty<double> HighlightAngleProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(HighlightAngle), 45.0);

        public static readonly StyledProperty<double> HighlightFalloffProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(HighlightFalloff), 1.0);

        static LiquidGlassSurface()
        {
            AffectsRender<LiquidGlassSurface>(
                CornerRadiusProperty,
                RefractionHeightProperty,
                RefractionAmountProperty,
                DepthEffectProperty,
                ChromaticAberrationProperty,
                BlurRadiusProperty,
                VibrancyProperty,
                TintColorProperty,
                SurfaceColorProperty,
                HighlightEnabledProperty,
                HighlightWidthProperty,
                HighlightBlurRadiusProperty,
                HighlightOpacityProperty,
                HighlightAngleProperty,
                HighlightFalloffProperty);

            CornerRadiusProperty.Changed.AddClassHandler<LiquidGlassSurface>((x, _) => x.UpdateClipGeometry());
        }

        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public double RefractionHeight
        {
            get => GetValue(RefractionHeightProperty);
            set => SetValue(RefractionHeightProperty, value);
        }

        public double RefractionAmount
        {
            get => GetValue(RefractionAmountProperty);
            set => SetValue(RefractionAmountProperty, value);
        }

        public bool DepthEffect
        {
            get => GetValue(DepthEffectProperty);
            set => SetValue(DepthEffectProperty, value);
        }

        public bool ChromaticAberration
        {
            get => GetValue(ChromaticAberrationProperty);
            set => SetValue(ChromaticAberrationProperty, value);
        }

        public double BlurRadius
        {
            get => GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        public double Vibrancy
        {
            get => GetValue(VibrancyProperty);
            set => SetValue(VibrancyProperty, value);
        }

        public Color TintColor
        {
            get => GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        public Color SurfaceColor
        {
            get => GetValue(SurfaceColorProperty);
            set => SetValue(SurfaceColorProperty, value);
        }

        public bool HighlightEnabled
        {
            get => GetValue(HighlightEnabledProperty);
            set => SetValue(HighlightEnabledProperty, value);
        }

        public double HighlightWidth
        {
            get => GetValue(HighlightWidthProperty);
            set => SetValue(HighlightWidthProperty, value);
        }

        public double HighlightBlurRadius
        {
            get => GetValue(HighlightBlurRadiusProperty);
            set => SetValue(HighlightBlurRadiusProperty, value);
        }

        public double HighlightOpacity
        {
            get => GetValue(HighlightOpacityProperty);
            set => SetValue(HighlightOpacityProperty, value);
        }

        public double HighlightAngle
        {
            get => GetValue(HighlightAngleProperty);
            set => SetValue(HighlightAngleProperty, value);
        }

        public double HighlightFalloff
        {
            get => GetValue(HighlightFalloffProperty);
            set => SetValue(HighlightFalloffProperty, value);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateClipGeometry(finalSize);
            return result;
        }

        public override void Render(DrawingContext context)
        {
            if (LiquidGlassBackdropProvider.IsCapturing)
                return;

            if (Bounds.Width <= 0 || Bounds.Height <= 0)
                return;

            LiquidGlassBackdropProvider.EnsureSnapshot(this);
            var snapshot = LiquidGlassBackdropProvider.TryGetSnapshot(this);

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            var parameters = new LiquidGlassDrawParameters
            {
                CornerRadius = CornerRadius,
                RefractionHeight = RefractionHeight,
                RefractionAmount = RefractionAmount,
                DepthEffect = DepthEffect,
                ChromaticAberration = ChromaticAberration,
                BlurRadius = BlurRadius,
                Vibrancy = Vibrancy,
                TintColor = TintColor,
                SurfaceColor = SurfaceColor,
                HighlightEnabled = HighlightEnabled,
                HighlightWidth = HighlightWidth,
                HighlightBlurRadius = HighlightBlurRadius,
                HighlightOpacity = HighlightOpacity,
                HighlightAngleDegrees = HighlightAngle,
                HighlightFalloff = HighlightFalloff,
            };

            context.Custom(new LiquidGlassDrawOperation(bounds, parameters, snapshot, LiquidGlassDrawPass.Lens));

            if (HighlightEnabled)
                context.Custom(new LiquidGlassDrawOperation(bounds, parameters, snapshot: null, LiquidGlassDrawPass.Highlight));
        }

        private void UpdateClipGeometry()
        {
            UpdateClipGeometry(Bounds.Size);
        }

        private void UpdateClipGeometry(Size size)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                Clip = null;
                return;
            }

            Clip = CreateRoundRectGeometry(new Rect(size), CornerRadius);
        }

        private static Geometry CreateRoundRectGeometry(Rect rect, CornerRadius cornerRadius)
        {
            var width = rect.Width;
            var height = rect.Height;
            if (width <= 0 || height <= 0)
                return new RectangleGeometry(rect);

            var maxRadius = Math.Min(width, height) * 0.5;
            var tl = Clamp(cornerRadius.TopLeft, 0.0, maxRadius);
            var tr = Clamp(cornerRadius.TopRight, 0.0, maxRadius);
            var br = Clamp(cornerRadius.BottomRight, 0.0, maxRadius);
            var bl = Clamp(cornerRadius.BottomLeft, 0.0, maxRadius);

            var x = rect.X;
            var y = rect.Y;
            var right = rect.Right;
            var bottom = rect.Bottom;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.SetFillRule(FillRule.NonZero);

                ctx.BeginFigure(new Point(x + tl, y), isFilled: true);
                ctx.LineTo(new Point(right - tr, y));
                if (tr > 0.0)
                    ctx.ArcTo(new Point(right, y + tr), new Size(tr, tr), 0.0, isLargeArc: false, SweepDirection.Clockwise);

                ctx.LineTo(new Point(right, bottom - br));
                if (br > 0.0)
                    ctx.ArcTo(new Point(right - br, bottom), new Size(br, br), 0.0, isLargeArc: false, SweepDirection.Clockwise);

                ctx.LineTo(new Point(x + bl, bottom));
                if (bl > 0.0)
                    ctx.ArcTo(new Point(x, bottom - bl), new Size(bl, bl), 0.0, isLargeArc: false, SweepDirection.Clockwise);

                ctx.LineTo(new Point(x, y + tl));
                if (tl > 0.0)
                    ctx.ArcTo(new Point(x + tl, y), new Size(tl, tl), 0.0, isLargeArc: false, SweepDirection.Clockwise);

                ctx.EndFigure(isClosed: true);
            }

            return geometry;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
