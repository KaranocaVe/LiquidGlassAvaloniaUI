using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Avalonia.VisualTree;
using SkiaSharp;
using System;
using System.IO;
using System.Numerics;
using Point = Avalonia.Point;
using Rect = Avalonia.Rect;

namespace LiquidGlassAvaloniaUI
{
    // Refactored to a Decorator
    public class FrostedGlassDecorator : Decorator
    {
        private CompositionCustomVisual? _customVisual;
        private readonly FrostedGlassVisualHandler _handler;

        public static readonly StyledProperty<double> RadiusProperty =
            AvaloniaProperty.Register<FrostedGlassDecorator, double>(nameof(Radius), 5.0);

        public double Radius
        {
            get => GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        static FrostedGlassDecorator()
        {
            AffectsRender<FrostedGlassDecorator>(RadiusProperty);
        }

        public FrostedGlassDecorator()
        {
            _handler = new FrostedGlassVisualHandler(this);

            // This trick is still important, as it ensures the Decorator is handled correctly by the rendering system.
            SetValue(Panel.BackgroundProperty, Brushes.Transparent);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // Subscribe to the layout updated event.
            LayoutUpdated += OnLayoutUpdated;

            var containerVisual = ElementComposition.GetElementVisual(this) as CompositionContainerVisual;
            var compositor = containerVisual?.Compositor;
            if (compositor is null || containerVisual is null) return;

            _customVisual = compositor.CreateCustomVisual(_handler);

            containerVisual.Children.InsertAtBottom(_customVisual);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            // Unsubscribe to prevent memory leaks.
            LayoutUpdated -= OnLayoutUpdated;

            var containerVisual = ElementComposition.GetElementVisual(this) as CompositionContainerVisual;
            if (_customVisual != null && containerVisual != null)
            {
                containerVisual.Children.Remove(_customVisual);
                _customVisual = null;
            }
        }

        /// <summary>
        /// Layout updated event handler.
        /// This is the key to solving the problem: we manually provide a size for the visual and force it to redraw here.
        /// </summary>
        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (_customVisual is null) return;

            // Key change: Provide an explicit, non-zero size for the custom visual.
            // If the size is (0,0), the compositor will optimize it away and OnRender will never be called.
            var newSize = new Vector2((float)Bounds.Width, (float)Bounds.Height);
            if (_customVisual.Size != newSize)
            {
                _customVisual.Size = newSize;
            }

            // Explicitly invalidate it to request a redraw.
            InvalidateVisual();
        }

        // The internal handler class remains unchanged.
        private class FrostedGlassVisualHandler : CompositionCustomVisualHandler
        {
            private readonly FrostedGlassDecorator _owner;
            private SKRuntimeEffect? _effect;
            private bool _isShaderLoaded;

            public FrostedGlassVisualHandler(FrostedGlassDecorator owner)
            {
                _owner = owner;
            }

            private void LoadShader()
            {
                if (_isShaderLoaded) return;
                _isShaderLoaded = true;

                try
                {
                    var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/FrostedGlassShader.sksl");
                    using var stream = AssetLoader.Open(assetUri);
                    using var reader = new StreamReader(stream);
                    var shaderCode = reader.ReadToEnd();

                    _effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
                    if (_effect == null)
                    {
                        Console.WriteLine($"Failed to create SKRuntimeEffect: {errorText}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred while loading the shader: {ex.Message}");
                }
            }

            public override void OnRender(ImmediateDrawingContext context)
            {
                LoadShader();

                var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
                if (leaseFeature is null) return;

                using var lease = leaseFeature.Lease();
                var canvas = lease.SkCanvas;
                canvas.Clear(SKColors.Transparent);

                if (_effect is null)
                {
                    using var errorPaint = new SKPaint
                    {
                        Color = new SKColor(255, 0, 0, 120),
                        Style = SKPaintStyle.Fill
                    };
                    canvas.DrawRect(SKRect.Create(0, 0, (float)_owner.Bounds.Width, (float)_owner.Bounds.Height), errorPaint);

                    using var textPaint = new SKPaint
                    {
                         Color = SKColors.White,
                         TextSize = 14,
                         IsAntialias = true,
                         TextAlign = SKTextAlign.Center
                    };
                    var errorMessage = "Shader failed to load!\nPlease check the file path and build action.";
                    canvas.DrawText(errorMessage, (float)_owner.Bounds.Width / 2, (float)_owner.Bounds.Height / 2, textPaint);
                    return;
                }

                var topLevel = _owner.GetVisualRoot() as TopLevel;
                if (topLevel is null) return;

                var controlBounds = _owner.Bounds;
                var controlPositionInTopLevel = _owner.TranslatePoint(new Point(0, 0), topLevel);
                if (!controlPositionInTopLevel.HasValue) return;

                var pixelSize = new PixelSize(
                    (int)(controlBounds.Width * lease.SkCanvas.TotalMatrix.ScaleX),
                    (int)(controlBounds.Height * lease.SkCanvas.TotalMatrix.ScaleY)
                );

                if (pixelSize.Width <= 0 || pixelSize.Height <= 0) return;

                using var backgroundBitmap = new RenderTargetBitmap(pixelSize);

                using (var backgroundContext = backgroundBitmap.CreateDrawingContext())
                {
                    // When capturing the background, we need to hide not only this control but also its child content.
                    _owner.IsVisible = false;
                    backgroundContext.PushTransform(Matrix.CreateTranslation(-controlPositionInTopLevel.Value));
                    topLevel.Render(backgroundContext);
                    _owner.IsVisible = true;
                }

                using var memoryStream = new MemoryStream();
                backgroundBitmap.Save(memoryStream);
                memoryStream.Position = 0;
                using var backgroundImage = SKImage.FromEncodedData(memoryStream);

                if (backgroundImage is null) return;

                using var uniforms = new SKRuntimeEffectUniforms(_effect);
                uniforms["blurRadius"] = (float)_owner.Radius;
                uniforms["resolution"] = new[] { (float)pixelSize.Width, (float)pixelSize.Height };

                using var imageShader = backgroundImage.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
                using var children = new SKRuntimeEffectChildren(_effect) { { "", imageShader } };
                using var shader = _effect.ToShader(uniforms, children);

                using var paint = new SKPaint { Shader = shader };
                canvas.DrawRect(SKRect.Create(0, 0, (float)controlBounds.Width, (float)controlBounds.Height), paint);
            }
        }
    }
}
