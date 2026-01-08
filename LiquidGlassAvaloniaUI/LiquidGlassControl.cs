using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.IO;
using System.Numerics;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// 液态玻璃效果模式枚举
    /// </summary>
    public enum LiquidGlassMode
    {
        Standard,   // 标准径向位移模式
        Polar,      // 极坐标位移模式
        Prominent,  // 突出边缘位移模式
        Shader      // 实时生成的 Shader 位移模式
    }

    /// <summary>
    /// Legacy “liquid glass” control kept for compatibility.
    /// Prefer <see cref="LiquidGlassSurface"/> for the AndroidLiquidGlass-style pipeline.
    /// </summary>
    [Obsolete("Use LiquidGlassSurface. This control keeps legacy parameter names and is kept for compatibility.")]
    public class LiquidGlassControl : Control
    {
        #region Avalonia Properties

        /// <summary>
        /// 位移缩放强度 (对应 TS 版本的 displacementScale)
        /// </summary>
        public static readonly StyledProperty<double> DisplacementScaleProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(DisplacementScale), 70.0);

        /// <summary>
        /// 模糊量 (对应 TS 版本的 blurAmount)
        /// </summary>
        public static readonly StyledProperty<double> BlurAmountProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(BlurAmount), 0.0625);

        /// <summary>
        /// 饱和度 (对应 TS 版本的 saturation)
        /// </summary>
        public static readonly StyledProperty<double> SaturationProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(Saturation), 140.0);

        /// <summary>
        /// 色差强度 (对应 TS 版本的 aberrationIntensity)
        /// </summary>
        public static readonly StyledProperty<double> AberrationIntensityProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(AberrationIntensity), 2.0);

        /// <summary>
        /// 弹性系数 (对应 TS 版本的 elasticity)
        /// </summary>
        public static readonly StyledProperty<double> ElasticityProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(Elasticity), 0.15);

        /// <summary>
        /// 圆角半径 (对应 TS 版本的 cornerRadius)
        /// </summary>
        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(CornerRadius), 999.0);

        /// <summary>
        /// 液态玻璃效果模式
        /// </summary>
        public static readonly StyledProperty<LiquidGlassMode> ModeProperty =
            AvaloniaProperty.Register<LiquidGlassControl, LiquidGlassMode>(nameof(Mode), LiquidGlassMode.Standard);

        /// <summary>
        /// 是否处于悬停状态
        /// </summary>
        public static readonly StyledProperty<bool> IsHoveredProperty =
            AvaloniaProperty.Register<LiquidGlassControl, bool>(nameof(IsHovered), false);

        /// <summary>
        /// 是否处于激活状态
        /// </summary>
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<LiquidGlassControl, bool>(nameof(IsActive), false);

        /// <summary>
        /// 是否在亮色背景上
        /// </summary>
        public static readonly StyledProperty<bool> OverLightProperty =
            AvaloniaProperty.Register<LiquidGlassControl, bool>(nameof(OverLight), false);

        /// <summary>
        /// 鼠标相对偏移 X (百分比)
        /// </summary>
        public static readonly StyledProperty<double> MouseOffsetXProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(MouseOffsetX), 0.0);

        /// <summary>
        /// 鼠标相对偏移 Y (百分比)
        /// </summary>
        public static readonly StyledProperty<double> MouseOffsetYProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(MouseOffsetY), 0.0);

        /// <summary>
        /// 全局鼠标位置 X
        /// </summary>
        public static readonly StyledProperty<double> GlobalMouseXProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(GlobalMouseX), 0.0);

        /// <summary>
        /// 全局鼠标位置 Y
        /// </summary>
        public static readonly StyledProperty<double> GlobalMouseYProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(GlobalMouseY), 0.0);

        /// <summary>
        /// 激活区域距离 (像素)
        /// </summary>
        public static readonly StyledProperty<double> ActivationZoneProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(ActivationZone), 200.0);

        #endregion

        #region Properties

        public double DisplacementScale
        {
            get => GetValue(DisplacementScaleProperty);
            set => SetValue(DisplacementScaleProperty, value);
        }

        public double BlurAmount
        {
            get => GetValue(BlurAmountProperty);
            set => SetValue(BlurAmountProperty, value);
        }

        public double Saturation
        {
            get => GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public double AberrationIntensity
        {
            get => GetValue(AberrationIntensityProperty);
            set => SetValue(AberrationIntensityProperty, value);
        }

        public double Elasticity
        {
            get => GetValue(ElasticityProperty);
            set => SetValue(ElasticityProperty, value);
        }

        public double CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public LiquidGlassMode Mode
        {
            get => GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public bool IsHovered
        {
            get => GetValue(IsHoveredProperty);
            set => SetValue(IsHoveredProperty, value);
        }

        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public bool OverLight
        {
            get => GetValue(OverLightProperty);
            set => SetValue(OverLightProperty, value);
        }

        public double MouseOffsetX
        {
            get => GetValue(MouseOffsetXProperty);
            set => SetValue(MouseOffsetXProperty, value);
        }

        public double MouseOffsetY
        {
            get => GetValue(MouseOffsetYProperty);
            set => SetValue(MouseOffsetYProperty, value);
        }

        public double GlobalMouseX
        {
            get => GetValue(GlobalMouseXProperty);
            set => SetValue(GlobalMouseXProperty, value);
        }

        public double GlobalMouseY
        {
            get => GetValue(GlobalMouseYProperty);
            set => SetValue(GlobalMouseYProperty, value);
        }

        public double ActivationZone
        {
            get => GetValue(ActivationZoneProperty);
            set => SetValue(ActivationZoneProperty, value);
        }

        #endregion

        static LiquidGlassControl()
        {
            // 当任何属性变化时，触发重新渲染
            AffectsRender<LiquidGlassControl>(
                DisplacementScaleProperty,
                BlurAmountProperty,
                SaturationProperty,
                AberrationIntensityProperty,
                ElasticityProperty,
                CornerRadiusProperty,
                ModeProperty,
                IsHoveredProperty,
                IsActiveProperty,
                OverLightProperty,
                MouseOffsetXProperty,
                MouseOffsetYProperty,
                GlobalMouseXProperty,
                GlobalMouseYProperty,
                ActivationZoneProperty
            );
        }

        protected override void OnPointerEntered(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            IsHovered = true;
            UpdateMousePosition(e.GetPosition(this));
        }

        protected override void OnPointerExited(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerExited(e);
            IsHovered = false;
        }

        protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            UpdateMousePosition(e.GetPosition(this));
        }

        protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            IsActive = true;
        }

        protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            IsActive = false;
        }

        /// <summary>
        /// 更新鼠标位置并计算相对偏移
        /// </summary>
        private void UpdateMousePosition(Point position)
        {
            var centerX = Bounds.Width / 2;
            var centerY = Bounds.Height / 2;

            // 计算相对偏移 (百分比)
            MouseOffsetX = ((position.X - centerX) / Bounds.Width) * 100;
            MouseOffsetY = ((position.Y - centerY) / Bounds.Height) * 100;

            // 设置全局鼠标位置（相对于控件）
            GlobalMouseX = position.X;
            GlobalMouseY = position.Y;
        }

        /// <summary>
        /// 计算淡入因子（基于鼠标距离元素边缘的距离）
        /// </summary>
        private double CalculateFadeInFactor()
        {
            if (GlobalMouseX == 0 && GlobalMouseY == 0) return 0;

            var centerX = Bounds.Width / 2;
            var centerY = Bounds.Height / 2;
            var pillWidth = Bounds.Width;
            var pillHeight = Bounds.Height;

            var edgeDistanceX = Math.Max(0, Math.Abs(GlobalMouseX - centerX) - pillWidth / 2);
            var edgeDistanceY = Math.Max(0, Math.Abs(GlobalMouseY - centerY) - pillHeight / 2);
            var edgeDistance = Math.Sqrt(edgeDistanceX * edgeDistanceX + edgeDistanceY * edgeDistanceY);

            return edgeDistance > ActivationZone ? 0 : 1 - edgeDistance / ActivationZone;
        }

        /// <summary>
        /// 计算方向性缩放变换
        /// </summary>
        private (double scaleX, double scaleY) CalculateDirectionalScale()
        {
            if (GlobalMouseX == 0 && GlobalMouseY == 0) return (1.0, 1.0);

            var centerX = Bounds.Width / 2;
            var centerY = Bounds.Height / 2;
            var deltaX = GlobalMouseX - centerX;
            var deltaY = GlobalMouseY - centerY;

            var centerDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (centerDistance == 0) return (1.0, 1.0);

            var normalizedX = deltaX / centerDistance;
            var normalizedY = deltaY / centerDistance;
            var fadeInFactor = CalculateFadeInFactor();
            var stretchIntensity = Math.Min(centerDistance / 300, 1) * Elasticity * fadeInFactor;

            // X轴缩放：左右移动时水平拉伸，上下移动时压缩
            var scaleX = 1 + Math.Abs(normalizedX) * stretchIntensity * 0.3 - Math.Abs(normalizedY) * stretchIntensity * 0.15;

            // Y轴缩放：上下移动时垂直拉伸，左右移动时压缩
            var scaleY = 1 + Math.Abs(normalizedY) * stretchIntensity * 0.3 - Math.Abs(normalizedX) * stretchIntensity * 0.15;

            return (Math.Max(0.8, scaleX), Math.Max(0.8, scaleY));
        }

        /// <summary>
        /// 计算弹性位移
        /// </summary>
        private (double x, double y) CalculateElasticTranslation()
        {
            var fadeInFactor = CalculateFadeInFactor();
            var centerX = Bounds.Width / 2;
            var centerY = Bounds.Height / 2;

            return (
                (GlobalMouseX - centerX) * Elasticity * 0.1 * fadeInFactor,
                (GlobalMouseY - centerY) * Elasticity * 0.1 * fadeInFactor
            );
        }

        public override void Render(DrawingContext context)
        {
            if (LiquidGlassBackdropProvider.IsCapturing)
                return;

            LiquidGlassBackdropProvider.EnsureSnapshot(this);
            var backdropSnapshot = LiquidGlassBackdropProvider.TryGetSnapshot(this);

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

            // Legacy control: map old property names to AndroidLiquidGlass-style pipeline parameters.
            var parameters = new LiquidGlassDrawParameters
            {
                CornerRadius = new CornerRadius(CornerRadius),
                RefractionHeight = 12.0,
                RefractionAmount = DisplacementScale,
                DepthEffect = Mode == LiquidGlassMode.Prominent,
                ChromaticAberration = AberrationIntensity > 0.001,
                BlurRadius = BlurAmount,
                Vibrancy = Saturation / 100.0,
                Brightness = 0.0,
                Contrast = 1.0,
                ExposureEv = 0.0,
                GammaPower = 1.0,
                BackdropOpacity = 1.0,
                TintColor = Colors.Transparent,
                SurfaceColor = Colors.Transparent,
                HighlightEnabled = true,
                HighlightWidth = 0.5,
                HighlightBlurRadius = 0.25,
                HighlightOpacity = 0.5,
                HighlightAngleDegrees = 45.0,
                HighlightFalloff = 1.0,
            };

            // 计算变换
            var (scaleX, scaleY) = CalculateDirectionalScale();
            var (translateX, translateY) = CalculateElasticTranslation();

            // 应用变换
            using (context.PushTransform(Matrix.CreateScale(scaleX, scaleY) * Matrix.CreateTranslation(translateX, translateY)))
            {
                context.Custom(new LiquidGlassDrawOperation(bounds, parameters, backdropSnapshot, LiquidGlassDrawPass.Lens));
                context.Custom(new LiquidGlassDrawOperation(bounds, parameters, snapshot: null, LiquidGlassDrawPass.Highlight));
            }
        }
    }

    // NOTE: The old public LiquidGlassParameters struct was removed as part of the AndroidLiquidGlass-style rewrite.
}
