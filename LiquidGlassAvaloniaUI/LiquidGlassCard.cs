using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// Legacy “liquid glass” card kept for compatibility.
    /// Prefer <see cref="LiquidGlassSurface"/> for the AndroidLiquidGlass-style pipeline.
    /// </summary>
    [Obsolete("Use LiquidGlassSurface. This control keeps legacy parameter names and is kept for compatibility.")]
    public class LiquidGlassCard : Control
    {
        #region Avalonia Properties

        /// <summary>
        /// 位移缩放强度
        /// </summary>
        public static readonly StyledProperty<double> DisplacementScaleProperty =
            AvaloniaProperty.Register<LiquidGlassCard, double>(nameof(DisplacementScale), 20.0);

        /// <summary>
        /// 模糊量
        /// </summary>
        public static readonly StyledProperty<double> BlurAmountProperty =
            AvaloniaProperty.Register<LiquidGlassCard, double>(nameof(BlurAmount), 0.15);

        /// <summary>
        /// 饱和度
        /// </summary>
        public static readonly StyledProperty<double> SaturationProperty =
            AvaloniaProperty.Register<LiquidGlassCard, double>(nameof(Saturation), 120.0);

        /// <summary>
        /// 色差强度
        /// </summary>
        public static readonly StyledProperty<double> AberrationIntensityProperty =
            AvaloniaProperty.Register<LiquidGlassCard, double>(nameof(AberrationIntensity), 7.0);

        /// <summary>
        /// 圆角半径
        /// </summary>
        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<LiquidGlassCard, double>(nameof(CornerRadius), 12.0);

        /// <summary>
        /// 液态玻璃效果模式
        /// </summary>
        public static readonly StyledProperty<LiquidGlassMode> ModeProperty =
            AvaloniaProperty.Register<LiquidGlassCard, LiquidGlassMode>(nameof(Mode), LiquidGlassMode.Standard);

        /// <summary>
        /// 是否在亮色背景上
        /// </summary>
        public static readonly StyledProperty<bool> OverLightProperty =
            AvaloniaProperty.Register<LiquidGlassCard, bool>(nameof(OverLight), false);

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

        public bool OverLight
        {
            get => GetValue(OverLightProperty);
            set => SetValue(OverLightProperty, value);
        }

        #endregion

        #region Border Gloss Effect

        private Point _lastMousePosition = new Point(0, 0);
        private bool _isMouseTracking = false;

        protected override void OnPointerEntered(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            _isMouseTracking = true;
            _lastMousePosition = e.GetPosition(this);
            InvalidateVisual();
        }

        protected override void OnPointerExited(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerExited(e);
            _isMouseTracking = false;
            InvalidateVisual();
        }

        protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isMouseTracking)
            {
                _lastMousePosition = e.GetPosition(this);
                InvalidateVisual();
            }
        }

        #endregion

        static LiquidGlassCard()
        {
            // 当任何属性变化时，触发重新渲染
            AffectsRender<LiquidGlassCard>(
                DisplacementScaleProperty,
                BlurAmountProperty,
                SaturationProperty,
                AberrationIntensityProperty,
                CornerRadiusProperty,
                ModeProperty,
                OverLightProperty
            );
        }

        public override void Render(DrawingContext context)
        {
            if (LiquidGlassBackdropProvider.IsCapturing)
                return;

            LiquidGlassBackdropProvider.EnsureSnapshot(this);
            var backdropSnapshot = LiquidGlassBackdropProvider.TryGetSnapshot(this);

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

            var parameters = new LiquidGlassDrawParameters
            {
                CornerRadius = new CornerRadius(CornerRadius),
                BackdropZoom = 1.0,
                BackdropOffset = new Vector(0.0, 0.0),
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

            context.Custom(new LiquidGlassDrawOperation(bounds, parameters, backdropSnapshot, LiquidGlassDrawPass.Lens));
            context.Custom(new LiquidGlassDrawOperation(bounds, parameters, snapshot: null, LiquidGlassDrawPass.Highlight));

            // 绘制边框光泽效果
            if (_isMouseTracking)
            {
                DrawBorderGloss(context, bounds);
            }
        }

        private void DrawBorderGloss(DrawingContext context, Rect bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // 计算鼠标相对于控件中心的角度
            var centerX = bounds.Width / 2;
            var centerY = bounds.Height / 2;
            var deltaX = _lastMousePosition.X - centerX;
            var deltaY = _lastMousePosition.Y - centerY;
            var angle = Math.Atan2(deltaY, deltaX);

            // 计算光泽应该出现的边框位置
            var glossLength = Math.Min(bounds.Width, bounds.Height) * 0.3; // 光泽长度
            var glossWidth = 3.0; // 光泽宽度

            // 根据鼠标位置决定光泽在哪条边上
            Point glossStart, glossEnd;
            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                // 光泽在左右边框
                if (deltaX > 0) // 鼠标在右侧，光泽在右边框
                {
                    var y = Math.Max(glossLength / 2, Math.Min(bounds.Height - glossLength / 2, _lastMousePosition.Y));
                    glossStart = new Point(bounds.Width - glossWidth / 2, y - glossLength / 2);
                    glossEnd = new Point(bounds.Width - glossWidth / 2, y + glossLength / 2);
                }
                else // 鼠标在左侧，光泽在左边框
                {
                    var y = Math.Max(glossLength / 2, Math.Min(bounds.Height - glossLength / 2, _lastMousePosition.Y));
                    glossStart = new Point(glossWidth / 2, y - glossLength / 2);
                    glossEnd = new Point(glossWidth / 2, y + glossLength / 2);
                }
            }
            else
            {
                // 光泽在上下边框
                if (deltaY > 0) // 鼠标在下方，光泽在下边框
                {
                    var x = Math.Max(glossLength / 2, Math.Min(bounds.Width - glossLength / 2, _lastMousePosition.X));
                    glossStart = new Point(x - glossLength / 2, bounds.Height - glossWidth / 2);
                    glossEnd = new Point(x + glossLength / 2, bounds.Height - glossWidth / 2);
                }
                else // 鼠标在上方，光泽在上边框
                {
                    var x = Math.Max(glossLength / 2, Math.Min(bounds.Width - glossLength / 2, _lastMousePosition.X));
                    glossStart = new Point(x - glossLength / 2, glossWidth / 2);
                    glossEnd = new Point(x + glossLength / 2, glossWidth / 2);
                }
            }

            // 创建光泽渐变
            var glossBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(glossStart.X / bounds.Width, glossStart.Y / bounds.Height, RelativeUnit.Relative),
                EndPoint = new RelativePoint(glossEnd.X / bounds.Width, glossEnd.Y / bounds.Height, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Colors.Transparent, 0.0),
                    new GradientStop(Color.FromArgb(100, 255, 255, 255), 0.5),
                    new GradientStop(Colors.Transparent, 1.0)
                }
            };

            // 绘制光泽线条
            var pen = new Pen(glossBrush, glossWidth);
            context.DrawLine(pen, glossStart, glossEnd);
        }
    }
}
