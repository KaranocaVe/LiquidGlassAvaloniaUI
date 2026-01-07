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

        Directory.CreateDirectory(outputDir);
        bitmap.Save(Path.Combine(outputDir, fileName));
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
}
