using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.IO;
using System.Collections.Generic;

namespace LiquidGlassAvaloniaUI
{
    /// <summary>
    /// 位移贴图管理器 - 负责加载和管理不同模式的位移贴图
    /// </summary>
    public static class DisplacementMapManager
    {
        private static readonly Dictionary<LiquidGlassMode, SKBitmap?> _preloadedMaps = new();
        private static readonly Dictionary<string, SKBitmap?> _shaderGeneratedMaps = new();
        private static readonly object _lockObject = new object(); // 添加线程安全锁
        private static bool _mapsLoaded = false;

        /// <summary>
        /// 预加载所有位移贴图
        /// </summary>
        public static void LoadDisplacementMaps()
        {
            lock (_lockObject)
            {
                if (_mapsLoaded) return;
                _mapsLoaded = true;

                try
                {
                    // 加载标准位移贴图
                    _preloadedMaps[LiquidGlassMode.Standard] = LoadMapFromAssets("DisplacementMaps/standard_displacement.jpeg");

                    // 加载极坐标位移贴图
                    _preloadedMaps[LiquidGlassMode.Polar] = LoadMapFromAssets("DisplacementMaps/polar_displacement.jpeg");

                    // 加载突出边缘位移贴图
                    _preloadedMaps[LiquidGlassMode.Prominent] = LoadMapFromAssets("DisplacementMaps/prominent_displacement.jpeg");

                    // Shader模式的贴图将动态生成
                    _preloadedMaps[LiquidGlassMode.Shader] = null;
                }
                catch (Exception ex)
                {
                    #if DEBUG
                    Console.WriteLine($"[DisplacementMapManager] Error loading displacement maps: {ex.Message}");
                    #endif
                }
            }
        }

        /// <summary>
        /// 从资源文件加载位移贴图
        /// </summary>
        private static SKBitmap? LoadMapFromAssets(string resourcePath)
        {
            try
            {
                var assetUri = new Uri($"avares://LiquidGlassAvaloniaUI/Assets/{resourcePath}");
                using var stream = AssetLoader.Open(assetUri);
                return SKBitmap.Decode(stream);
            }
            catch (Exception ex)
            {
                #if DEBUG
                Console.WriteLine($"[DisplacementMapManager] Failed to load {resourcePath}: {ex.Message}");
                #endif
                return null;
            }
        }

        /// <summary>
        /// 获取指定模式的位移贴图
        /// </summary>
        public static SKBitmap? GetDisplacementMap(LiquidGlassMode mode, int width = 0, int height = 0)
        {
            lock (_lockObject)
            {
                LoadDisplacementMaps();

                if (mode == LiquidGlassMode.Shader)
                {
                    // 为Shader模式生成动态位移贴图
                    var key = $"shader_{width}x{height}";
                    if (!_shaderGeneratedMaps.ContainsKey(key) && width > 0 && height > 0)
                    {
                        #if DEBUG
                        Console.WriteLine($"[DisplacementMapManager] Generating shader displacement map: {width}x{height}");
                        #endif
                        try
                        {
                            var bitmap = GenerateShaderDisplacementMap(width, height);
                            _shaderGeneratedMaps[key] = bitmap;
                        }
                        catch (Exception ex)
                        {
                            #if DEBUG
                            Console.WriteLine($"[DisplacementMapManager] Error generating shader map: {ex.Message}");
                            #endif
                            _shaderGeneratedMaps[key] = null;
                        }
                    }
                    var result = _shaderGeneratedMaps.TryGetValue(key, out var shaderBitmap) ? shaderBitmap : null;

                    // 验证位图是否有效
                    if (result != null && (result.IsEmpty || result.IsNull))
                    {
                        #if DEBUG
                        Console.WriteLine($"[DisplacementMapManager] Shader bitmap is invalid, removing from cache");
                        #endif
                        _shaderGeneratedMaps.Remove(key);
                        result = null;
                    }

                    #if DEBUG
                    Console.WriteLine($"[DisplacementMapManager] Shader mode map: {(result != null ? "Found" : "Not found")}");
                    #endif
                    return result;
                }

                var preloadedResult = _preloadedMaps.TryGetValue(mode, out var preloadedBitmap) ? preloadedBitmap : null;

                // 验证预加载位图是否有效
                if (preloadedResult != null && (preloadedResult.IsEmpty || preloadedResult.IsNull))
                {
                    #if DEBUG
                    Console.WriteLine($"[DisplacementMapManager] Preloaded bitmap for {mode} is invalid");
                    #endif
                    preloadedResult = null;
                }

                #if DEBUG
                Console.WriteLine($"[DisplacementMapManager] {mode} mode map: {(preloadedResult != null ? "Found" : "Not found")}");
                #endif
                return preloadedResult;
            }
        }

