using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.IO;

namespace LiquidGlassAvaloniaUI
{
    // 注意：这是一个控件（Control），而不是一个装饰器（Decorator）。它没有子元素。
    public class LiquidGlassControl : Control
    {
        #region Avalonia Properties

        public static readonly StyledProperty<double> RadiusProperty =
            AvaloniaProperty.Register<LiquidGlassControl, double>(nameof(Radius), 25.0);

        // Radius 属性控制扭曲效果的强度。
        public double Radius
        {
            get => GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        #endregion

        static LiquidGlassControl()
        {
            // 当 Radius 属性变化时，触发重新渲染。
            AffectsRender<LiquidGlassControl>(RadiusProperty);
        }

        /// <summary>
        /// 重写标准的 Render 方法来执行所有的绘图操作。
        /// </summary>
        public override void Render(DrawingContext context)
        {
            // 使用 Custom 方法将我们的 Skia 绘图逻辑插入到渲染管线中。
            // 关键改动：在这里直接传递 Radius 的值，而不是在渲染线程中访问它。
            context.Custom(new LiquidGlassDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), Radius));

            // 因为这个控件没有子元素，所以我们不再调用 base.Render()。
        }

        /// <summary>
        /// 一个处理 Skia 渲染的自定义绘图操作。
        /// </summary>
        private class LiquidGlassDrawOperation : ICustomDrawOperation
        {
            private readonly Rect _bounds;
            // 存储从 UI 线程传递过来的 Radius 值。
            private readonly double _radius;

            private static SKRuntimeEffect? _effect;
            private static bool _isShaderLoaded;

            // 构造函数现在接收一个 double 类型的 radius。
            public LiquidGlassDrawOperation(Rect bounds, double radius)
            {
                _bounds = bounds;
                _radius = radius;
            }

            public void Dispose() { }

            public bool HitTest(Point p) => _bounds.Contains(p);

            public Rect Bounds => _bounds;

            public bool Equals(ICustomDrawOperation? other) => false;

            public void Render(ImmediateDrawingContext context)
            {
                var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
                if (leaseFeature is null) return;

                // 确保着色器只被加载一次。
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
                    // 确保你的 .csproj 文件中包含了正确的 AvaloniaResource。
                    // <AvaloniaResource Include="Assets\LiquidGlassShader.sksl" />
                    var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/LiquidGlassShader.sksl");
                    using var stream = AssetLoader.Open(assetUri);
                    using var reader = new StreamReader(stream);
                    var shaderCode = reader.ReadToEnd();

                    _effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
                    if (_effect == null)
                    {
                        Console.WriteLine($"[SKIA ERROR] Failed to create SKRuntimeEffect: {errorText}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AVALONIA ERROR] Exception while loading shader: {ex.Message}");
                }
            }

            private void DrawErrorHint(SKCanvas canvas)
            {
                using var errorPaint = new SKPaint
                {
                    Color = new SKColor(255, 0, 0, 120), // 半透明红色
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

                // 获取背景的快照
                using var backgroundSnapshot = lease.SkSurface.Snapshot();
                if (backgroundSnapshot is null) return;

                if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                    return;

                using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);

                var pixelSize = new PixelSize((int)_bounds.Width, (int)_bounds.Height);
                using var uniforms = new SKRuntimeEffectUniforms(_effect);

                // 关键改动：使用从构造函数中存储的 _radius 值。
                uniforms["radius"] = (float)_radius;
                uniforms["resolution"] = new[] { (float)pixelSize.Width, (float)pixelSize.Height };

                using var children = new SKRuntimeEffectChildren(_effect) { { "content", backdropShader } };
                using var finalShader = _effect.ToShader(uniforms, children);

                using var paint = new SKPaint { Shader = finalShader };
                canvas.DrawRect(SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height), paint);
            }
        }
    }
}
