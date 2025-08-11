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
    // 注意：此控件现在应用的是“液态玻璃”扭曲效果，而不是之前的毛玻璃模糊。
    public class LiquidGlassDecorator : Decorator
    {
        #region Re-entrancy Guard

        // 一个防止 Render 方法被递归调用的标志。
        private bool _isRendering;

        #endregion

        #region Avalonia Properties

        public static readonly StyledProperty<double> RadiusProperty =
            AvaloniaProperty.Register<LiquidGlassDecorator, double>(nameof(Radius), 25.0);

        // Radius 属性现在控制扭曲效果。
        public double Radius
        {
            get => GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        #endregion

        static LiquidGlassDecorator()
        {
            // 当 Radius 属性变化时，触发重绘。
            AffectsRender<LiquidGlassDecorator>(RadiusProperty);
        }

        /// <summary>
        /// 重写标准的 Render 方法来执行所有绘制操作。
        /// </summary>
        public override void Render(DrawingContext context)
        {
            // 重入守卫：如果我们已经在渲染中，则不开始新的渲染。
            // 这会打破递归循环。
            if (_isRendering)
                return;

            try
            {
                _isRendering = true;

                // 1. 首先，调用 base.Render(context) 让 Avalonia 绘制所有子控件。
                //    这样就完成了标准的渲染通道。
                base.Render(context);

                // 2. 在子控件被绘制之后，插入我们的自定义模糊操作。
                //    这会在已渲染的子控件之上绘制我们的效果，
                //    但效果本身采样的是原始背景，从而产生子控件在玻璃之上的错觉。
                //    这个机制打破了渲染循环。
                context.Custom(new LiquidGlassDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), this));
            }
            finally
            {
                _isRendering = false;
            }
        }

        /// <summary>
        /// 处理 Skia 渲染的自定义绘制操作。
        /// </summary>
        private class LiquidGlassDrawOperation : ICustomDrawOperation
        {
            private readonly Rect _bounds;
            private readonly LiquidGlassDecorator _owner;

            private static SKRuntimeEffect? _effect;
            private static bool _isShaderLoaded;

            public LiquidGlassDrawOperation(Rect bounds, LiquidGlassDecorator owner)
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
                    // 更新为加载新的液态玻璃着色器。
                    var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/LiquidGlassShader.sksl");
                    using var stream = AssetLoader.Open(assetUri);
                    using var reader = new StreamReader(stream);
                    var shaderCode = reader.ReadToEnd();

                    _effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
                    if (_effect == null)
                    {
                        Console.WriteLine($"创建 SKRuntimeEffect 失败: {errorText}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载着色器时发生异常: {ex.Message}");
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
                canvas.DrawText("着色器加载失败！", (float)_bounds.Width / 2, (float)_bounds.Height / 2, textPaint);
            }

            private void DrawLiquidGlassEffect(SKCanvas canvas, ISkiaSharpApiLease lease)
            {
                if (_effect is null) return;

                // 1. 截取当前绘图表面的快照。这会捕获到目前为止绘制的所有内容。
                using var backgroundSnapshot = lease.SkSurface.Snapshot();
                if (backgroundSnapshot is null) return;

                // 2. 获取画布的反转变换矩阵。这对于将全屏快照正确映射到
                //    我们本地控件的坐标空间至关重要。
                if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                    return;

                // 3. 从背景快照创建一个着色器，并应用反转变换。
                //    这个着色器现在将正确地采样我们控件正后方的像素。
                using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);

                // 4. 为我们的 SKSL 扭曲着色器准备 uniforms。
                var pixelSize = new PixelSize((int)_bounds.Width, (int)_bounds.Height);
                using var uniforms = new SKRuntimeEffectUniforms(_effect);

                // 更新为传递 "radius" 而不是 "blurRadius"。
                uniforms["radius"] = (float)_owner.Radius;
                uniforms["resolution"] = new[] { (float)pixelSize.Width, (float)pixelSize.Height };

                // 5. 通过将我们的背景着色器作为 'content' 输入提供给 SKSL 效果，来创建最终的着色器。
                using var children = new SKRuntimeEffectChildren(_effect) { { "content", backdropShader } };
                using var finalShader = _effect.ToShader(uniforms, children);

                // 6. 创建一个带有最终着色器的画笔并进行绘制。
                using var paint = new SKPaint { Shader = finalShader };
                canvas.DrawRect(SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height), paint);
            }
        }
    }
}