        /// <summary>
        /// 生成Shader模式的位移贴图（对应TS版本的ShaderDisplacementGenerator）
        /// </summary>
        private static SKBitmap GenerateShaderDisplacementMap(int width, int height)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var canvas = new SKCanvas(bitmap);

            try
            {
                // 实现液态玻璃Shader算法
                var pixels = new uint[width * height];
                var maxScale = 0f;
                var rawValues = new List<(float dx, float dy)>();

                // 第一遍：计算所有位移值
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var uv = new LiquidVector2(x / (float)width, y / (float)height);
                        var pos = LiquidGlassShader(uv);

                        var dx = pos.X * width - x;
                        var dy = pos.Y * height - y;

                        maxScale = Math.Max(maxScale, Math.Max(Math.Abs(dx), Math.Abs(dy)));
                        rawValues.Add((dx, dy));
                    }
                }

                // 确保最小缩放值防止过度归一化
                if (maxScale > 0)
                    maxScale = Math.Max(maxScale, 1);
                else
                    maxScale = 1;

                // 第二遍：转换为图像数据
                int rawIndex = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var (dx, dy) = rawValues[rawIndex++];

                        // 边缘平滑化
                        var edgeDistance = Math.Min(Math.Min(x, y), Math.Min(width - x - 1, height - y - 1));
                        var edgeFactor = Math.Min(1f, edgeDistance / 2f);

                        var smoothedDx = dx * edgeFactor;
                        var smoothedDy = dy * edgeFactor;

                        var r = smoothedDx / maxScale + 0.5f;
                        var g = smoothedDy / maxScale + 0.5f;

                        var red = (byte)Math.Max(0, Math.Min(255, r * 255));
                        var green = (byte)Math.Max(0, Math.Min(255, g * 255));
                        var blue = (byte)Math.Max(0, Math.Min(255, g * 255)); // 蓝色通道复制绿色以兼容SVG
                        var alpha = (byte)255;

                        pixels[y * width + x] = (uint)((alpha << 24) | (blue << 16) | (green << 8) | red);
                    }
                }

                // 将像素数据写入bitmap
                unsafe
                {
                    fixed (uint* ptr = pixels)
                    {
                        bitmap.SetPixels((IntPtr)ptr);
                    }
                }
            }
            finally
            {
                canvas.Dispose();
            }

            return bitmap;
        }

        /// <summary>
        /// 液态玻璃Shader函数 (对应TS版本的liquidGlass shader)
        /// </summary>
        private static LiquidVector2 LiquidGlassShader(LiquidVector2 uv)
        {
            var ix = uv.X - 0.5f;
            var iy = uv.Y - 0.5f;
            var distanceToEdge = RoundedRectSDF(ix, iy, 0.3f, 0.2f, 0.6f);
            var displacement = SmoothStep(0.8f, 0f, distanceToEdge - 0.15f);
            var scaled = SmoothStep(0f, 1f, displacement);
            return new LiquidVector2(ix * scaled + 0.5f, iy * scaled + 0.5f);
        }

        /// <summary>
        /// 有符号距离场 - 圆角矩形
        /// </summary>
        private static float RoundedRectSDF(float x, float y, float width, float height, float radius)
        {
            var qx = Math.Abs(x) - width + radius;
            var qy = Math.Abs(y) - height + radius;
            return Math.Min(Math.Max(qx, qy), 0) + Length(Math.Max(qx, 0), Math.Max(qy, 0)) - radius;
        }

        /// <summary>
        /// 向量长度计算
        /// </summary>
        private static float Length(float x, float y)
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// 平滑步进函数 (Hermite插值)
        /// </summary>
        private static float SmoothStep(float a, float b, float t)
        {
            t = Math.Max(0, Math.Min(1, (t - a) / (b - a)));
            return t * t * (3 - 2 * t);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Cleanup()
        {
            foreach (var bitmap in _preloadedMaps.Values)
            {
                bitmap?.Dispose();
            }
            _preloadedMaps.Clear();

            foreach (var bitmap in _shaderGeneratedMaps.Values)
            {
                bitmap?.Dispose();
            }
            _shaderGeneratedMaps.Clear();

            _mapsLoaded = false;
        }
    }

    /// <summary>
    /// LiquidVector2结构体 - 避免与系统Vector2冲突
    /// </summary>
    public struct LiquidVector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public LiquidVector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}