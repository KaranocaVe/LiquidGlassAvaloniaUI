using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using Xunit;

namespace LiquidGlassAvaloniaUI.Tests;

public class LiquidGlassRenderTests
{
    [AvaloniaFact]
    public void Lens_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassShader.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassShader.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Highlight_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassHighlight.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassHighlight.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Blur_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassBlur.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassBlur.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Vibrancy_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassVibrancy.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassVibrancy.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Interactive_highlight_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassInteractiveHighlight.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassInteractiveHighlight.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Gamma_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassGamma.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassGamma.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Progressive_mask_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassProgressiveMask.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassProgressiveMask.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Backdrop_transform_shader_compiles()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassBackdropTransform.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassBackdropTransform.sksl: {errorText}");
        effect!.Dispose();
    }

    [AvaloniaFact]
    public void Interactive_highlight_plus_blend_is_not_full_white()
    {
        var assetUri = new Uri("avares://LiquidGlassAvaloniaUI/Assets/Shaders/LiquidGlassInteractiveHighlight.sksl");
        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        using var effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        Assert.True(effect is not null, $"Failed to compile LiquidGlassInteractiveHighlight.sksl: {errorText}");

        var info = new SKImageInfo(64, 64, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        Assert.NotNull(surface);

        var canvas = surface!.Canvas;
        canvas.Clear(new SKColor(0, 0, 0, 255));

        using var uniforms = new SKRuntimeEffectUniforms(effect!);
        uniforms["size"] = new[] { 64f, 64f };
        uniforms["color"] = new[] { 1f, 1f, 1f, 0.15f };
        uniforms["radius"] = 96f;
        uniforms["position"] = new[] { 32f, 32f };

        using var children = new SKRuntimeEffectChildren(effect!);
        using var shader = effect!.ToShader(uniforms, children);
        Assert.NotNull(shader);

        using var paint = new SKPaint
        {
            Shader = shader,
            BlendMode = SKBlendMode.Plus
        };

        canvas.DrawRect(SKRect.Create(0, 0, 64, 64), paint);

        using var snapshot = surface.Snapshot();
        using var bitmap = new SKBitmap(info);
        Assert.True(snapshot.ReadPixels(info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0));

        var c = bitmap.GetPixel(32, 32);
        Assert.True(c.Red < 128, $"Expected subtle additive highlight (<128) but got R={c.Red}, A={c.Alpha}");
    }

    [AvaloniaFact]
    public void Blur_reduces_local_contrast()
    {
        var root = new Grid();

        var stripes = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < 60; i++)
        {
            stripes.Children.Add(new Border
            {
                Width = 8,
                Background = i % 2 == 0 ? Brushes.DeepSkyBlue : Brushes.Gold
            });
        }

        root.Children.Add(stripes);

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            HighlightEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var sharp = window.CaptureRenderedFrame();
            Assert.NotNull(sharp);

            var centerY = (int)(window.Height / 2);
            var sharpContrast = ComputeLineContrast(sharp!, centerY, 80, 400, step: 8);

            glass.BlurRadius = 16;

            Thread.Sleep(60);

            _ = window.CaptureRenderedFrame();
            var blurred = window.CaptureRenderedFrame();
            Assert.NotNull(blurred);

            var blurredContrast = ComputeLineContrast(blurred!, centerY, 80, 400, step: 8);
            Assert.True(blurredContrast < sharpContrast * 0.75, $"Expected blur to reduce contrast (sharp={sharpContrast:F1}, blurred={blurredContrast:F1}).");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Blur_does_not_translate_linear_gradient_backdrop()
    {
        var root = new Grid
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.Black, 0),
                    new GradientStop(Colors.White, 1)
                }
            }
        };

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(0),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent,
            HighlightEnabled = false,
            ShadowEnabled = false,
            InnerShadowEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();
            var frame1 = window.CaptureRenderedFrame();
            Assert.NotNull(frame1);

            var centerLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height / 2);
            var center = glass.TranslatePoint(centerLocal, window);
            Assert.True(center.HasValue);

            var sampleLocals = new[]
            {
                new Point(glass.Bounds.Width * 0.25, glass.Bounds.Height * 0.5),
                new Point(glass.Bounds.Width * 0.5, glass.Bounds.Height * 0.5),
                new Point(glass.Bounds.Width * 0.75, glass.Bounds.Height * 0.5),
            };

            glass.BlurRadius = 36;
            Thread.Sleep(60);

            _ = window.CaptureRenderedFrame();
            var frame2 = window.CaptureRenderedFrame();
            Assert.NotNull(frame2);

            foreach (var local in sampleLocals)
            {
                var pt = glass.TranslatePoint(local, window);
                Assert.True(pt.HasValue);

                var x = (int)Math.Round(pt!.Value.X);
                var y = (int)Math.Round(pt.Value.Y);

                var before = GetPixel(frame1!, x, y);
                var after = GetPixel(frame2!, x, y);

                var delta = Math.Abs(after.r - before.r) + Math.Abs(after.g - before.g) + Math.Abs(after.b - before.b);
                Assert.True(delta < 12, $"Expected blur to preserve linear gradient at {local} (before={before}, after={after}, Δ={delta}).");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Blur_filter_keeps_opaque_edges_in_filtered_backdrop()
    {
        var root = new Grid
        {
            Background = new SolidColorBrush(Color.FromRgb(80, 120, 200))
        };

        var glass = new LiquidGlassSurface
        {
            Width = 160,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(0),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 24,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            HighlightEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 220,
            Height = 220,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var state = GetProviderState(window);
            Assert.NotNull(state);

            var snapshotField = state!.GetType().GetField("Snapshot", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(snapshotField);

            var snapshot = snapshotField!.GetValue(state);
            Assert.NotNull(snapshot);

            var filteredField = snapshot!.GetType().GetField("_filtered", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(filteredField);

            var filtered = filteredField!.GetValue(snapshot);
            Assert.NotNull(filtered);

            var values = filtered!.GetType().GetProperty("Values");
            Assert.NotNull(values);

            var enumerable = values!.GetValue(filtered) as System.Collections.IEnumerable;
            Assert.NotNull(enumerable);

            SKImage? filteredImage = null;
            foreach (var entry in enumerable!)
            {
                var imageProp = entry!.GetType().GetProperty("Image");
                filteredImage = imageProp?.GetValue(entry) as SKImage;
                if (filteredImage is not null)
                    break;
            }

            Assert.NotNull(filteredImage);

            using var bitmap = new SKBitmap(new SKImageInfo(filteredImage!.Width, filteredImage.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
            Assert.True(filteredImage.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0));

            var tl = bitmap.GetPixel(0, 0);
            var tr = bitmap.GetPixel(bitmap.Width - 1, 0);
            var bl = bitmap.GetPixel(0, bitmap.Height - 1);
            var br = bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1);

            Assert.Equal(255, tl.Alpha);
            Assert.Equal(255, tr.Alpha);
            Assert.Equal(255, bl.Alpha);
            Assert.Equal(255, br.Alpha);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Progressive_blur_mask_makes_bottom_less_blurred_than_top()
    {
        var root = new Grid();

        var stripes = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < 80; i++)
        {
            stripes.Children.Add(new Border
            {
                Width = 6,
                Background = i % 2 == 0 ? Brushes.DeepSkyBlue : Brushes.Gold
            });
        }

        root.Children.Add(stripes);

        var glass = new LiquidGlassSurface
        {
            Width = 320,
            Height = 180,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(0),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 24,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent,
            ProgressiveBlurEnabled = false,
            ProgressiveBlurStart = 0.5,
            ProgressiveBlurEnd = 1.0,
            ProgressiveTintColor = Colors.White,
            ProgressiveTintIntensity = 0.0,
            HighlightEnabled = false,
            ShadowEnabled = false,
            InnerShadowEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 540,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();
            var baseline = window.CaptureRenderedFrame();
            Assert.NotNull(baseline);

            var topLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height * 0.25);
            var bottomLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height * 0.85);
            var top = glass.TranslatePoint(topLocal, window);
            var bottom = glass.TranslatePoint(bottomLocal, window);
            Assert.True(top.HasValue && bottom.HasValue);

            var leftTop = glass.TranslatePoint(new Point(10, topLocal.Y), window);
            var rightTop = glass.TranslatePoint(new Point(glass.Bounds.Width - 10, topLocal.Y), window);
            var leftBottom = glass.TranslatePoint(new Point(10, bottomLocal.Y), window);
            var rightBottom = glass.TranslatePoint(new Point(glass.Bounds.Width - 10, bottomLocal.Y), window);
            Assert.True(leftTop.HasValue && rightTop.HasValue && leftBottom.HasValue && rightBottom.HasValue);

            var topY = (int)Math.Round(top!.Value.Y);
            var bottomY = (int)Math.Round(bottom!.Value.Y);

            var xTop0 = (int)Math.Round(leftTop!.Value.X);
            var xTop1 = (int)Math.Round(rightTop!.Value.X);
            var xBottom0 = (int)Math.Round(leftBottom!.Value.X);
            var xBottom1 = (int)Math.Round(rightBottom!.Value.X);

            var topContrast0 = ComputeLineContrast(baseline!, topY, xTop0, xTop1, step: 6);
            var bottomContrast0 = ComputeLineContrast(baseline!, bottomY, xBottom0, xBottom1, step: 6);

            // With uniform blur, top and bottom should be similar.
            Assert.True(Math.Abs(topContrast0 - bottomContrast0) < 6.0,
                $"Expected baseline blur to be uniform (top={topContrast0:F1}, bottom={bottomContrast0:F1}).");

            glass.ProgressiveBlurEnabled = true;

            _ = window.CaptureRenderedFrame();
            var masked = window.CaptureRenderedFrame();
            Assert.NotNull(masked);

            var topContrast1 = ComputeLineContrast(masked!, topY, xTop0, xTop1, step: 6);
            var bottomContrast1 = ComputeLineContrast(masked!, bottomY, xBottom0, xBottom1, step: 6);

            Assert.True(bottomContrast1 > topContrast1 * 1.25,
                $"Expected progressive mask to make bottom less blurred than top (top={topContrast1:F1}, bottom={bottomContrast1:F1}).");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Backdrop_zoom_and_offset_change_sampled_backdrop_pixels()
    {
        var root = new Grid
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.Black, 0),
                    new GradientStop(Colors.White, 1)
                }
            }
        };

        var glass = new LiquidGlassSurface
        {
            Width = 260,
            Height = 180,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(0),
            BackdropZoom = 1.0,
            BackdropOffset = new Vector(0.0, 0.0),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent,
            HighlightEnabled = false,
            ShadowEnabled = false,
            InnerShadowEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 520,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();
            var baseline = window.CaptureRenderedFrame();
            Assert.NotNull(baseline);

            var sampleLocal = new Point(glass.Bounds.Width * 0.1, glass.Bounds.Height * 0.5);
            var sample = glass.TranslatePoint(sampleLocal, window);
            Assert.True(sample.HasValue);

            var sx = (int)Math.Round(sample!.Value.X);
            var sy = (int)Math.Round(sample.Value.Y);

            var c0 = GetPixel(baseline!, sx, sy);
            var l0 = c0.r + c0.g + c0.b;

            glass.BackdropZoom = 2.0;

            _ = window.CaptureRenderedFrame();
            var zoomed = window.CaptureRenderedFrame();
            Assert.NotNull(zoomed);

            var c1 = GetPixel(zoomed!, sx, sy);
            var l1 = c1.r + c1.g + c1.b;

            Assert.True(l1 > l0 + 8,
                $"Expected zoom to change sampled pixels (baseline={c0}, zoomed={c1}).");

            glass.BackdropZoom = 1.0;
            glass.BackdropOffset = new Vector(60.0, 0.0);

            _ = window.CaptureRenderedFrame();
            var offsetFrame = window.CaptureRenderedFrame();
            Assert.NotNull(offsetFrame);

            var c2 = GetPixel(offsetFrame!, sx, sy);
            var l2 = c2.r + c2.g + c2.b;

            Assert.True(l2 < l0 - 8,
                $"Expected positive X offset to shift sampled backdrop left and appear darker (baseline={c0}, offset={c2}).");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Adaptive_luminance_samples_backdrop_and_changes_blur_strength()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };

        var leftStripes = new StackPanel { Orientation = Orientation.Horizontal };
        var rightStripes = new StackPanel { Orientation = Orientation.Horizontal };

        for (var i = 0; i < 90; i++)
        {
            leftStripes.Children.Add(new Border
            {
                Width = 6,
                Background = i % 2 == 0 ? Brushes.Black : Brushes.DimGray
            });

            rightStripes.Children.Add(new Border
            {
                Width = 6,
                Background = i % 2 == 0 ? Brushes.White : Brushes.LightGray
            });
        }

        root.Children.Add(leftStripes);
        Grid.SetColumn(leftStripes, 0);

        root.Children.Add(rightStripes);
        Grid.SetColumn(rightStripes, 1);

        static LiquidGlassSurface CreateAdaptiveGlass()
        {
            return new LiquidGlassSurface
            {
                Width = 220,
                Height = 150,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CornerRadius = new CornerRadius(0),
                RefractionHeight = 0,
                RefractionAmount = 0,
                BlurRadius = 8,
                Vibrancy = 1,
                Brightness = 0,
                Contrast = 1,
                ExposureEv = 0,
                GammaPower = 1,
                BackdropOpacity = 1,
                TintColor = Colors.Transparent,
                SurfaceColor = Colors.Transparent,
                HighlightEnabled = false,
                ShadowEnabled = false,
                InnerShadowEnabled = false,
                AdaptiveLuminanceEnabled = true,
                AdaptiveLuminanceUpdateIntervalMs = 1000,
                AdaptiveLuminanceSmoothing = 1.0
            };
        }

        var leftGlass = CreateAdaptiveGlass();
        var rightGlass = CreateAdaptiveGlass();

        root.Children.Add(leftGlass);
        Grid.SetColumn(leftGlass, 0);

        root.Children.Add(rightGlass);
        Grid.SetColumn(rightGlass, 1);

        var window = new Window
        {
            Width = 680,
            Height = 300,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var tick = typeof(LiquidGlassSurface).GetMethod("TickAdaptiveLuminance", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(tick);

            for (var i = 0; i < 6 && rightGlass.AdaptiveLuminance <= 0.5; i++)
            {
                tick!.Invoke(leftGlass, null);
                tick.Invoke(rightGlass, null);
                _ = window.CaptureRenderedFrame();
            }

            _ = window.CaptureRenderedFrame();
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);

            Assert.True(leftGlass.AdaptiveLuminance < 0.5, $"Expected left luminance < 0.5 but got {leftGlass.AdaptiveLuminance:F2}.");
            Assert.True(rightGlass.AdaptiveLuminance > 0.5, $"Expected right luminance > 0.5 but got {rightGlass.AdaptiveLuminance:F2}.");

            var createParams = typeof(LiquidGlassSurface).GetMethod("CreateDrawParameters", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(createParams);

            var leftParams = createParams!.Invoke(leftGlass, null)!;
            var rightParams = createParams.Invoke(rightGlass, null)!;

            var blurProp = leftParams.GetType().GetProperty("BlurRadius");
            var contrastProp = leftParams.GetType().GetProperty("Contrast");
            Assert.NotNull(blurProp);
            Assert.NotNull(contrastProp);

            var leftBlur = (double)blurProp!.GetValue(leftParams)!;
            var rightBlur = (double)blurProp.GetValue(rightParams)!;

            var leftContrast = (double)contrastProp!.GetValue(leftParams)!;
            var rightContrast = (double)contrastProp.GetValue(rightParams)!;

            Assert.True(rightBlur > leftBlur + 1.0, $"Expected brighter backdrop to select higher blur (left={leftBlur:F2}, right={rightBlur:F2}).");
            Assert.True(Math.Abs(leftContrast - 1.0) < 0.001, $"Expected dark backdrop contrast to remain 1.0 but got {leftContrast:F3}.");
            Assert.True(rightContrast < 0.95, $"Expected bright backdrop to reduce contrast but got {rightContrast:F3}.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Shadow_darkens_pixels_outside_control_bounds()
    {
        var backgroundColor = Color.FromRgb(232, 232, 232);
        var root = new Grid
        {
            Background = new SolidColorBrush(backgroundColor)
        };

        var glass = new LiquidGlassSurface
        {
            Width = 200,
            Height = 120,
            Margin = new Thickness(80),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(24),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            HighlightEnabled = false,
            ShadowEnabled = false,
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame1 = window.CaptureRenderedFrame();
            Assert.NotNull(frame1);
            MaybeSave(frame1!, "shadow-disabled.png");

            var outsideLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height + 14);
            var outside = glass.TranslatePoint(outsideLocal, window);
            Assert.True(outside.HasValue);

            var before = GetPixel(frame1!, (int)Math.Round(outside!.Value.X), (int)Math.Round(outside!.Value.Y));
            var lBefore = before.r + before.g + before.b;

            glass.ShadowEnabled = true;
            glass.ShadowColor = Color.FromArgb(200, 0, 0, 0);
            glass.ShadowOpacity = 1.0;
            glass.ShadowRadius = 18.0;
            glass.ShadowOffset = new Vector(0, 12);

            _ = window.CaptureRenderedFrame();
            var frame2 = window.CaptureRenderedFrame();
            Assert.NotNull(frame2);
            MaybeSave(frame2!, "shadow-enabled.png");

            var after = GetPixel(frame2!, (int)Math.Round(outside!.Value.X), (int)Math.Round(outside!.Value.Y));
            var lAfter = after.r + after.g + after.b;

            // Force a full redraw to ensure dirty-rect clipping isn't masking the shadow.
            root.Background = new SolidColorBrush(Color.FromRgb(233, 233, 233));

            _ = window.CaptureRenderedFrame();
            var frame3 = window.CaptureRenderedFrame();
            Assert.NotNull(frame3);
            MaybeSave(frame3!, "shadow-enabled-fullredraw.png");

            var afterFull = GetPixel(frame3!, (int)Math.Round(outside!.Value.X), (int)Math.Round(outside!.Value.Y));
            var lAfterFull = afterFull.r + afterFull.g + afterFull.b;

            Assert.True(lAfter < lBefore - 5 || lAfterFull < lBefore - 5,
                $"Expected shadow to darken pixel outside bounds (before={before}, after={after}, afterFull={afterFull}).");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Inner_shadow_darkens_edge_pixels()
    {
        var backgroundColor = Color.FromRgb(120, 160, 220);
        var root = new Grid
        {
            Background = new SolidColorBrush(backgroundColor)
        };

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 140,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            HighlightEnabled = false,
            ShadowEnabled = false,
            InnerShadowEnabled = true,
            InnerShadowColor = Color.FromArgb(220, 0, 0, 0),
            InnerShadowOpacity = 1.0,
            InnerShadowRadius = 22.0,
            InnerShadowOffset = new Vector(0, 18)
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 520,
            Height = 360,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);

            var edgeLocal = new Point(glass.Bounds.Width / 2, 3);
            var centerLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height / 2);

            var edge = glass.TranslatePoint(edgeLocal, window);
            var center = glass.TranslatePoint(centerLocal, window);
            Assert.True(edge.HasValue && center.HasValue);

            var cEdge = GetPixel(frame!, (int)Math.Round(edge!.Value.X), (int)Math.Round(edge!.Value.Y));
            var cCenter = GetPixel(frame!, (int)Math.Round(center!.Value.X), (int)Math.Round(center!.Value.Y));

            var lEdge = cEdge.r + cEdge.g + cEdge.b;
            var lCenter = cCenter.r + cCenter.g + cCenter.b;

            Assert.True(lEdge < lCenter - 6, $"Expected inner shadow edge darker than center (edge={lEdge}, center={lCenter}, edgePixel={cEdge}, centerPixel={cCenter}).");

            MaybeSave(frame!, "inner-shadow.png");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Tint_and_surface_overlays_change_backdrop_pixels()
    {
        var root = new Grid();

        var stripes = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < 60; i++)
        {
            stripes.Children.Add(new Border
            {
                Width = 8,
                Background = i % 2 == 0 ? Brushes.DeepSkyBlue : Brushes.Gold
            });
        }

        root.Children.Add(stripes);

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            HighlightEnabled = false,
            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame1 = window.CaptureRenderedFrame();
            Assert.NotNull(frame1);

            glass.TintColor = Color.FromArgb(160, 255, 60, 200);
            glass.SurfaceColor = Color.FromArgb(70, 255, 255, 255);

            _ = window.CaptureRenderedFrame();
            var frame2 = window.CaptureRenderedFrame();
            Assert.NotNull(frame2);

            var x = (int)(window.Width / 2);
            var y = (int)(window.Height / 2);

            var a = GetPixel(frame1!, x, y);
            var b = GetPixel(frame2!, x, y);
            var diff = Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b);
            Assert.True(diff > 15, $"Expected tint/surface overlays to change pixels, got diff={diff} (before={a}, after={b}).");

            MaybeSave(frame2!, "tint-surface.png");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Interactive_highlight_changes_pixels_on_press()
    {
        var root = new Grid
        {
            Background = Brushes.Black
        };

        var glass = new LiquidGlassInteractiveSurface
        {
            Width = 240,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            ExposureEv = 0,
            GammaPower = 1,
            BackdropOpacity = 1,
            HighlightEnabled = false,
            ShadowEnabled = false,
            InnerShadowEnabled = false,
            IsInteractive = true,
            InteractiveHighlightEnabled = true
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 520,
            Height = 360,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var beforeFrame = window.CaptureRenderedFrame();
            Assert.NotNull(beforeFrame);

            var centerLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height / 2);
            var center = glass.TranslatePoint(centerLocal, window);
            Assert.True(center.HasValue);

            var before = GetPixel(beforeFrame!, (int)Math.Round(center!.Value.X), (int)Math.Round(center!.Value.Y));

            using var pointer = new Avalonia.Input.Pointer(Avalonia.Input.Pointer.GetNextFreeId(), Avalonia.Input.PointerType.Mouse, true);
            var pressedProps = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.LeftMouseButton, Avalonia.Input.PointerUpdateKind.LeftButtonPressed);

            glass.RaiseEvent(new Avalonia.Input.PointerPressedEventArgs(
                source: glass,
                pointer: pointer,
                rootVisual: window,
                rootVisualPosition: center.Value,
                timestamp: 0,
                properties: pressedProps,
                modifiers: Avalonia.Input.KeyModifiers.None,
                clickCount: 1));

            var pressProgressField = typeof(LiquidGlassInteractiveSurface).GetField("_pressProgress", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(pressProgressField);
            var pressProgress = pressProgressField!.GetValue(glass);
            Assert.NotNull(pressProgress);

            var snapTo = pressProgress!.GetType().GetMethod("SnapTo", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(snapTo);
            snapTo!.Invoke(pressProgress, new object[] { 1.0 });

            var movedProps = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.LeftMouseButton, Avalonia.Input.PointerUpdateKind.Other);
            window.RaiseEvent(new Avalonia.Input.PointerEventArgs(
                Avalonia.Input.InputElement.PointerMovedEvent,
                source: window,
                pointer: pointer,
                rootVisual: window,
                rootVisualPosition: center.Value,
                timestamp: 1,
                properties: movedProps,
                modifiers: Avalonia.Input.KeyModifiers.None));

            _ = window.CaptureRenderedFrame();
            var afterFrame = window.CaptureRenderedFrame();
            Assert.NotNull(afterFrame);

            var after = GetPixel(afterFrame!, (int)Math.Round(center!.Value.X), (int)Math.Round(center!.Value.Y));
            var lBefore = before.r + before.g + before.b;
            var lAfter = after.r + after.g + after.b;

            Assert.True(lAfter > lBefore + 10, $"Expected interactive highlight to increase brightness (before={before}, after={after}).");

            MaybeSave(afterFrame!, "interactive-highlight.png");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Color_controls_affect_backdrop()
    {
        var root = new Grid();

        var background = new Border { Background = Brushes.DeepSkyBlue };
        root.Children.Add(background);

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            Brightness = 0,
            Contrast = 1,
            HighlightEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame1 = window.CaptureRenderedFrame();
            Assert.NotNull(frame1);

            glass.Brightness = 0.4;
            glass.Contrast = 0.5;
            glass.Vibrancy = 1.5;

            _ = window.CaptureRenderedFrame();
            var frame2 = window.CaptureRenderedFrame();
            Assert.NotNull(frame2);

            var x = (int)(window.Width / 2);
            var y = (int)(window.Height / 2);

            var a = GetPixel(frame1!, x, y);
            var b = GetPixel(frame2!, x, y);
            var diff = Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b);

            Assert.True(diff > 15, $"Expected color controls to change pixels, got diff={diff} (before={a}, after={b}).");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(0.0, 1.0, 1.0, 0.0, 1.0)]
    [InlineData(0.2, 1.0, 1.0, 0.0, 1.0)]
    [InlineData(-0.15, 1.25, 0.85, -0.5, 1.0)]
    [InlineData(0.0, 1.0, 1.5, 0.0, 1.0)]
    [InlineData(0.15, 0.9, 1.4, 0.75, 1.2)]
    public void Color_controls_and_gamma_match_android_reference_math(
        double brightness,
        double contrast,
        double saturation,
        double exposureEv,
        double gammaPower)
    {
        var baseColor = Color.FromRgb(80, 120, 200);
        var root = new Grid
        {
            Background = new SolidColorBrush(baseColor)
        };

        var glass = new LiquidGlassSurface
        {
            Width = 180,
            Height = 140,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(0),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = saturation,
            Brightness = brightness,
            Contrast = contrast,
            ExposureEv = exposureEv,
            GammaPower = gammaPower,
            BackdropOpacity = 1,
            HighlightEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 320,
            Height = 220,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);

            var x = (int)(window.Width / 2);
            var y = (int)(window.Height / 2);
            var actual = GetPixel(frame!, x, y);

            var expected = ApplyAndroidColorPipeline(
                baseColor,
                brightness: brightness,
                contrast: contrast,
                saturation: saturation,
                exposureEv: exposureEv,
                gammaPower: gammaPower);

            Assert.InRange(actual.a, 250, 255);
            Assert.InRange(actual.r, expected.r - 3, expected.r + 3);
            Assert.InRange(actual.g, expected.g - 3, expected.g + 3);
            Assert.InRange(actual.b, expected.b - 3, expected.b + 3);

            if (Math.Abs(brightness - 0.15) < 0.0001
                && Math.Abs(contrast - 0.9) < 0.0001
                && Math.Abs(saturation - 1.4) < 0.0001
                && Math.Abs(exposureEv - 0.75) < 0.0001
                && Math.Abs(gammaPower - 1.2) < 0.0001)
            {
                MaybeSave(frame!, "color-controls-gamma.png");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlass_changes_backdrop_pixels()
    {
        var root = new Grid();

        var stripes = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < 60; i++)
        {
            stripes.Children.Add(new Border
            {
                Width = 8,
                Background = i % 2 == 0 ? Brushes.DeepSkyBlue : Brushes.Gold
            });
        }

        root.Children.Add(stripes);

        var glass = new LiquidGlassSurface
        {
            Width = 200,
            Height = 150,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            // First frame triggers backdrop capture; second frame should use it.
            _ = window.CaptureRenderedFrame();
            var withGlass = window.CaptureRenderedFrame();
            Assert.NotNull(withGlass);

            root.Children.Remove(glass);
            var withoutGlass = window.CaptureRenderedFrame();
            Assert.NotNull(withoutGlass);

            // Sample at the center of the card area.
            var x = (int)(window.Width / 2);
            var y = (int)(window.Height / 2);

            var a = GetPixel(withGlass!, x, y);
            var b = GetPixel(withoutGlass!, x, y);

            var diff = Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b);
            Assert.True(diff > 10, $"Expected a visible pixel difference, got diff={diff}");

            MaybeSave(withGlass!, "with-glass.png");
            MaybeSave(withoutGlass!, "without-glass.png");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlass_updates_when_backdrop_changes()
    {
        var background = new Border
        {
            Background = Brushes.DeepSkyBlue
        };

        var root = new Grid();
        root.Children.Add(background);

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame1 = window.CaptureRenderedFrame();
            Assert.NotNull(frame1);

            background.Background = Brushes.Gold;

            // Ensure we exceed the provider's capture interval (~33ms).
            Thread.Sleep(60);

            // First frame queues a new snapshot; second uses it (same pattern as the other tests).
            _ = window.CaptureRenderedFrame();
            var frame2 = window.CaptureRenderedFrame();
            Assert.NotNull(frame2);

            var x = (int)(window.Width / 2);
            var y = (int)(window.Height / 2);

            var a = GetPixel(frame1!, x, y);
            var b = GetPixel(frame2!, x, y);
            var diff = Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b);
            Assert.True(diff > 20, $"Expected glass pixels to change after backdrop change, got diff={diff}");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Backdrop_capture_excludes_glass_subtree()
    {
        var background = new Border
        {
            Background = Brushes.DeepSkyBlue
        };

        var child = new Border
        {
            Background = Brushes.White
        };

        var glass = new LiquidGlassSurface
        {
            Width = 240,
            Height = 180,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false,
            Child = child
        };

        var root = new Grid();
        root.Children.Add(background);
        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var hash1 = GetProviderSnapshotHash(window);
            Assert.NotEqual(0UL, hash1);

            child.Background = Brushes.Gold;

            // Ensure we exceed the provider's capture interval (~33ms).
            Thread.Sleep(60);

            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var hash2 = GetProviderSnapshotHash(window);
            Assert.Equal(hash1, hash2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Backdrop_origin_is_stable_across_updates()
    {
        var background = new Border
        {
            Background = Brushes.DeepSkyBlue
        };

        var glass = new LiquidGlassSurface
        {
            Width = 240,
            Height = 180,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false
        };

        var root = new Grid();
        root.Children.Add(background);
        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var origin1 = GetProviderSnapshotOrigin(window);
            Assert.NotNull(origin1);

            background.Background = Brushes.Gold;

            // Ensure we exceed the provider's capture interval (~33ms).
            Thread.Sleep(60);

            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var origin2 = GetProviderSnapshotOrigin(window);
            Assert.Equal(origin1, origin2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Backdrop_recaptures_immediately_when_required_clip_grows()
    {
        var root = new Grid
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.DarkBlue, 0),
                    new GradientStop(Colors.Gold, 1)
                }
            }
        };

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            Margin = new Thickness(80),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            HighlightEnabled = false,
            ShadowEnabled = false,
            InnerShadowEnabled = false
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();

            var origin1 = GetProviderSnapshotOrigin(window);
            Assert.NotNull(origin1);

            var state = GetProviderState(window);
            Assert.NotNull(state);

            var lastCapture = state!.GetType().GetField("LastCaptureTicksUtc", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(lastCapture);

            // Block the normal 33ms cadence so a growing clip would be skipped without the "clip grew" fast path.
            lastCapture!.SetValue(state, DateTime.UtcNow.Ticks + TimeSpan.FromMinutes(10).Ticks);

            glass.BlurRadius = 32;

            for (var i = 0; i < 6; i++)
                _ = window.CaptureRenderedFrame();

            var origin2 = GetProviderSnapshotOrigin(window);
            Assert.NotNull(origin2);

            Assert.NotEqual(origin1, origin2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlassSurface_clips_child_to_corner_radius()
    {
        var background = new Border
        {
            Background = Brushes.DarkRed
        };

        var glass = new LiquidGlassSurface
        {
            Width = 140,
            Height = 140,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(64),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false,
            Child = new Border
            {
                Background = Brushes.White
            }
        };

        var root = new Grid();
        root.Children.Add(background);
        root.Children.Add(glass);

        var window = new Window
        {
            Width = 320,
            Height = 240,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var withGlass = window.CaptureRenderedFrame();
            Assert.NotNull(withGlass);

            root.Children.Remove(glass);
            var withoutGlass = window.CaptureRenderedFrame();
            Assert.NotNull(withoutGlass);

            // (1,1) is inside the control bounds but well outside the rounded-rect.
            var a = GetPixel(withGlass!, 1, 1);
            var b = GetPixel(withoutGlass!, 1, 1);

            var diff = Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b);
            Assert.True(diff <= 5, $"Expected clipped corner to match backdrop, got diff={diff} (with={a}, without={b})");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlass_is_stable_when_scene_static()
    {
        var root = new Grid();

        var stripes = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < 60; i++)
        {
            stripes.Children.Add(new Border
            {
                Width = 8,
                Background = i % 2 == 0 ? Brushes.DeepSkyBlue : Brushes.Gold
            });
        }

        root.Children.Add(stripes);

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 160,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = true,
            HighlightOpacity = 0.5,
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();
            var frame1 = window.CaptureRenderedFrame();
            Assert.NotNull(frame1);

            Thread.Sleep(80);

            var frame2 = window.CaptureRenderedFrame();
            Assert.NotNull(frame2);

            var hash1 = ComputeBitmapHash(frame1!);
            var hash2 = ComputeBitmapHash(frame2!);
            Assert.Equal(hash1, hash2);

            MaybeSave(frame1!, "stable-frame-1.png");
            MaybeSave(frame2!, "stable-frame-2.png");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlassSurface_does_not_block_child_input()
    {
        var clicked = false;

        var button = new Button
        {
            Content = "Click",
            Width = 120,
            Height = 44,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        button.Click += (_, _) => clicked = true;

        var glass = new LiquidGlassSurface
        {
            Width = 220,
            Height = 140,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false,
            Child = button
        };

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = glass
        };

        window.Show();

        try
        {
            var p = new Point(window.Width / 2, window.Height / 2);
            window.MouseMove(p);
            window.MouseDown(p, Avalonia.Input.MouseButton.Left);
            window.MouseUp(p, Avalonia.Input.MouseButton.Left);
            Assert.True(clicked, "Expected click to reach the Button inside LiquidGlassSurface.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlassInteractiveSurface_does_not_block_child_input()
    {
        var clicked = false;

        var button = new Button
        {
            Content = "Click",
            Width = 120,
            Height = 44,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        button.Click += (_, _) => clicked = true;

        var glass = new LiquidGlassInteractiveSurface
        {
            IsInteractive = false,
            Width = 220,
            Height = 140,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(28),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false,
            Child = button
        };

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = glass
        };

        window.Show();

        try
        {
            var p = new Point(window.Width / 2, window.Height / 2);
            window.MouseMove(p);
            window.MouseDown(p, Avalonia.Input.MouseButton.Left);
            window.MouseUp(p, Avalonia.Input.MouseButton.Left);
            Assert.True(clicked, "Expected click to reach the Button inside LiquidGlassInteractiveSurface.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlassInteractiveSurface_applies_deformation_transform()
    {
        var glass = new LiquidGlassInteractiveSurface
        {
            Width = 160,
            Height = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(999),
            RefractionHeight = 12,
            RefractionAmount = 24,
            BlurRadius = 2,
            Vibrancy = 1.5,
            HighlightEnabled = false,
            Child = new TextBlock
            {
                Text = "Drag",
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var root = new Grid
        {
            Background = Brushes.Black,
            Children = { glass }
        };

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            // Ensure the visual tree is laid out before we convert positions.
            _ = window.CaptureRenderedFrame();

            using var pointer = new Avalonia.Input.Pointer(Avalonia.Input.Pointer.GetNextFreeId(), Avalonia.Input.PointerType.Mouse, true);

            var center = new Point(window.Width / 2, window.Height / 2);

            var pressedProps = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.LeftMouseButton, Avalonia.Input.PointerUpdateKind.LeftButtonPressed);
            glass.RaiseEvent(new Avalonia.Input.PointerPressedEventArgs(
                source: glass,
                pointer: pointer,
                rootVisual: window,
                rootVisualPosition: center,
                timestamp: 0,
                properties: pressedProps,
                modifiers: Avalonia.Input.KeyModifiers.None,
                clickCount: 1));

            var movedProps = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.LeftMouseButton, Avalonia.Input.PointerUpdateKind.Other);
            window.RaiseEvent(new Avalonia.Input.PointerEventArgs(
                Avalonia.Input.InputElement.PointerMovedEvent,
                source: window,
                pointer: pointer,
                rootVisual: window,
                rootVisualPosition: new Point(center.X + 40, center.Y + 8),
                timestamp: 1,
                properties: movedProps,
                modifiers: Avalonia.Input.KeyModifiers.None));

            Assert.NotNull(glass.RenderTransform);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LiquidGlass_highlight_corner_is_visible_under_render_transform()
    {
        var root = new Grid
        {
            Background = Brushes.Black
        };

        var glass = new LiquidGlassInteractiveSurface
        {
            IsInteractive = false,
            Width = 220,
            Height = 140,
            Margin = new Thickness(40),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(40),
            RefractionHeight = 0,
            RefractionAmount = 0,
            BlurRadius = 0,
            Vibrancy = 1,
            HighlightEnabled = true,
            HighlightOpacity = 1,
            HighlightWidth = 3,
            HighlightBlurRadius = 3,
            HighlightAngle = 45,
            HighlightFalloff = 1,
            Child = new Border
            {
                Background = Brushes.Transparent
            }
        };

        root.Children.Add(glass);

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Content = root
        };

        window.Show();

        try
        {
            _ = window.CaptureRenderedFrame();

            glass.RenderTransform = new MatrixTransform(Matrix.CreateScale(1.08, 1.04));

            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);

            var r = glass.CornerRadius.TopLeft;
            var cornerLocal = new Point(r * 0.35, r * 0.35);
            var topEdgeLocal = new Point(glass.Bounds.Width / 2, 2);
            var leftEdgeLocal = new Point(2, glass.Bounds.Height / 2);
            var centerLocal = new Point(glass.Bounds.Width / 2, glass.Bounds.Height / 2);

            var corner = glass.TranslatePoint(cornerLocal, window);
            var topEdge = glass.TranslatePoint(topEdgeLocal, window);
            var leftEdge = glass.TranslatePoint(leftEdgeLocal, window);
            var center = glass.TranslatePoint(centerLocal, window);

            Assert.True(corner.HasValue && topEdge.HasValue && leftEdge.HasValue && center.HasValue);

            var cCorner = GetPixel(frame!, (int)Math.Round(corner!.Value.X), (int)Math.Round(corner!.Value.Y));
            var cTop = GetPixel(frame!, (int)Math.Round(topEdge!.Value.X), (int)Math.Round(topEdge!.Value.Y));
            var cLeft = GetPixel(frame!, (int)Math.Round(leftEdge!.Value.X), (int)Math.Round(leftEdge!.Value.Y));
            var cCenter = GetPixel(frame!, (int)Math.Round(center!.Value.X), (int)Math.Round(center!.Value.Y));

            var lCorner = cCorner.r + cCorner.g + cCorner.b;
            var lTop = cTop.r + cTop.g + cTop.b;
            var lLeft = cLeft.r + cLeft.g + cLeft.b;
            var lCenter = cCenter.r + cCenter.g + cCenter.b;

            Assert.True(lCorner > lCenter + 10, $"Expected corner highlight to exceed center (corner={lCorner}, center={lCenter}).");
            Assert.True(lCorner >= Math.Min(lTop, lLeft) * 0.6, $"Expected corner highlight to be comparable to edges (corner={lCorner}, top={lTop}, left={lLeft}).");

            MaybeSave(frame!, "highlight-transform.png");
        }
        finally
        {
            window.Close();
        }
    }

    private static void MaybeSave(WriteableBitmap bitmap, string fileName)
    {
        var outputDir = Environment.GetEnvironmentVariable("LIQUIDGLASS_TEST_OUTPUT_DIR");
        if (string.IsNullOrWhiteSpace(outputDir))
            return;

        var resolvedDir = ResolveTestOutputDirectory(outputDir);
        Directory.CreateDirectory(resolvedDir);
        bitmap.Save(Path.Combine(resolvedDir, fileName));
    }

    private static string ResolveTestOutputDirectory(string outputDir)
    {
        if (Path.IsPathRooted(outputDir))
            return outputDir;

        // dotnet test runs from the testhost output folder, so resolve relative paths
        // against the solution root for stable artifact locations.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "LiquidGlassAvaloniaUI.sln")))
                return Path.GetFullPath(Path.Combine(dir.FullName, outputDir));

            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputDir));
    }

    private static ulong GetProviderSnapshotHash(TopLevel topLevel)
    {
        var state = GetProviderState(topLevel);
        if (state is null)
            return 0;

        var hashField = state.GetType().GetField("SnapshotHash", BindingFlags.Public | BindingFlags.Instance);
        if (hashField?.GetValue(state) is not ulong hash)
            return 0;

        return hash;
    }

    private static PixelPoint? GetProviderSnapshotOrigin(TopLevel topLevel)
    {
        var state = GetProviderState(topLevel);
        if (state is null)
            return null;

        var originField = state.GetType().GetField("SnapshotOriginInPixels", BindingFlags.Public | BindingFlags.Instance);
        if (originField?.GetValue(state) is not PixelPoint origin)
            return null;

        return origin;
    }

    private static object? GetProviderState(TopLevel topLevel)
    {
        var providerType = typeof(LiquidGlassSurface).Assembly.GetType("LiquidGlassAvaloniaUI.LiquidGlassBackdropProvider");
        if (providerType is null)
            return null;

        var statesField = providerType.GetField("s_states", BindingFlags.NonPublic | BindingFlags.Static);
        if (statesField?.GetValue(null) is not object states)
            return null;

        var tryGetValue = states.GetType().GetMethod("TryGetValue", BindingFlags.Public | BindingFlags.Instance);
        if (tryGetValue is null)
            return null;

        object?[] args = { topLevel, null };
        var found = tryGetValue.Invoke(states, args) as bool? ?? false;
        if (!found || args[1] is null)
            return null;

        return args[1];
    }

    private static (int r, int g, int b, int a) GetPixel(WriteableBitmap bitmap, int x, int y)
    {
        using var fb = bitmap.Lock();

        if (x < 0 || y < 0 || x >= fb.Size.Width || y >= fb.Size.Height)
            throw new ArgumentOutOfRangeException($"Pixel out of range ({x},{y}) for {fb.Size.Width}x{fb.Size.Height}");

        unsafe
        {
            var ptr = (byte*)fb.Address;
            var offset = y * fb.RowBytes + x * (fb.Format.BitsPerPixel / 8);

            if (fb.Format == PixelFormat.Bgra8888)
                return (ptr[offset + 2], ptr[offset + 1], ptr[offset + 0], ptr[offset + 3]);
            if (fb.Format == PixelFormat.Rgba8888)
                return (ptr[offset + 0], ptr[offset + 1], ptr[offset + 2], ptr[offset + 3]);

            throw new NotSupportedException($"Unsupported pixel format: {fb.Format}");
        }
    }

    private static double ComputeLineContrast(WriteableBitmap bitmap, int y, int x0, int x1, int step)
    {
        var prev = GetPixel(bitmap, x0, y);
        var sum = 0.0;
        var n = 0;

        for (var x = x0 + step; x <= x1; x += step)
        {
            var cur = GetPixel(bitmap, x, y);
            sum += Math.Abs(cur.r - prev.r) + Math.Abs(cur.g - prev.g) + Math.Abs(cur.b - prev.b);
            prev = cur;
            n++;
        }

        return n == 0 ? 0.0 : sum / n;
    }

    private static ulong ComputeBitmapHash(WriteableBitmap bitmap)
    {
        using var fb = bitmap.Lock();

        const ulong fnvOffset = 14695981039346656037UL;
        const ulong fnvPrime = 1099511628211UL;
        ulong hash = fnvOffset;

        unsafe
        {
            var ptr = (byte*)fb.Address;
            var rowBytes = fb.RowBytes;
            var bpp = fb.Format.BitsPerPixel / 8;

            const int samplesX = 12;
            const int samplesY = 12;
            var maxX = Math.Max(1, fb.Size.Width) - 1;
            var maxY = Math.Max(1, fb.Size.Height) - 1;

            for (var sy = 0; sy < samplesY; sy++)
            {
                var y = samplesY == 1 ? 0 : (int)((long)sy * maxY / (samplesY - 1));
                var row = y * rowBytes;

                for (var sx = 0; sx < samplesX; sx++)
                {
                    var x = samplesX == 1 ? 0 : (int)((long)sx * maxX / (samplesX - 1));
                    var offset = row + x * bpp;

                    for (var i = 0; i < bpp; i++)
                        hash = (hash ^ ptr[offset + i]) * fnvPrime;
                }
            }
        }

        hash = (hash ^ (ulong)bitmap.PixelSize.Width) * fnvPrime;
        hash = (hash ^ (ulong)bitmap.PixelSize.Height) * fnvPrime;
        return hash;
    }

    private static (int r, int g, int b) ApplyAndroidColorPipeline(
        Color input,
        double brightness,
        double contrast,
        double saturation,
        double exposureEv,
        double gammaPower)
    {
        var r0 = input.R;
        var g0 = input.G;
        var b0 = input.B;

        var (r1, g1, b1) = ApplyAndroidColorControls(r0, g0, b0, (float)brightness, (float)contrast, (float)saturation);
        var (r2, g2, b2) = ApplyAndroidExposure(r1, g1, b1, (float)exposureEv);
        var (r3, g3, b3) = ApplyAndroidGamma(r2, g2, b2, (float)gammaPower);

        return (r3, g3, b3);
    }

    private static (int r, int g, int b) ApplyAndroidColorControls(int r, int g, int b, float brightness, float contrast, float saturation)
    {
        var rIn = r / 255f;
        var gIn = g / 255f;
        var bIn = b / 255f;

        var invSat = 1f - saturation;
        var rr = 0.213f * invSat;
        var gg = 0.715f * invSat;
        var bb = 0.072f * invSat;

        var c = contrast;
        var t = (0.5f - c * 0.5f + brightness);
        var s = saturation;

        var cr = c * rr;
        var cg = c * gg;
        var cb = c * bb;
        var cs = c * s;

        var rf = (cr + cs) * rIn + cg * gIn + cb * bIn + t;
        var gf = cr * rIn + (cg + cs) * gIn + cb * bIn + t;
        var bf = cr * rIn + cg * gIn + (cb + cs) * bIn + t;

        return (ClampToByte(rf * 255f), ClampToByte(gf * 255f), ClampToByte(bf * 255f));
    }

    private static (int r, int g, int b) ApplyAndroidExposure(int r, int g, int b, float exposureEv)
    {
        if (Math.Abs(exposureEv) < 0.0005f)
            return (r, g, b);

        var scale = (float)Math.Pow(2.0, exposureEv / 2.2);
        return (ClampToByte(r * scale), ClampToByte(g * scale), ClampToByte(b * scale));
    }

    private static (int r, int g, int b) ApplyAndroidGamma(int r, int g, int b, float gammaPower)
    {
        if (Math.Abs(gammaPower - 1.0f) < 0.0005f)
            return (r, g, b);

        var p = Math.Max(gammaPower, 0.000001f);
        var rf = (float)Math.Pow(r / 255f, p) * 255f;
        var gf = (float)Math.Pow(g / 255f, p) * 255f;
        var bf = (float)Math.Pow(b / 255f, p) * 255f;
        return (ClampToByte(rf), ClampToByte(gf), ClampToByte(bf));
    }

    private static int ClampToByte(float v)
    {
        if (v < 0) return 0;
        if (v > 255) return 255;
        return (int)Math.Round(v);
    }
}
