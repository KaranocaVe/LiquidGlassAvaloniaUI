using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// 可拖拽的液态玻璃卡片 - 悬浮在所有内容之上，支持鼠标拖拽移动
    /// </summary>
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
            // 监听所有属性变化并立即重新渲染
            PropertyChanged += OnPropertyChanged;
            
            // 监听DataContext变化，确保绑定生效后立即重新渲染
            PropertyChanged += (sender, args) =>
            {
                if (args.Property == DataContextProperty)
                {
                    Console.WriteLine($"[DraggableLiquidGlassCard] DataContext changed - forcing re-render");
                    // 延迟一下确保绑定完全生效
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => InvalidateVisual(), Avalonia.Threading.DispatcherPriority.Background);
                }
            };
            
            // 确保控件加载完成后立即重新渲染，以应用正确的初始参数
            this.Loaded += (sender, e) => 
            {
                Console.WriteLine($"[DraggableLiquidGlassCard] Loaded event - forcing re-render with current values");
                InvalidateVisual();
            };
            
            // 在属性系统完全初始化后再次渲染
            this.AttachedToVisualTree += (sender, e) =>
            {
                Console.WriteLine($"[DraggableLiquidGlassCard] AttachedToVisualTree - forcing re-render");
                InvalidateVisual();
            };

            // 设置默认大小
            Width = 200;
            Height = 150;
            
            // 设置鼠标光标为手型，表示可拖拽
            Cursor = new Cursor(StandardCursorType.Hand);
        }

        private void OnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            // 强制立即重新渲染
            if (e.Property == DisplacementScaleProperty ||
                e.Property == BlurAmountProperty ||
                e.Property == SaturationProperty ||
                e.Property == AberrationIntensityProperty ||
                e.Property == CornerRadiusProperty ||
                e.Property == ModeProperty ||
                e.Property == OverLightProperty)
            {
                Console.WriteLine($"[DraggableLiquidGlassCard] Property {e.Property.Name} changed from {e.OldValue} to {e.NewValue}");
                InvalidateVisual();
            }
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
                
                Console.WriteLine($"[DraggableLiquidGlassCard] Drag started at ({_dragStartX}, {_dragStartY})");
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
                
                Console.WriteLine($"[DraggableLiquidGlassCard] Dragging to ({X}, {Y})");
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
                
                Console.WriteLine($"[DraggableLiquidGlassCard] Drag ended at ({X}, {Y})");
            }
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            
            if (_isDragging)
            {
                _isDragging = false;
                Cursor = new Cursor(StandardCursorType.Hand);
                Console.WriteLine($"[DraggableLiquidGlassCard] Drag cancelled");
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
            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

            // 调试：输出当前参数值
            Console.WriteLine($"[DraggableLiquidGlassCard] Rendering at ({X}, {Y}) with DisplacementScale={DisplacementScale}, Saturation={Saturation}");

            // 创建液态玻璃效果参数 - 悬浮卡片模式
            var parameters = new LiquidGlassParameters
            {
                DisplacementScale = DisplacementScale,
                BlurAmount = BlurAmount,
                Saturation = Saturation,
                AberrationIntensity = AberrationIntensity,
                Elasticity = 0.0, // 悬浮卡片不需要弹性效果
                CornerRadius = CornerRadius,
                Mode = Mode,
                IsHovered = _isDragging, // 拖拽时显示悬停效果
                IsActive = false,
                OverLight = OverLight,
                MouseOffsetX = 0.0, // 静态位置
                MouseOffsetY = 0.0, // 静态位置
                GlobalMouseX = 0.0,
                GlobalMouseY = 0.0,
                ActivationZone = 0.0 // 无激活区域
            };

            // 静态渲染，无变换
            context.Custom(new LiquidGlassDrawOperation(bounds, parameters));
        }
    }
}
