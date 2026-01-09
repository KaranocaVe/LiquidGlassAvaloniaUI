using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// A composited “liquid glass” surface inspired by AndroidLiquidGlass’ backdrop pipeline:
    /// vibrancy + blur + lens refraction + (optional) highlights + shadows.
    /// </summary>
    public class LiquidGlassSurface : ContentControl
    {
        public static readonly StyledProperty<double> BackdropZoomProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(BackdropZoom), 1.0);

        public static readonly StyledProperty<Vector> BackdropOffsetProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Vector>(nameof(BackdropOffset), new Vector(0.0, 0.0));

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

        public static readonly StyledProperty<double> BrightnessProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(Brightness), 0.0);

        public static readonly StyledProperty<double> ContrastProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(Contrast), 1.0);

        public static readonly StyledProperty<double> ExposureEvProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(ExposureEv), 0.0);

        public static readonly StyledProperty<double> GammaPowerProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(GammaPower), 1.0);

        public static readonly StyledProperty<double> BackdropOpacityProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(BackdropOpacity), 1.0);

        public static readonly StyledProperty<Color> TintColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(TintColor), Colors.Transparent);

        public static readonly StyledProperty<Color> SurfaceColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(SurfaceColor), Colors.Transparent);

        public static readonly StyledProperty<bool> ProgressiveBlurEnabledProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(ProgressiveBlurEnabled), false);

        /// <summary>
        /// Progressive blur start position (0..1, relative to control height).
        /// Above this fraction the blur is fully visible.
        /// </summary>
        public static readonly StyledProperty<double> ProgressiveBlurStartProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(ProgressiveBlurStart), 0.5);

        /// <summary>
        /// Progressive blur end position (0..1, relative to control height).
        /// Below this fraction the blur fades out to fully transparent.
        /// </summary>
        public static readonly StyledProperty<double> ProgressiveBlurEndProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(ProgressiveBlurEnd), 1.0);

        public static readonly StyledProperty<Color> ProgressiveTintColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(ProgressiveTintColor), Colors.Transparent);

        public static readonly StyledProperty<double> ProgressiveTintIntensityProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(ProgressiveTintIntensity), 0.8);

        public static readonly StyledProperty<bool> AdaptiveLuminanceEnabledProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(AdaptiveLuminanceEnabled), false);

        public static readonly StyledProperty<double> AdaptiveLuminanceUpdateIntervalMsProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(AdaptiveLuminanceUpdateIntervalMs), 250.0);

        public static readonly StyledProperty<double> AdaptiveLuminanceSmoothingProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(AdaptiveLuminanceSmoothing), 0.2);

        public static readonly DirectProperty<LiquidGlassSurface, double> AdaptiveLuminanceProperty =
            AvaloniaProperty.RegisterDirect<LiquidGlassSurface, double>(
                nameof(AdaptiveLuminance),
                o => o.AdaptiveLuminance);

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

        public static readonly StyledProperty<bool> ShadowEnabledProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(ShadowEnabled), true);

        public static readonly StyledProperty<double> ShadowRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(ShadowRadius), 24.0);

        public static readonly StyledProperty<Vector> ShadowOffsetProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Vector>(nameof(ShadowOffset), new Vector(0.0, 4.0));

        public static readonly StyledProperty<Color> ShadowColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(ShadowColor), Color.FromArgb(26, 0, 0, 0));

        public static readonly StyledProperty<double> ShadowOpacityProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(ShadowOpacity), 1.0);

        public static readonly StyledProperty<bool> InnerShadowEnabledProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, bool>(nameof(InnerShadowEnabled), false);

        public static readonly StyledProperty<double> InnerShadowRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(InnerShadowRadius), 24.0);

        public static readonly StyledProperty<Vector> InnerShadowOffsetProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Vector>(nameof(InnerShadowOffset), new Vector(0.0, 24.0));

        public static readonly StyledProperty<Color> InnerShadowColorProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, Color>(nameof(InnerShadowColor), Color.FromArgb(38, 0, 0, 0));

        public static readonly StyledProperty<double> InnerShadowOpacityProperty =
            AvaloniaProperty.Register<LiquidGlassSurface, double>(nameof(InnerShadowOpacity), 1.0);

        static LiquidGlassSurface()
        {
            // TemplatedControl defaults ClipToBounds=true, which would clip the shadow pass to the control bounds.
            // We clip the content via the template's Border and clip shader passes explicitly.
            ClipToBoundsProperty.OverrideDefaultValue<LiquidGlassSurface>(false);

            AffectsRender<LiquidGlassSurface>(
                CornerRadiusProperty,
                BackdropZoomProperty,
                BackdropOffsetProperty,
                RefractionHeightProperty,
                RefractionAmountProperty,
                DepthEffectProperty,
                ChromaticAberrationProperty,
                BlurRadiusProperty,
                VibrancyProperty,
                BrightnessProperty,
                ContrastProperty,
                ExposureEvProperty,
                GammaPowerProperty,
                BackdropOpacityProperty,
                TintColorProperty,
                SurfaceColorProperty,
                ProgressiveBlurEnabledProperty,
                ProgressiveBlurStartProperty,
                ProgressiveBlurEndProperty,
                ProgressiveTintColorProperty,
                ProgressiveTintIntensityProperty,
                AdaptiveLuminanceEnabledProperty,
                AdaptiveLuminanceUpdateIntervalMsProperty,
                AdaptiveLuminanceSmoothingProperty,
                ShadowEnabledProperty,
                ShadowRadiusProperty,
                ShadowOffsetProperty,
                ShadowColorProperty,
                ShadowOpacityProperty);

            TemplateProperty.OverrideDefaultValue<LiquidGlassSurface>(CreateDefaultTemplate());
        }

        internal LiquidGlassInteractiveOverlay? InteractiveOverlay { get; private set; }
        internal LiquidGlassFrontOverlay? FrontOverlay { get; private set; }

        private DispatcherTimer? _adaptiveLuminanceTimer;
        private EventHandler? _adaptiveLuminanceTickHandler;
        private double _adaptiveLuminance;

        /// <summary>
        /// Last sampled backdrop luminance (0..1) when <see cref="AdaptiveLuminanceEnabled"/> is true.
        /// </summary>
        public double AdaptiveLuminance
        {
            get => _adaptiveLuminance;
            private set => SetAndRaise(AdaptiveLuminanceProperty, ref _adaptiveLuminance, value);
        }

        public Control? Child
        {
            get => Content as Control;
            set => Content = value;
        }

        /// <summary>
        /// Magnifier-style zoom applied to backdrop sampling (1 = no zoom).
        /// </summary>
        public double BackdropZoom
        {
            get => GetValue(BackdropZoomProperty);
            set => SetValue(BackdropZoomProperty, value);
        }

        /// <summary>
        /// Additional translation applied to the sampled backdrop (in DIPs).
        /// </summary>
        public Vector BackdropOffset
        {
            get => GetValue(BackdropOffsetProperty);
            set => SetValue(BackdropOffsetProperty, value);
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

        public double Saturation
        {
            get => Vibrancy;
            set => Vibrancy = value;
        }

        public double Brightness
        {
            get => GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        public double Contrast
        {
            get => GetValue(ContrastProperty);
            set => SetValue(ContrastProperty, value);
        }

        public double ExposureEv
        {
            get => GetValue(ExposureEvProperty);
            set => SetValue(ExposureEvProperty, value);
        }

        public double GammaPower
        {
            get => GetValue(GammaPowerProperty);
            set => SetValue(GammaPowerProperty, value);
        }

        public double BackdropOpacity
        {
            get => GetValue(BackdropOpacityProperty);
            set => SetValue(BackdropOpacityProperty, value);
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

        /// <summary>
        /// Enables the progressive blur mask stage (similar to AndroidLiquidGlass’ ProgressiveBlurContent).
        /// </summary>
        public bool ProgressiveBlurEnabled
        {
            get => GetValue(ProgressiveBlurEnabledProperty);
            set => SetValue(ProgressiveBlurEnabledProperty, value);
        }

        public double ProgressiveBlurStart
        {
            get => GetValue(ProgressiveBlurStartProperty);
            set => SetValue(ProgressiveBlurStartProperty, value);
        }

        public double ProgressiveBlurEnd
        {
            get => GetValue(ProgressiveBlurEndProperty);
            set => SetValue(ProgressiveBlurEndProperty, value);
        }

        public Color ProgressiveTintColor
        {
            get => GetValue(ProgressiveTintColorProperty);
            set => SetValue(ProgressiveTintColorProperty, value);
        }

        public double ProgressiveTintIntensity
        {
            get => GetValue(ProgressiveTintIntensityProperty);
            set => SetValue(ProgressiveTintIntensityProperty, value);
        }

        /// <summary>
        /// Enables adaptive luminance mode which samples the backdrop behind this surface and adjusts
        /// rendering parameters to improve legibility (inspired by AndroidLiquidGlass’ AdaptiveLuminanceGlassContent).
        /// </summary>
        public bool AdaptiveLuminanceEnabled
        {
            get => GetValue(AdaptiveLuminanceEnabledProperty);
            set => SetValue(AdaptiveLuminanceEnabledProperty, value);
        }

        public double AdaptiveLuminanceUpdateIntervalMs
        {
            get => GetValue(AdaptiveLuminanceUpdateIntervalMsProperty);
            set => SetValue(AdaptiveLuminanceUpdateIntervalMsProperty, value);
        }

        public double AdaptiveLuminanceSmoothing
        {
            get => GetValue(AdaptiveLuminanceSmoothingProperty);
            set => SetValue(AdaptiveLuminanceSmoothingProperty, value);
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

        public bool ShadowEnabled
        {
            get => GetValue(ShadowEnabledProperty);
            set => SetValue(ShadowEnabledProperty, value);
        }

        public double ShadowRadius
        {
            get => GetValue(ShadowRadiusProperty);
            set => SetValue(ShadowRadiusProperty, value);
        }

        public Vector ShadowOffset
        {
            get => GetValue(ShadowOffsetProperty);
            set => SetValue(ShadowOffsetProperty, value);
        }

        public Color ShadowColor
        {
            get => GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public double ShadowOpacity
        {
            get => GetValue(ShadowOpacityProperty);
            set => SetValue(ShadowOpacityProperty, value);
        }

        public bool InnerShadowEnabled
        {
            get => GetValue(InnerShadowEnabledProperty);
            set => SetValue(InnerShadowEnabledProperty, value);
        }

        public double InnerShadowRadius
        {
            get => GetValue(InnerShadowRadiusProperty);
            set => SetValue(InnerShadowRadiusProperty, value);
        }

        public Vector InnerShadowOffset
        {
            get => GetValue(InnerShadowOffsetProperty);
            set => SetValue(InnerShadowOffsetProperty, value);
        }

        public Color InnerShadowColor
        {
            get => GetValue(InnerShadowColorProperty);
            set => SetValue(InnerShadowColorProperty, value);
        }

        public double InnerShadowOpacity
        {
            get => GetValue(InnerShadowOpacityProperty);
            set => SetValue(InnerShadowOpacityProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            if (LiquidGlassBackdropProvider.IsCapturing)
                return;

            if (Bounds.Width <= 0 || Bounds.Height <= 0)
                return;

            var parameters = CreateDrawParameters();
            var controlBounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

            if (ShadowEnabled && ShadowOpacity > 0.001 && ShadowColor.A > 0 && ShadowRadius > 0.001)
                context.Custom(new LiquidGlassShadowDrawOperation(controlBounds, parameters));

            LiquidGlassBackdropProvider.EnsureSnapshot(this);
            var snapshot = LiquidGlassBackdropProvider.TryGetSnapshot(this);

            context.Custom(new LiquidGlassDrawOperation(controlBounds, parameters, snapshot, LiquidGlassDrawPass.Lens));
        }

        internal LiquidGlassDrawParameters CreateDrawParameters()
        {
            var parameters = new LiquidGlassDrawParameters
            {
                CornerRadius = CornerRadius,
                BackdropZoom = BackdropZoom,
                BackdropOffset = BackdropOffset,
                RefractionHeight = RefractionHeight,
                RefractionAmount = RefractionAmount,
                DepthEffect = DepthEffect,
                ChromaticAberration = ChromaticAberration,
                BlurRadius = BlurRadius,
                Vibrancy = Vibrancy,
                Brightness = Brightness,
                Contrast = Contrast,
                ExposureEv = ExposureEv,
                GammaPower = GammaPower,
                BackdropOpacity = BackdropOpacity,
                TintColor = TintColor,
                SurfaceColor = SurfaceColor,
                ProgressiveBlurEnabled = ProgressiveBlurEnabled,
                ProgressiveBlurStart = ProgressiveBlurStart,
                ProgressiveBlurEnd = ProgressiveBlurEnd,
                ProgressiveTintColor = ProgressiveTintColor,
                ProgressiveTintIntensity = ProgressiveTintIntensity,
                HighlightEnabled = HighlightEnabled,
                HighlightWidth = HighlightWidth,
                HighlightBlurRadius = HighlightBlurRadius,
                HighlightOpacity = HighlightOpacity,
                HighlightAngleDegrees = HighlightAngle,
                HighlightFalloff = HighlightFalloff,
                ShadowEnabled = ShadowEnabled,
                ShadowRadius = ShadowRadius,
                ShadowOffset = ShadowOffset,
                ShadowColor = ShadowColor,
                ShadowOpacity = ShadowOpacity,
                InnerShadowEnabled = InnerShadowEnabled,
                InnerShadowRadius = InnerShadowRadius,
                InnerShadowOffset = InnerShadowOffset,
                InnerShadowColor = InnerShadowColor,
                InnerShadowOpacity = InnerShadowOpacity,
            };
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InteractiveOverlay = e.NameScope.Find<LiquidGlassInteractiveOverlay>("PART_InteractiveOverlay");
            FrontOverlay = e.NameScope.Find<LiquidGlassFrontOverlay>("PART_FrontOverlay");
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CornerRadiusProperty)
            {
                InteractiveOverlay?.InvalidateVisual();
                FrontOverlay?.InvalidateVisual();
                return;
            }

            if (change.Property == HighlightEnabledProperty
                || change.Property == HighlightWidthProperty
                || change.Property == HighlightBlurRadiusProperty
                || change.Property == HighlightOpacityProperty
                || change.Property == HighlightAngleProperty
                || change.Property == HighlightFalloffProperty
                || change.Property == InnerShadowEnabledProperty
                || change.Property == InnerShadowRadiusProperty
                || change.Property == InnerShadowOffsetProperty
                || change.Property == InnerShadowColorProperty
                || change.Property == InnerShadowOpacityProperty)
            {
                FrontOverlay?.InvalidateVisual();
            }
        }

        private static FuncControlTemplate CreateDefaultTemplate()
        {
            return new FuncControlTemplate<LiquidGlassSurface>((_, ns) =>
            {
                var interactiveOverlay = new LiquidGlassInteractiveOverlay
                {
                    Name = "PART_InteractiveOverlay",
                    IsHitTestVisible = false
                }.RegisterInNameScope(ns);

                var presenter = new ContentPresenter
                {
                    Name = "PART_ContentPresenter",
                    [~ContentPresenter.ContentProperty] = new TemplateBinding(ContentProperty),
                    [~ContentPresenter.ContentTemplateProperty] = new TemplateBinding(ContentTemplateProperty),
                    [~ContentPresenter.VerticalContentAlignmentProperty] = new TemplateBinding(VerticalContentAlignmentProperty),
                    [~ContentPresenter.HorizontalContentAlignmentProperty] = new TemplateBinding(HorizontalContentAlignmentProperty)
                }.RegisterInNameScope(ns);

                var frontOverlay = new LiquidGlassFrontOverlay
                {
                    Name = "PART_FrontOverlay",
                    IsHitTestVisible = false
                }.RegisterInNameScope(ns);

                var grid = new Grid();
                grid.Children.Add(interactiveOverlay);
                grid.Children.Add(presenter);
                grid.Children.Add(frontOverlay);

                return new Border
                {
                    ClipToBounds = true,
                    [~Border.CornerRadiusProperty] = new TemplateBinding(CornerRadiusProperty),
                    Child = grid
                };
            });
        }
    }
}
