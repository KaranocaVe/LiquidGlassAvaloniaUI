using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.IO;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// Shader 调试工具 - 检查 Shader 是否正确加载和参数是否正确传递
    /// </summary>
    public static class ShaderDebugger
    {
        public static void TestShaderLoading()
        {
            Console.WriteLine("[ShaderDebugger] 开始测试 Shader 加载...");

            try
            {
                // 尝试加载 Shader 文件
                var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassShader.sksl");
                using var stream = AssetLoader.Open(assetUri);
                using var reader = new StreamReader(stream);
                var shaderCode = reader.ReadToEnd();

                Console.WriteLine($"[ShaderDebugger] Shader 文件大小: {shaderCode.Length} 字符");
                Console.WriteLine($"[ShaderDebugger] Shader 前100字符: {shaderCode.Substring(0, Math.Min(100, shaderCode.Length))}");

                // 尝试编译 Shader
                var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
                if (effect != null)
                {
                    Console.WriteLine("[ShaderDebugger] ✅ Shader 编译成功!");
                    
                    // 检查 Uniform 变量
                    var uniformSize = effect.UniformSize;
                    Console.WriteLine($"[ShaderDebugger] Uniform 大小: {uniformSize} 字节");
                    
                    Console.WriteLine($"[ShaderDebugger] 尝试创建 Uniforms 对象...");
                    using var uniforms = new SKRuntimeEffectUniforms(effect);
                    Console.WriteLine($"[ShaderDebugger] ✅ Uniforms 对象创建成功");
                    
                    Console.WriteLine($"[ShaderDebugger] 尝试创建 Children 对象...");
                    using var children = new SKRuntimeEffectChildren(effect);
                    Console.WriteLine($"[ShaderDebugger] ✅ Children 对象创建成功");
                    
                    effect.Dispose();
                }
                else
                {
                    Console.WriteLine($"[ShaderDebugger] ❌ Shader 编译失败: {errorText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ShaderDebugger] ❌ 异常: {ex.Message}");
                Console.WriteLine($"[ShaderDebugger] 堆栈跟踪: {ex.StackTrace}");
            }
        }

        public static void TestDisplacementMaps()
        {
            Console.WriteLine("[ShaderDebugger] 开始测试位移贴图加载...");

            DisplacementMapManager.LoadDisplacementMaps();

            var modes = new[] { LiquidGlassMode.Standard, LiquidGlassMode.Polar, LiquidGlassMode.Prominent, LiquidGlassMode.Shader };
            
            foreach (var mode in modes)
            {
                var map = DisplacementMapManager.GetDisplacementMap(mode, 256, 256);
                if (map != null)
                {
                    Console.WriteLine($"[ShaderDebugger] ✅ {mode} 位移贴图加载成功 ({map.Width}x{map.Height})");
                }
                else
                {
                    Console.WriteLine($"[ShaderDebugger] ❌ {mode} 位移贴图加载失败");
                }
            }
        }

        public static void TestParameters()
        {
            Console.WriteLine("[ShaderDebugger] 开始测试参数传递...");

            var parameters = new LiquidGlassDrawParameters
            {
                CornerRadius = new CornerRadius(25.0),
                BackdropZoom = 1.0,
                BackdropOffset = new Vector(0.0, 0.0),
                RefractionHeight = 12.0,
                RefractionAmount = 24.0,
                DepthEffect = false,
                ChromaticAberration = false,
                BlurRadius = 2.0,
                Vibrancy = 1.5,
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

            Console.WriteLine($"[ShaderDebugger] RefractionHeight: {parameters.RefractionHeight}");
            Console.WriteLine($"[ShaderDebugger] RefractionAmount: {parameters.RefractionAmount}");
            Console.WriteLine($"[ShaderDebugger] BlurRadius: {parameters.BlurRadius}");
            Console.WriteLine($"[ShaderDebugger] Vibrancy: {parameters.Vibrancy}");
            Console.WriteLine($"[ShaderDebugger] ChromaticAberration: {parameters.ChromaticAberration}");
            Console.WriteLine($"[ShaderDebugger] DepthEffect: {parameters.DepthEffect}");
        }
    }
}
