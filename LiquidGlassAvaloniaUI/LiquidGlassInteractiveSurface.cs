using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// An interactive variant of <see cref="LiquidGlassSurface"/> that replicates AndroidLiquidGlass'
    /// press/drag deformation and interactive highlight overlay.
    /// </summary>
    public class LiquidGlassInteractiveSurface : LiquidGlassSurface
    {
        public static readonly StyledProperty<bool> IsInteractiveProperty =
            AvaloniaProperty.Register<LiquidGlassInteractiveSurface, bool>(nameof(IsInteractive), true);

        public static readonly StyledProperty<bool> InteractiveHighlightEnabledProperty =
            AvaloniaProperty.Register<LiquidGlassInteractiveSurface, bool>(nameof(InteractiveHighlightEnabled), true);

        public static readonly StyledProperty<double> InteractiveMaxScaleDipProperty =
            AvaloniaProperty.Register<LiquidGlassInteractiveSurface, double>(nameof(InteractiveMaxScaleDip), 4.0);

        static LiquidGlassInteractiveSurface()
        {
            AffectsRender<LiquidGlassInteractiveSurface>(
                IsInteractiveProperty,
                InteractiveHighlightEnabledProperty,
                InteractiveMaxScaleDipProperty);
        }

        private const double SpringDampingRatio = 0.5;
        private const double SpringStiffness = 300.0;
        private const double SpringProgressThreshold = 0.001;
        private const double InitialDerivative = 0.05;

        private readonly DispatcherTimer _animationTimer;
        private DateTime _lastAnimationTickUtc;

        private int? _activePointerId;
        private TopLevel? _hookedTopLevel;

        private Point _startPosition;
        private SpringDouble _pressProgress = new(SpringStiffness, SpringDampingRatio, SpringProgressThreshold);
        private SpringPoint _position = new(SpringStiffness, SpringDampingRatio, 0.5);

        public LiquidGlassInteractiveSurface()
        {
            RenderTransformOrigin = RelativePoint.Center;

            _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _animationTimer.Tick += (_, _) =>
            {
                var now = DateTime.UtcNow;
                var dt = (now - _lastAnimationTickUtc).TotalSeconds;
                _lastAnimationTickUtc = now;

                // Clamp dt to avoid huge steps on debugger stops / tab switches.
                dt = Math.Max(0.0, Math.Min(0.05, dt));

                var any = false;
                any |= _pressProgress.Step(dt);
                any |= _position.Step(dt);

                ApplyDeformation();
                InvalidateVisual();
                InteractiveOverlay?.InvalidateVisual();

                if (!any)
                    _animationTimer.Stop();
            };

            AddHandler(PointerPressedEvent, OnSelfPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        }

        public bool IsInteractive
        {
            get => GetValue(IsInteractiveProperty);
            set => SetValue(IsInteractiveProperty, value);
        }

        public bool InteractiveHighlightEnabled
        {
            get => GetValue(InteractiveHighlightEnabledProperty);
            set => SetValue(InteractiveHighlightEnabledProperty, value);
        }

        public double InteractiveMaxScaleDip
        {
            get => GetValue(InteractiveMaxScaleDipProperty);
            set => SetValue(InteractiveMaxScaleDipProperty, value);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            UnhookTopLevel();
            _activePointerId = null;
            _animationTimer.Stop();
        }

        private void OnSelfPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!IsInteractive)
                return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            if (_activePointerId is not null)
                return;

            _activePointerId = e.Pointer.Id;
            _startPosition = e.GetPosition(this);
            _position.SnapTo(_startPosition);

            _pressProgress.Target = 1.0;
            _position.Target = _startPosition;

            HookTopLevel();
            ApplyDeformation();
            InvalidateVisual();
            InteractiveOverlay?.InvalidateVisual();
            StartAnimation();
        }

        private void OnTopLevelPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_activePointerId is null || e.Pointer.Id != _activePointerId.Value)
                return;

            var p = e.GetPosition(this);
            _position.SnapTo(p);
            ApplyDeformation();
            InvalidateVisual();
            InteractiveOverlay?.InvalidateVisual();
            StartAnimation();
        }

        private void OnTopLevelPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_activePointerId is null || e.Pointer.Id != _activePointerId.Value)
                return;

            EndInteraction();
        }

        private void OnTopLevelPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            // Treat capture loss as cancellation.
            if (_activePointerId is null)
                return;

            EndInteraction();
        }

        private void EndInteraction()
        {
            _activePointerId = null;
            UnhookTopLevel();

            _pressProgress.Target = 0.0;
            _position.Target = _startPosition;
            ApplyDeformation();
            InvalidateVisual();
            InteractiveOverlay?.InvalidateVisual();
            StartAnimation();
        }

        private void HookTopLevel()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null || ReferenceEquals(topLevel, _hookedTopLevel))
                return;

            UnhookTopLevel();

            _hookedTopLevel = topLevel;
            topLevel.AddHandler(InputElement.PointerMovedEvent, OnTopLevelPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
            topLevel.AddHandler(InputElement.PointerReleasedEvent, OnTopLevelPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
            topLevel.AddHandler(InputElement.PointerCaptureLostEvent, OnTopLevelPointerCaptureLost, RoutingStrategies.Tunnel, handledEventsToo: true);
        }

        private void UnhookTopLevel()
        {
            if (_hookedTopLevel is null)
                return;

            _hookedTopLevel.RemoveHandler(InputElement.PointerMovedEvent, OnTopLevelPointerMoved);
            _hookedTopLevel.RemoveHandler(InputElement.PointerReleasedEvent, OnTopLevelPointerReleased);
            _hookedTopLevel.RemoveHandler(InputElement.PointerCaptureLostEvent, OnTopLevelPointerCaptureLost);
            _hookedTopLevel = null;
        }

        private void StartAnimation()
        {
            if (_animationTimer.IsEnabled)
                return;

            _lastAnimationTickUtc = DateTime.UtcNow;
            _animationTimer.Start();
        }

        private void ApplyDeformation()
        {
            if (!IsInteractive || Bounds.Width <= 0 || Bounds.Height <= 0)
            {
                RenderTransform = null;
                return;
            }

            var width = Bounds.Width;
            var height = Bounds.Height;

            var progress = Clamp(_pressProgress.Value, 0.0, 1.0);
            var maxScale = Math.Max(0.0, InteractiveMaxScaleDip);

            // Matches LiquidButton.kt layerBlock math (dp converted to px there; DIPs here).
            var scale = Lerp(1.0, 1.0 + maxScale / height, progress);

            var offset = _position.Value - _startPosition;

            var minDim = Math.Min(width, height);
            var maxDim = Math.Max(width, height);

            var tx = minDim * Math.Tanh(InitialDerivative * offset.X / minDim);
            var ty = minDim * Math.Tanh(InitialDerivative * offset.Y / minDim);

            var maxDragScale = maxScale / height;
            var angle = Math.Atan2(offset.Y, offset.X);

            var aspectX = Math.Min(width / height, 1.0);
            var aspectY = Math.Min(height / width, 1.0);

            var sx =
                scale +
                maxDragScale * Math.Abs(Math.Cos(angle) * offset.X / maxDim) * aspectX;

            var sy =
                scale +
                maxDragScale * Math.Abs(Math.Sin(angle) * offset.Y / maxDim) * aspectY;

            if (Math.Abs(tx) < 0.01 && Math.Abs(ty) < 0.01 && Math.Abs(sx - 1.0) < 0.0005 && Math.Abs(sy - 1.0) < 0.0005)
                RenderTransform = null;
            else
                RenderTransform = new MatrixTransform(Matrix.CreateScale(sx, sy) * Matrix.CreateTranslation(tx, ty));
        }

        internal double GetInteractiveHighlightProgress()
        {
            if (!IsInteractive || !InteractiveHighlightEnabled)
                return 0.0;

            return Clamp(_pressProgress.Value, 0.0, 1.0);
        }

        internal Point GetInteractiveHighlightPosition() => _position.Value;

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsInteractiveProperty
                || change.Property == InteractiveHighlightEnabledProperty
                || change.Property == InteractiveMaxScaleDipProperty)
            {
                InteractiveOverlay?.InvalidateVisual();
            }
        }

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private sealed class SpringDouble
        {
            private readonly double _stiffness;
            private readonly double _dampingRatio;
            private readonly double _threshold;

            public SpringDouble(double stiffness, double dampingRatio, double threshold)
            {
                _stiffness = stiffness;
                _dampingRatio = dampingRatio;
                _threshold = threshold;
            }

            public double Value { get; private set; }
            public double Velocity { get; private set; }
            public double Target { get; set; }

            public void SnapTo(double value)
            {
                Value = value;
                Velocity = 0.0;
            }

            public bool Step(double dt)
            {
                var x = Value;
                var v = Velocity;
                var target = Target;

                var k = _stiffness;
                var c = 2.0 * _dampingRatio * Math.Sqrt(k);

                var a = -k * (x - target) - c * v;
                v += a * dt;
                x += v * dt;

                Value = x;
                Velocity = v;

                var done = Math.Abs(x - target) <= _threshold && Math.Abs(v) <= _threshold;
                if (done)
                {
                    Value = target;
                    Velocity = 0.0;
                }

                return !done;
            }
        }

        private sealed class SpringPoint
        {
            private readonly SpringDouble _x;
            private readonly SpringDouble _y;

            public SpringPoint(double stiffness, double dampingRatio, double positionThreshold)
            {
                _x = new SpringDouble(stiffness, dampingRatio, positionThreshold);
                _y = new SpringDouble(stiffness, dampingRatio, positionThreshold);
            }

            public Point Value => new Point(_x.Value, _y.Value);

            public Point Target
            {
                get => new Point(_x.Target, _y.Target);
                set
                {
                    _x.Target = value.X;
                    _y.Target = value.Y;
                }
            }

            public void SnapTo(Point value)
            {
                _x.SnapTo(value.X);
                _y.SnapTo(value.Y);
            }

            public bool Step(double dt)
            {
                return _x.Step(dt) | _y.Step(dt);
            }
        }
    }
}
