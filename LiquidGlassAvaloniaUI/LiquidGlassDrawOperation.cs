using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.IO;
using Avalonia.Media;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// 液态玻璃绘制操作 - 处理 Skia 渲染的自定义绘图操作
    /// </summary>
    internal class LiquidGlassDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly LiquidGlassParameters _parameters;

        private static SKRuntimeEffect? _liquidGlassEffect;
        private static bool _isShaderLoaded;

        public LiquidGlassDrawOperation(Rect bounds, LiquidGlassParameters parameters)
        {
            _bounds = bounds;
            _parameters = parameters;
        }

        public void Dispose() { }

        public bool HitTest(Point p) => _bounds.Contains(p);

        public Rect Bounds => _bounds;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null) return;

            // 确保位移贴图已加载
            DisplacementMapManager.LoadDisplacementMaps();

            // 确保着色器只被加载一次
            LoadShader();

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            if (_liquidGlassEffect is null)
            {
                DrawErrorHint(canvas);
            }
            else
            {
                DrawLiquidGlassEffect(canvas, lease);
            }
        }

        /// <summary>
        /// 加载液态玻璃着色器
        /// </summary>
        private void LoadShader()
        {
            if (_isShaderLoaded) return;
            _isShaderLoaded = true;

            try
            {
                // 加载SKSL着色器代码
                var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassShader.sksl");
                using var stream = AssetLoader.Open(assetUri);
                using var reader = new StreamReader(stream);
                var shaderCode = reader.ReadToEnd();

                _liquidGlassEffect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
                if (_liquidGlassEffect == null)
                {
                    Console.WriteLine($"[SKIA ERROR] Failed to create liquid glass effect: {errorText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AVALONIA ERROR] Exception while loading liquid glass shader: {ex.Message}");
            }
        }

        /// <summary>
        /// 绘制错误提示
        /// </summary>
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
            canvas.DrawText("Liquid Glass Shader Failed to Load!",
                (float)_bounds.Width / 2, (float)_bounds.Height / 2, textPaint);
        }

        /// <summary>
        /// 绘制液态玻璃效果
        /// </summary>
        private void DrawLiquidGlassEffect(SKCanvas canvas, ISkiaSharpApiLease lease)
        {
            if (_liquidGlassEffect is null) return;

            // 获取背景快照
            using var backgroundSnapshot = lease.SkSurface.Snapshot();
            if (backgroundSnapshot is null) return;

            if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                return;

            // 创建背景着色器
            using var backdropShader = SKShader.CreateImage(
                backgroundSnapshot,
                SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp,
                currentInvertedTransform);

            // 获取位移贴图
            var displacementMap = DisplacementMapManager.GetDisplacementMap(
                _parameters.Mode,
                (int)_bounds.Width,
                (int)_bounds.Height);

            SKShader? displacementShader = null;
            if (displacementMap != null && !displacementMap.IsEmpty && !displacementMap.IsNull)
            {
                try
                {
                    // 额外验证位图数据完整性
                    var info = displacementMap.Info;
                    if (info.Width > 0 && info.Height > 0 && info.BytesSize > 0)
                    {
                        displacementShader = SKShader.CreateBitmap(
                            displacementMap,
                            SKShaderTileMode.Clamp,
                            SKShaderTileMode.Clamp);
                    }
                    else
                    {
                        Console.WriteLine("[LiquidGlassDrawOperation] Displacement bitmap has invalid dimensions or size");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LiquidGlassDrawOperation] Error creating displacement shader: {ex.Message}");
                    displacementShader = null;
                }
            }
            else
            {
                Console.WriteLine($"[LiquidGlassDrawOperation] No valid displacement map for mode: {_parameters.Mode}");
            }

            // 设置 Uniform 变量
            using var uniforms = new SKRuntimeEffectUniforms(_liquidGlassEffect);

            // 基础参数 - 修复参数范围和计算
            uniforms["resolution"] = new[] { (float)_bounds.Width, (float)_bounds.Height };
            
            var displacementValue = (float)(_parameters.DisplacementScale * GetModeScale());
            uniforms["displacementScale"] = displacementValue;
            Console.WriteLine($"[LiquidGlassDrawOperation] DisplacementScale: {_parameters.DisplacementScale} -> {displacementValue}");
            
            uniforms["blurAmount"] = (float)_parameters.BlurAmount; // 直接传递，不要额外缩放
            
            // 修复饱和度计算 - TypeScript版本使用0-2范围，1为正常
            uniforms["saturation"] = (float)(_parameters.Saturation / 100.0); // 140/100 = 1.4
            
            uniforms["aberrationIntensity"] = (float)_parameters.AberrationIntensity;
            uniforms["cornerRadius"] = (float)_parameters.CornerRadius;

            // 鼠标交互参数 - 修复坐标传递
            uniforms["mouseOffset"] = new[] { (float)(_parameters.MouseOffsetX / 100.0), (float)(_parameters.MouseOffsetY / 100.0) };
            uniforms["globalMouse"] = new[] { (float)_parameters.GlobalMouseX, (float)_parameters.GlobalMouseY };

            // 状态参数
            uniforms["isHovered"] = _parameters.IsHovered ? 1.0f : 0.0f;
            uniforms["isActive"] = _parameters.IsActive ? 1.0f : 0.0f;
            uniforms["overLight"] = _parameters.OverLight ? 1.0f : 0.0f;

            // 修复边缘遮罩参数计算
            var edgeMaskOffset = (float)Math.Max(0.1, (100.0 - _parameters.AberrationIntensity * 10.0) / 100.0);
            uniforms["edgeMaskOffset"] = edgeMaskOffset;

            // 修复色差偏移计算 - 根据TypeScript版本调整
            var baseScale = _parameters.Mode == LiquidGlassMode.Shader ? 1.0f : 1.0f;
            var redScale = baseScale;
            var greenScale = baseScale - (float)_parameters.AberrationIntensity * 0.002f;
            var blueScale = baseScale - (float)_parameters.AberrationIntensity * 0.004f;

            uniforms["chromaticAberrationScales"] = new[] { redScale, greenScale, blueScale };

            // 设置纹理
            using var children = new SKRuntimeEffectChildren(_liquidGlassEffect);
            children["backgroundTexture"] = backdropShader;

            if (displacementShader != null)
            {
                children["displacementTexture"] = displacementShader;
                uniforms["hasDisplacementMap"] = 1.0f;
            }
            else
            {
                uniforms["hasDisplacementMap"] = 0.0f;
            }

            // 创建最终着色器
            try
            {
                using var finalShader = _liquidGlassEffect.ToShader(uniforms, children);
                if (finalShader == null)
                {
                    Console.WriteLine("[LiquidGlassDrawOperation] Failed to create final shader");
                    return;
                }
                
                using var paint = new SKPaint { Shader = finalShader, IsAntialias = true };

                // 应用背景模糊效果 - 修复模糊计算
                if (_parameters.BlurAmount > 0.001) // 只有真正需要模糊时才应用
                {
                    // 使用更线性和可控的模糊半径计算
                    var blurRadius = (float)(_parameters.BlurAmount * 20.0); // 调整缩放因子
                    
                    // 根据OverLight状态增加基础模糊（可选）
                    if (_parameters.OverLight)
                    {
                        blurRadius += 2.0f; // 在亮色背景上增加轻微的基础模糊
                    }
                    
                    // 确保模糊半径在合理范围内
                    blurRadius = Math.Max(0.1f, Math.Min(blurRadius, 50.0f));
                    
                    using var blurFilter = SKImageFilter.CreateBlur(blurRadius, blurRadius);
                    paint.ImageFilter = blurFilter;
                }

                // 绘制带圆角的效果 - 关键修复：只在圆角矩形内绘制
                var cornerRadius = (float)Math.Min(_parameters.CornerRadius, Math.Min(_bounds.Width, _bounds.Height) / 2);
                var rect = SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height);
                
                // 创建圆角矩形路径
                using var path = new SKPath();
                path.AddRoundRect(rect, cornerRadius, cornerRadius);
                
                // 裁剪到圆角矩形并绘制
                canvas.Save();
                canvas.ClipPath(path, SKClipOperation.Intersect, true);
                canvas.DrawRect(rect, paint);
                canvas.Restore();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LiquidGlassDrawOperation] Error creating or using shader: {ex.Message}");
            }

            // 绘制边框效果
            DrawBorderEffects(canvas);

            // 绘制悬停和激活状态效果
            if (_parameters.IsHovered || _parameters.IsActive)
            {
                DrawInteractionEffects(canvas);
            }

            // 清理位移着色器
            displacementShader?.Dispose();
        }

        /// <summary>
        /// 获取模式缩放系数 - 修复版本
        /// </summary>
        private double GetModeScale()
        {
            // 所有模式都使用正向缩放，通过Shader内部逻辑区分
            return _parameters.Mode switch
            {
                LiquidGlassMode.Standard => 1.0,
                LiquidGlassMode.Polar => 1.2,    // Polar模式稍微增强效果
                LiquidGlassMode.Prominent => 1.5, // Prominent模式显著增强效果
                LiquidGlassMode.Shader => 1.0,
                _ => 1.0
            };
        }

        /// <summary>
        /// 绘制边框效果 (对应TS版本的多层边框)
        /// </summary>
        private void DrawBorderEffects(SKCanvas canvas)
        {
            var cornerRadius = (float)Math.Min(_parameters.CornerRadius, Math.Min(_bounds.Width, _bounds.Height) / 2);
            var rect = SKRect.Create(1.5f, 1.5f, (float)_bounds.Width - 3f, (float)_bounds.Height - 3f);

            // 第一层边框 (Screen blend mode)
            var angle = 135f + (float)_parameters.MouseOffsetX * 1.2f;
            var startOpacity = 0.12f + Math.Abs((float)_parameters.MouseOffsetX) * 0.008f;
            var midOpacity = 0.4f + Math.Abs((float)_parameters.MouseOffsetX) * 0.012f;
            var startPos = Math.Max(10f, 33f + (float)_parameters.MouseOffsetY * 0.3f) / 100f;
            var endPos = Math.Min(90f, 66f + (float)_parameters.MouseOffsetY * 0.4f) / 100f;

            using var borderPaint1 = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            var colors = new SKColor[]
            {
                new SKColor(255, 255, 255, 0),
                new SKColor(255, 255, 255, (byte)(startOpacity * 255)),
                new SKColor(255, 255, 255, (byte)(midOpacity * 255)),
                new SKColor(255, 255, 255, 0)
            };

            var positions = new float[] { 0f, startPos, endPos, 1f };

            using var gradient1 = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint((float)_bounds.Width, (float)_bounds.Height),
                colors,
                positions,
                SKShaderTileMode.Clamp);

            borderPaint1.Shader = gradient1;
            canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, borderPaint1);

            // 第二层边框 (Overlay blend mode)
            using var borderPaint2 = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Overlay
            };

            var colors2 = new SKColor[]
            {
                new SKColor(255, 255, 255, 0),
                new SKColor(255, 255, 255, (byte)((startOpacity + 0.2f) * 255)),
                new SKColor(255, 255, 255, (byte)((midOpacity + 0.2f) * 255)),
                new SKColor(255, 255, 255, 0)
            };

            using var gradient2 = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint((float)_bounds.Width, (float)_bounds.Height),
                colors2,
                positions,
                SKShaderTileMode.Clamp);

            borderPaint2.Shader = gradient2;
            canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, borderPaint2);
        }

        /// <summary>
        /// 绘制交互效果 (悬停和激活状态)
        /// </summary>
        private void DrawInteractionEffects(SKCanvas canvas)
        {
            var cornerRadius = (float)Math.Min(_parameters.CornerRadius, Math.Min(_bounds.Width, _bounds.Height) / 2);
            var rect = SKRect.Create(0, 0, (float)_bounds.Width, (float)_bounds.Height);

            if (_parameters.IsHovered)
            {
                // 悬停效果
                using var hoverPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    BlendMode = SKBlendMode.Overlay
                };

                using var hoverGradient = SKShader.CreateRadialGradient(
                    new SKPoint((float)_bounds.Width / 2, 0),
                    (float)_bounds.Width / 2,
                    new SKColor[] {
                        new SKColor(255, 255, 255, 127), // 50% opacity
                        new SKColor(255, 255, 255, 0)
                    },
                    new float[] { 0f, 0.5f },
                    SKShaderTileMode.Clamp);

                hoverPaint.Shader = hoverGradient;
                canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, hoverPaint);
            }

            if (_parameters.IsActive)
            {
                // 激活效果
                using var activePaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    BlendMode = SKBlendMode.Overlay
                };

                using var activeGradient = SKShader.CreateRadialGradient(
                    new SKPoint((float)_bounds.Width / 2, 0),
                    (float)_bounds.Width,
                    new SKColor[] {
                        new SKColor(255, 255, 255, 204), // 80% opacity
                        new SKColor(255, 255, 255, 0)
                    },
                    new float[] { 0f, 1f },
                    SKShaderTileMode.Clamp);

                activePaint.Shader = activeGradient;
                canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, activePaint);
            }
        }
    }
}
