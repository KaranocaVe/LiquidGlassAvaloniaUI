using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// Legacy “liquid glass” draggable card kept for compatibility.
    /// Prefer <see cref="LiquidGlassSurface"/> for the AndroidLiquidGlass-style pipeline.
    /// </summary>
    [Obsolete("Use LiquidGlassSurface. This control keeps legacy parameter names and is kept for compatibility.")]
    public class DraggableLiquidGlassCard : Control
    {
        #region Avalonia Properties

        /// <summary>
        /// 位移缩放强度
        /// </summary>
        public static readonly StyledProperty<double> DisplacementScaleProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(DisplacementScale), 20.0);

        /// <summary>
        /// 模糊量
        /// </summary>
        public static readonly StyledProperty<double> BlurAmountProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(BlurAmount), 0.15);

        /// <summary>
        /// 饱和度
        /// </summary>
        public static readonly StyledProperty<double> SaturationProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(Saturation), 120.0);

        /// <summary>
        /// 色差强度
        /// </summary>
        public static readonly StyledProperty<double> AberrationIntensityProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(AberrationIntensity), 7.0);

        /// <summary>
        /// 圆角半径
        /// </summary>
        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(CornerRadius), 12.0);

        /// <summary>
        /// 液态玻璃效果模式
        /// </summary>
        public static readonly StyledProperty<LiquidGlassMode> ModeProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, LiquidGlassMode>(nameof(Mode), LiquidGlassMode.Standard);

        /// <summary>
        /// 是否在亮色背景上
        /// </summary>
        public static readonly StyledProperty<bool> OverLightProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, bool>(nameof(OverLight), false);

        /// <summary>
        /// X位置
        /// </summary>
        public static readonly StyledProperty<double> XProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(X), 100.0);

        /// <summary>
        /// Y位置
        /// </summary>
        public static readonly StyledProperty<double> YProperty =
            AvaloniaProperty.Register<DraggableLiquidGlassCard, double>(nameof(Y), 100.0);

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

        public double X
        {
            get => GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public double Y
        {
            get => GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        #endregion

        #region Drag State

        private bool _isDragging = false;
        private Point _dragStartPoint;
        private double _dragStartX;
        private double _dragStartY;

        #endregion

        static DraggableLiquidGlassCard()
        {
            // 当任何属性变化时，触发重新渲染
            AffectsRender<DraggableLiquidGlassCard>(
                DisplacementScaleProperty,
                BlurAmountProperty,
                SaturationProperty,
                AberrationIntensityProperty,
                CornerRadiusProperty,
                ModeProperty,
                OverLightProperty
            );

            // 位置变化时触发重新布局
            AffectsArrange<DraggableLiquidGlassCard>(XProperty, YProperty);
        }

        public DraggableLiquidGlassCard()
        {
            // 设置默认大小
            Width = 200;
            Height = 150;
            
            // 设置鼠标光标为手型，表示可拖拽
            Cursor = new Cursor(StandardCursorType.Hand);
        }

        #region Mouse Events for Dragging

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(Parent as Visual);
                _dragStartX = X;
                _dragStartY = Y;
                
                // 捕获指针，确保即使鼠标移出控件范围也能继续拖拽
                e.Pointer.Capture(this);
                
                // 改变光标为拖拽状态
                Cursor = new Cursor(StandardCursorType.SizeAll);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            
            if (_isDragging)
            {
                var currentPoint = e.GetPosition(Parent as Visual);
                var deltaX = currentPoint.X - _dragStartPoint.X;
                var deltaY = currentPoint.Y - _dragStartPoint.Y;
                
                X = _dragStartX + deltaX;
                Y = _dragStartY + deltaY;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            
            if (_isDragging)
            {
                _isDragging = false;
                e.Pointer.Capture(null);
                
                // 恢复光标为手型
                Cursor = new Cursor(StandardCursorType.Hand);
            }
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            
            if (_isDragging)
            {
                _isDragging = false;
                Cursor = new Cursor(StandardCursorType.Hand);
            }
        }

        #endregion

        protected override Size ArrangeOverride(Size finalSize)
        {
            // 使用X和Y属性来定位控件
            return base.ArrangeOverride(finalSize);
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
        }
    }
}
