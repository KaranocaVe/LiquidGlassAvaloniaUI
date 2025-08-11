using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.VisualTree;
using SkiaSharp;
using System;
using System.IO;

namespace LiquidGlassAvaloniaUI
{
    // Note: This is now a Control, not a Decorator. It does not have children.
    public class LiquidGlassControl : Control
    {
        #region Avalonia Properties

        public static readonly StyledProperty<double> RadiusProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(Radius), 25.0);

        // The Radius property controls the distortion effect.
        public double Radius
        {
            get => GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        #endregion

        static LiquidGlassControl()
        {
            // When Radius property changes, trigger a re-render.
            AffectsRender<LiquidGlassControl>(RadiusProperty);
        }

        /// <summary>
        /// Overrides the standard Render method to perform all drawing operations.
        /// </summary>
        public override void Render(DrawingContext context)
        {
            // Use the Custom method to insert our Skia drawing logic into the render pipeline.
            context.Custom(new LiquidGlassDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), this));

            // We no longer call base.Render() because this control has no children.
        }

        /// <summary>
        /// A custom draw operation that handles the Skia rendering.
        /// </summary>
        private class LiquidGlassDrawOperation : ICustomDrawOperation
        {
            private readonly Rect _bounds;
            private readonly LiquidGlassControl _owner;

            private static SKRuntimeEffect? _effect;
            private static bool _isShaderLoaded;

            public LiquidGlassDrawOperation(Rect bounds, LiquidGlassControl owner)
            {
                _bounds = bounds;
                _owner = owner;
            }

            public void Dispose() { }

            public bool HitTest(Point p) => _bounds.Contains(p);

            public Rect Bounds => _bounds;

            public bool Equals(ICustomDrawOperation? other) => false;

            public void Render(ImmediateDrawingContext context)
            {
                var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
                if (leaseFeature is null) return;

                LoadShader();

                using var lease = leaseFeature.Lease();
                var canvas = lease.SkCanvas;

                if (_effect is null)
                {
                    DrawErrorHint(canvas);
                }
                else
                {
                    DrawLiquidGlassEffect(canvas, lease);
                }
            }

            private void LoadShader()
            {
                if (_isShaderLoaded) return;
                _isShaderLoaded = true;

                try
                {
                    var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/LiquidGlassShader.sksl");
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
                    Console.WriteLine($"Exception while loading shader: {ex.Message}");
                }
            }

            private void DrawErrorHint(SKCanvas canvas)
            {
                using var errorPaint = new SKPaint
                {
                    Color = new SKColor(255, 0, 0, 120), // Semi-transparent red
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawRect(SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height), errorPaint);

                using var textPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 14,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("Shader Failed to Load!", (float)_bounds.Width / 2, (float)_bounds.Height / 2, textPaint);
            }

            private void DrawLiquidGlassEffect(SKCanvas canvas, ISkiaSharpApiLease lease)
            {
                if (_effect is null) return;

                using var backgroundSnapshot = lease.SkSurface.Snapshot();
                if (backgroundSnapshot is null) return;

                if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                    return;

                using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);

                var pixelSize = new PixelSize((int)_bounds.Width, (int)_bounds.Height);
                using var uniforms = new SKRuntimeEffectUniforms(_effect);

                uniforms["radius"] = (float)_owner.Radius;
                uniforms["resolution"] = new[] { (float)pixelSize.Width, (float)pixelSize.Height };

                using var children = new SKRuntimeEffectChildren(_effect) { { "content", backdropShader } };
                using var finalShader = _effect.ToShader(uniforms, children);

                using var paint = new SKPaint { Shader = finalShader };
                canvas.DrawRect(SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height), paint);
            }
        }
    }
}
