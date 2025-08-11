using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// 液态玻璃按钮控件 - 完整的鼠标交互响应
    /// </summary>
    public class LiquidGlassButton : Control
    {
        #region Avalonia Properties

        /// <summary>
        /// 位移缩放强度
        /// </summary>
        public static readonly StyledProperty<double> DisplacementScaleProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(DisplacementScale), 20.0);

        /// <summary>
        /// 模糊量
        /// </summary>
        public static readonly StyledProperty<double> BlurAmountProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(BlurAmount), 0.15);

        /// <summary>
        /// 饱和度
        /// </summary>
        public static readonly StyledProperty<double> SaturationProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(Saturation), 120.0);

        /// <summary>
        /// 色差强度
        /// </summary>
        public static readonly StyledProperty<double> AberrationIntensityProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(AberrationIntensity), 7.0);

        /// <summary>
        /// 弹性系数
        /// </summary>
        public static readonly StyledProperty<double> ElasticityProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(Elasticity), 0.15);

        /// <summary>
        /// 圆角半径
        /// </summary>
        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(CornerRadius), 999.0);

        /// <summary>
        /// 液态玻璃效果模式
        /// </summary>
        public static readonly StyledProperty<LiquidGlassMode> ModeProperty =
            AvaloniaProperty.Register<LiquidGlassButton, LiquidGlassMode>(nameof(Mode), LiquidGlassMode.Standard);

        /// <summary>
        /// 是否处于悬停状态
        /// </summary>
        public static readonly StyledProperty<bool> IsHoveredProperty =
            AvaloniaProperty.Register<LiquidGlassButton, bool>(nameof(IsHovered), false);

        /// <summary>
        /// 是否处于激活状态
        /// </summary>
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<LiquidGlassButton, bool>(nameof(IsActive), false);

        /// <summary>
        /// 是否在亮色背景上
        /// </summary>
        public static readonly StyledProperty<bool> OverLightProperty =
            AvaloniaProperty.Register<LiquidGlassButton, bool>(nameof(OverLight), false);

        /// <summary>
        /// 鼠标相对偏移 X (百分比)
        /// </summary>
        public static readonly StyledProperty<double> MouseOffsetXProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(MouseOffsetX), 0.0);

        /// <summary>
        /// 鼠标相对偏移 Y (百分比)
        /// </summary>
        public static readonly StyledProperty<double> MouseOffsetYProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(MouseOffsetY), 0.0);

        /// <summary>
        /// 全局鼠标位置 X
        /// </summary>
        public static readonly StyledProperty<double> GlobalMouseXProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(GlobalMouseX), 0.0);

        /// <summary>
        /// 全局鼠标位置 Y
        /// </summary>
        public static readonly StyledProperty<double> GlobalMouseYProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(GlobalMouseY), 0.0);

        /// <summary>
        /// 激活区域距离 (像素)
        /// </summary>
        public static readonly StyledProperty<double> ActivationZoneProperty =
            AvaloniaProperty.Register<LiquidGlassButton, double>(nameof(ActivationZone), 200.0);

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

        static LiquidGlassButton()
        {
            // 当任何属性变化时，触发重新渲染
            AffectsRender<LiquidGlassButton>(
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

        public LiquidGlassButton()
        {
            // 监听所有属性变化并立即重新渲染
            PropertyChanged += OnPropertyChanged;
            
            // 监听DataContext变化，确保绑定生效后立即重新渲染
            PropertyChanged += (sender, args) =>
            {
                if (args.Property == DataContextProperty)
                {
                    Console.WriteLine($"[LiquidGlassButton] DataContext changed - forcing re-render");
                    // 延迟一下确保绑定完全生效
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => InvalidateVisual(), Avalonia.Threading.DispatcherPriority.Background);
                }
            };
            
            // 确保控件加载完成后立即重新渲染，以应用正确的初始参数
            this.Loaded += (sender, e) => 
            {
                Console.WriteLine($"[LiquidGlassButton] Loaded event - forcing re-render with current values");
                InvalidateVisual();
            };
            
            // 在属性系统完全初始化后再次渲染
            this.AttachedToVisualTree += (sender, e) =>
            {
                Console.WriteLine($"[LiquidGlassButton] AttachedToVisualTree - forcing re-render");
                InvalidateVisual();
            };
        }

        private void OnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            // 强制立即重新渲染
            if (e.Property == DisplacementScaleProperty ||
                e.Property == BlurAmountProperty ||
                e.Property == SaturationProperty ||
                e.Property == AberrationIntensityProperty ||
                e.Property == ElasticityProperty ||
                e.Property == CornerRadiusProperty ||
                e.Property == ModeProperty ||
                e.Property == IsHoveredProperty ||
                e.Property == IsActiveProperty ||
                e.Property == OverLightProperty ||
                e.Property == MouseOffsetXProperty ||
                e.Property == MouseOffsetYProperty ||
                e.Property == GlobalMouseXProperty ||
                e.Property == GlobalMouseYProperty ||
                e.Property == ActivationZoneProperty)
            {
                Console.WriteLine($"[LiquidGlassButton] Property {e.Property.Name} changed from {e.OldValue} to {e.NewValue}");
                InvalidateVisual();
            }
        }

        #region Mouse Event Handlers

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
            // 鼠标离开时重置位置
            MouseOffsetX = 0.0;
            MouseOffsetY = 0.0;
            GlobalMouseX = 0.0;
            GlobalMouseY = 0.0;
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// 更新鼠标位置并计算相对偏移
        /// </summary>
        private void UpdateMousePosition(Point position)
        {
            if (Bounds.Width == 0 || Bounds.Height == 0) return;

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

        #endregion

        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

            // 创建液态玻璃效果参数
            var parameters = new LiquidGlassParameters
            {
                DisplacementScale = DisplacementScale,
                BlurAmount = BlurAmount,
                Saturation = Saturation,
                AberrationIntensity = AberrationIntensity,
                Elasticity = Elasticity,
                CornerRadius = CornerRadius,
                Mode = Mode,
                IsHovered = IsHovered,
                IsActive = IsActive,
                OverLight = OverLight,
                MouseOffsetX = MouseOffsetX,
                MouseOffsetY = MouseOffsetY,
                GlobalMouseX = GlobalMouseX,
                GlobalMouseY = GlobalMouseY,
                ActivationZone = ActivationZone
            };

            // 计算变换
            var (scaleX, scaleY) = CalculateDirectionalScale();
            var (translateX, translateY) = CalculateElasticTranslation();

            // 应用变换
            using (context.PushTransform(Matrix.CreateScale(scaleX, scaleY) * Matrix.CreateTranslation(translateX, translateY)))
            {
                context.Custom(new LiquidGlassDrawOperation(bounds, parameters));
            }
        }
    }
}
