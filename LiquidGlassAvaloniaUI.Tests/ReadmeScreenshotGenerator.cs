using System;
using System.IO;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Shapes;
using ShapePath = Avalonia.Controls.Shapes.Path;
using Xunit;

namespace LiquidGlassAvaloniaUI.Tests;

public class ReadmeScreenshotGenerator
{
    [AvaloniaFact]
    public void Generate_readme_screenshots()
    {
        var outputDir = Environment.GetEnvironmentVariable("LIQUIDGLASS_README_SCREENSHOTS_DIR");
        if (string.IsNullOrWhiteSpace(outputDir))
            return;

        var resolvedDir = ResolveOutputDirectory(outputDir);
        Directory.CreateDirectory(resolvedDir);

        SaveShowcase(System.IO.Path.Combine(resolvedDir, "showcase.png"));
        SaveDistortion(System.IO.Path.Combine(resolvedDir, "distortion.png"));
        SaveMagnifier(System.IO.Path.Combine(resolvedDir, "magnifier.png"));
        SaveProgressiveBlur(System.IO.Path.Combine(resolvedDir, "progressive.png"));
    }

    private static void SaveShowcase(string path)
    {
        var root = CreateGlowBackdrop();
        root.Children.Add(CreateGuides(opacity: 0.14));
        root.Children.Add(new Rectangle
        {
            Fill = CreateDistortionBrush(),
            Opacity = 0.18,
            IsHitTestVisible = false
        });

        var glass = new LiquidGlassSurface
        {
            Width = 920,
            Height = 540,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(56),

            // Intentionally exaggerated for README: emphasize refraction + dispersion.
            RefractionHeight = 34.0,
            RefractionAmount = 120.0,
            DepthEffect = true,
            ChromaticAberration = true,

            BlurRadius = 0.0,
            Vibrancy = 1.3,
            Brightness = 0.04,
            Contrast = 1.0,
            ExposureEv = 0.0,
            GammaPower = 1.0,
            BackdropOpacity = 1.0,

            TintColor = Colors.Transparent,
            SurfaceColor = Color.FromArgb(10, 255, 255, 255),

            HighlightEnabled = true,
            HighlightWidth = 0.65,
            HighlightBlurRadius = 0.4,
            HighlightOpacity = 0.5,
            HighlightAngle = 55.0,
            HighlightFalloff = 1.2,

            ShadowEnabled = true,
            ShadowRadius = 30.0,
            ShadowOffset = new Vector(0.0, 12.0),
            ShadowOpacity = 1.0,
            ShadowColor = Color.FromArgb(70, 0, 0, 0),

            InnerShadowEnabled = false,

            Child = new Grid
            {
                Children =
                {
                    new Border
                    {
                        Margin = new Thickness(2),
                        BorderBrush = new SolidColorBrush(Color.Parse("#66FFFFFF")),
                        BorderThickness = new Thickness(1.5),
                        CornerRadius = new CornerRadius(54)
                    },
                    CreateShowcaseOverlayContent()
                }
            }
        };

        root.Children.Add(glass);

        SaveWindow(path, root);
    }

    private static void SaveDistortion(string path)
    {
        var root = new Grid
        {
            Background = CreateDistortionBrush()
        };

        root.Children.Add(new Rectangle
        {
            Fill = new SolidColorBrush(Color.Parse("#FF0B0C10")),
            Opacity = 0.14
        });

        root.Children.Add(new TextBlock
        {
            Text = "DISTORTION",
            FontSize = 84,
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#22FFFFFF")),
            Margin = new Thickness(54, 48, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        });

        var glass = new LiquidGlassSurface
        {
            Width = 920,
            Height = 540,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(56),

            // Intentionally exaggerated for README: show where the lens distorts the backdrop.
            RefractionHeight = 44.0,
            RefractionAmount = 128.0,
            DepthEffect = true,
            ChromaticAberration = true,

            BlurRadius = 0.0,
            Vibrancy = 1.0,
            Brightness = 0.0,
            Contrast = 1.0,
            ExposureEv = 0.0,
            GammaPower = 1.0,
            BackdropOpacity = 1.0,

            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent,

            HighlightEnabled = true,
            HighlightWidth = 0.6,
            HighlightBlurRadius = 0.35,
            HighlightOpacity = 0.38,
            HighlightAngle = 60.0,
            HighlightFalloff = 1.2,

            ShadowEnabled = true,
            ShadowRadius = 30.0,
            ShadowOffset = new Vector(0.0, 14.0),
            ShadowOpacity = 1.0,
            ShadowColor = Color.FromArgb(80, 0, 0, 0),
            InnerShadowEnabled = false,

            Child = CreateGlassOutlineOverlay(
                title: "Refraction + Chromatic Aberration",
                subtitle: "Transparent lens (no blur / no tint)")
        };

        root.Children.Add(glass);

        SaveWindow(path, root);
    }

    private static void SaveMagnifier(string path)
    {
        var root = CreateGlowBackdrop();

        var label = new TextBlock
        {
            Text = "MAGNIFIER",
            FontSize = 96,
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#26FFFFFF")),
            Margin = new Thickness(54, 48, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        root.Children.Add(label);

        var glass = new LiquidGlassSurface
        {
            Width = 760,
            Height = 480,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 90, 0),
            CornerRadius = new CornerRadius(56),

            BackdropZoom = 1.55,
            BackdropOffset = new Vector(0.0, -120.0),

            RefractionHeight = 18.0,
            RefractionAmount = 86.0,
            DepthEffect = true,
            ChromaticAberration = true,

            BlurRadius = 0.0,
            Vibrancy = 1.0,
            Brightness = 0.0,
            Contrast = 1.0,
            ExposureEv = 0.0,
            GammaPower = 1.0,
            BackdropOpacity = 1.0,

            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent,

            HighlightEnabled = true,
            HighlightWidth = 0.5,
            HighlightBlurRadius = 0.35,
            HighlightOpacity = 0.4,
            HighlightAngle = 55.0,
            HighlightFalloff = 1.2,

            ShadowEnabled = true,
            ShadowRadius = 26.0,
            ShadowOffset = new Vector(0, 12),
            ShadowOpacity = 1.0,
            ShadowColor = Color.FromArgb(90, 0, 0, 0),

            InnerShadowEnabled = true,
            InnerShadowRadius = 16.0,
            InnerShadowOffset = new Vector(0, 10),
            InnerShadowOpacity = 1.0,
            InnerShadowColor = Color.FromArgb(110, 0, 0, 0),

            Child = new Grid
            {
                Margin = new Thickness(42),
                RowDefinitions = new RowDefinitions("Auto,Auto,*"),
                Children =
                {
                    new TextBlock
                    {
                        Text = "Backdrop transform",
                        FontSize = 22,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brushes.White
                    },
                    new TextBlock
                    {
                        Text = "Zoom + offset sampling (magnifier-style).",
                        Margin = new Thickness(0, 10, 0, 0),
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Color.Parse("#CCFFFFFF"))
                    }
                }
            }
        };
        Grid.SetRow(((Grid)glass.Child!).Children[1], 1);

        root.Children.Add(glass);

        SaveWindow(path, root);
    }

    private static void SaveProgressiveBlur(string path)
    {
        var root = CreateProgressiveBackdrop();
        root.Children.Add(new Rectangle
        {
            Fill = CreateDistortionBrush(),
            Opacity = 0.1,
            IsHitTestVisible = false
        });

        var glass = new LiquidGlassSurface
        {
            Width = 920,
            Height = 540,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(56),

            RefractionHeight = 36.0,
            RefractionAmount = 120.0,
            DepthEffect = true,
            ChromaticAberration = true,

            BlurRadius = 8.0,
            Vibrancy = 1.35,
            Brightness = 0.05,
            Contrast = 1.0,
            ExposureEv = 0.0,
            GammaPower = 1.0,
            BackdropOpacity = 1.0,

            ProgressiveBlurEnabled = true,
            ProgressiveBlurStart = 0.28,
            ProgressiveBlurEnd = 0.96,
            ProgressiveTintColor = Colors.Transparent,
            ProgressiveTintIntensity = 0.0,

            TintColor = Colors.Transparent,
            SurfaceColor = Colors.Transparent,

            HighlightEnabled = true,
            HighlightWidth = 0.55,
            HighlightBlurRadius = 0.35,
            HighlightOpacity = 0.32,
            HighlightAngle = 55.0,
            HighlightFalloff = 1.2,

            ShadowEnabled = true,
            ShadowRadius = 26.0,
            ShadowOffset = new Vector(0.0, 12.0),
            ShadowOpacity = 1.0,
            ShadowColor = Color.FromArgb(70, 0, 0, 0),
            InnerShadowEnabled = false,

            Child = CreateGlassOutlineOverlay(
                title: "PROGRESSIVE BLUR",
                subtitle: "Top is blurred; bottom fades to clear")
        };

        root.Children.Add(glass);

        SaveWindow(path, root);
    }

    private static Control CreateShowcaseOverlayContent()
    {
        var panel = new Grid
        {
            Margin = new Thickness(52),
            RowDefinitions = new RowDefinitions("Auto,Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            RowSpacing = 12
        };

        panel.Children.Add(new TextBlock
        {
            Text = "LiquidGlassSurface",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });

        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#22FFFFFF")),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(14, 8),
            HorizontalAlignment = HorizontalAlignment.Right,
            Child = new TextBlock
            {
                Text = "transparent · refractive · Skia",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White
            }
        };
        Grid.SetColumn(badge, 1);
        panel.Children.Add(badge);

        var subtitle = new TextBlock
        {
            Text = "Refraction + depth + chromatic aberration (minimal blur)",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#CCFFFFFF"))
        };
        Grid.SetRow(subtitle, 1);
        Grid.SetColumnSpan(subtitle, 2);
        panel.Children.Add(subtitle);

        var chips = new WrapPanel
        {
            ItemSpacing = 10,
            LineSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        Grid.SetRow(chips, 2);
        Grid.SetColumnSpan(chips, 2);
        panel.Children.Add(chips);

        chips.Children.Add(CreateChip("Lens"));
        chips.Children.Add(CreateChip("Stable backdrop"));
        chips.Children.Add(CreateChip("Highlight + shadow"));

        return panel;
    }

    private static Control CreateChip(string text)
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.Parse("#16FFFFFF")),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(14, 8),
            Child = new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White
            }
        };
    }

    private static Control CreateGlassOutlineOverlay(string title, string subtitle)
    {
        var root = new Grid();

        root.Children.Add(new Rectangle
        {
            Margin = new Thickness(10),
            Stroke = new SolidColorBrush(Color.Parse("#B3FFFFFF")),
            StrokeThickness = 3,
            StrokeDashArray = new AvaloniaList<double> { 12, 10 },
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            RadiusX = 46,
            RadiusY = 46,
            Fill = null
        });

        root.Children.Add(new TextBlock
        {
            Text = "GLASS",
            FontSize = 84,
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#10FFFFFF")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        });

        root.Children.Add(new Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new SolidColorBrush(Color.Parse("#FF00E5FF")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(22)
        });

        root.Children.Add(new Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new SolidColorBrush(Color.Parse("#FFFF007A")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(22)
        });

        root.Children.Add(new Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new SolidColorBrush(Color.Parse("#FFFF8D28")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(22)
        });

        root.Children.Add(new Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new SolidColorBrush(Color.Parse("#FF00C8A0")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(22)
        });

        var panel = new StackPanel
        {
            Margin = new Thickness(54, 44, 54, 0),
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });

        panel.Children.Add(new TextBlock
        {
            Text = subtitle,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#CCFFFFFF"))
        });

        root.Children.Add(panel);
        return root;
    }

    private static Canvas CreateGuides(double opacity)
    {
        const int w = 1920;
        const int h = 1080;

        var canvas = new Canvas
        {
            IsHitTestVisible = false,
            Opacity = opacity
        };

        canvas.Children.Add(new Line
        {
            StartPoint = new Point(-80, h * 0.26),
            EndPoint = new Point(w + 80, h * 0.72),
            Stroke = new SolidColorBrush(Color.Parse("#30FF8D28")),
            StrokeThickness = 5,
            StrokeLineCap = PenLineCap.Round
        });

        canvas.Children.Add(new Line
        {
            StartPoint = new Point(-80, h * 0.74),
            EndPoint = new Point(w + 80, h * 0.28),
            Stroke = new SolidColorBrush(Color.Parse("#300091FF")),
            StrokeThickness = 5,
            StrokeLineCap = PenLineCap.Round
        });

        return canvas;
    }

    private static Grid CreateGlowBackdrop()
    {
        var root = new Grid
        {
            Background = new SolidColorBrush(Color.Parse("#FF0B0C10"))
        };

        var canvas = new Canvas { IsHitTestVisible = false };
        root.Children.Add(canvas);

        var liquid = new TextBlock
        {
            Text = "LIQUID",
            FontSize = 180,
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#18FFFFFF"))
        };
        Canvas.SetLeft(liquid, 28);
        Canvas.SetTop(liquid, 40);
        canvas.Children.Add(liquid);

        var glass = new TextBlock
        {
            Text = "GLASS",
            FontSize = 180,
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#12FFFFFF"))
        };
        Canvas.SetLeft(glass, 28);
        Canvas.SetTop(glass, 190);
        canvas.Children.Add(glass);

        canvas.Children.Add(CreateGlowEllipse(
            width: 760,
            height: 760,
            left: -260,
            top: -320,
            centerX: 0.35,
            centerY: 0.35,
            color: Color.Parse("#B30091FF")));

        canvas.Children.Add(CreateGlowEllipse(
            width: 820,
            height: 820,
            left: 760,
            top: 140,
            centerX: 0.6,
            centerY: 0.4,
            color: Color.Parse("#A0FF007A")));

        canvas.Children.Add(CreateGlowEllipse(
            width: 620,
            height: 620,
            left: 60,
            top: 560,
            centerX: 0.4,
            centerY: 0.4,
            color: Color.Parse("#90FF8D28")));

        root.Children.Add(new Rectangle
        {
            Fill = new RadialGradientBrush
            {
                Center = new RelativePoint(0.5, 0.4, RelativeUnit.Relative),
                GradientOrigin = new RelativePoint(0.5, 0.4, RelativeUnit.Relative),
                RadiusX = new RelativeScalar(0.95, RelativeUnit.Relative),
                RadiusY = new RelativeScalar(0.95, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#00000000"), 0),
                    new GradientStop(Color.Parse("#4D000000"), 1),
                }
            }
        });

        return root;
    }

    private static Ellipse CreateGlowEllipse(
        double width,
        double height,
        double left,
        double top,
        double centerX,
        double centerY,
        Color color)
    {
        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,
            Fill = new RadialGradientBrush
            {
                Center = new RelativePoint(centerX, centerY, RelativeUnit.Relative),
                GradientOrigin = new RelativePoint(centerX, centerY, RelativeUnit.Relative),
                RadiusX = new RelativeScalar(0.9, RelativeUnit.Relative),
                RadiusY = new RelativeScalar(0.9, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(color, 0),
                    new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1),
                }
            }
        };

        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        return ellipse;
    }

    private static Control CreateShowcaseCardContent()
    {
        var panel = new Grid
        {
            Margin = new Thickness(44),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            RowSpacing = 16
        };

        panel.Children.Add(new TextBlock
        {
            Text = "LiquidGlassSurface",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });

        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#28FFFFFF")),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(14, 8),
            HorizontalAlignment = HorizontalAlignment.Right,
            Child = new TextBlock
            {
                Text = "Avalonia + Skia (headless)",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White
            }
        };
        Grid.SetColumn(badge, 1);
        panel.Children.Add(badge);

        var subtitle = new TextBlock
        {
            Text = "Vibrancy → Blur → Lens refraction → Gamma → Surface → Highlight",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#CCFFFFFF"))
        };
        Grid.SetRow(subtitle, 1);
        Grid.SetColumnSpan(subtitle, 2);
        panel.Children.Add(subtitle);

        var bullets = new StackPanel
        {
            Spacing = 12
        };
        Grid.SetRow(bullets, 2);
        Grid.SetColumnSpan(bullets, 2);
        panel.Children.Add(bullets);

        bullets.Children.Add(CreateBullet("Lens distortion", "Rounded-rect refraction with optional depth + CA."));
        bullets.Children.Add(CreateBullet("Backdrop capture", "Fast snapshot caching with stable origin."));
        bullets.Children.Add(CreateBullet("Refraction shader", "Rounded-rect lens with optional dispersion."));

        return panel;
    }

    private static Control CreateBullet(string title, string description)
    {
        var root = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#14FFFFFF")),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(16, 14)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 14
        };
        root.Child = grid;

        var dot = new Border
        {
            Width = 30,
            Height = 30,
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.Parse("#28FFFFFF"))
        };
        grid.Children.Add(dot);

        var text = new StackPanel
        {
            Spacing = 4
        };
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);

        text.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White
        });
        text.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#B3FFFFFF"))
        });

        return root;
    }

    private static IBrush CreateDistortionBrush()
    {
        var tile = new Grid
        {
            Width = 240,
            Height = 240
        };

        tile.Children.Add(new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#FF001DFF"), 0),
                    new GradientStop(Color.Parse("#FFFF007A"), 0.52),
                    new GradientStop(Color.Parse("#FFFF8D28"), 1),
                }
            }
        });

        tile.Children.Add(new Rectangle
        {
            Fill = new SolidColorBrush(Color.Parse("#FF0B0C10")),
            Opacity = 0.45
        });

        tile.Children.Add(new ShapePath
        {
            Data = Geometry.Parse("M0 60 H240 M0 120 H240 M0 180 H240 M60 0 V240 M120 0 V240 M180 0 V240"),
            Stroke = new SolidColorBrush(Color.Parse("#80FFFFFF")),
            StrokeThickness = 1
        });

        tile.Children.Add(new ShapePath
        {
            Data = Geometry.Parse("M0 120 H240 M120 0 V240"),
            Stroke = new SolidColorBrush(Color.Parse("#E6FFFFFF")),
            StrokeThickness = 2
        });

        tile.Children.Add(new ShapePath
        {
            Data = Geometry.Parse("M0 0 L240 240 M240 0 L0 240"),
            Stroke = new SolidColorBrush(Color.Parse("#40FFFFFF")),
            StrokeThickness = 1
        });

        tile.Children.Add(new Ellipse
        {
            Width = 24,
            Height = 24,
            Fill = new SolidColorBrush(Color.Parse("#FF00E5FF")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(18)
        });

        tile.Children.Add(new Ellipse
        {
            Width = 24,
            Height = 24,
            Fill = new SolidColorBrush(Color.Parse("#FFFF007A")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(18)
        });

        tile.Children.Add(new Ellipse
        {
            Width = 24,
            Height = 24,
            Fill = new SolidColorBrush(Color.Parse("#FFFF8D28")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(18)
        });

        tile.Children.Add(new Ellipse
        {
            Width = 24,
            Height = 24,
            Fill = new SolidColorBrush(Color.Parse("#FF00C8A0")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(18)
        });

        return new VisualBrush
        {
            TileMode = TileMode.Tile,
            Stretch = Stretch.Fill,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            DestinationRect = new RelativeRect(0, 0, 240, 240, RelativeUnit.Absolute),
            Visual = tile
        };
    }

    private static Grid CreateProgressiveBackdrop()
    {
        var root = new Grid();

        root.Children.Add(new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#FF001DFF"), 0),
                    new GradientStop(Color.Parse("#FF0B0C10"), 0.45),
                    new GradientStop(Color.Parse("#FFFF8D28"), 1),
                }
            }
        });

        var stripes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Opacity = 0.9,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var stripeColors = new[]
        {
            Color.Parse("#FF00E5FF"),
            Color.Parse("#FFFF007A"),
            Color.Parse("#FFFF8D28"),
            Color.Parse("#FF00C8A0"),
        };

        for (var i = 0; i < 64; i++)
        {
            var bright = stripeColors[(i / 2) % stripeColors.Length];
            var isBright = i % 2 == 0;
            var stripeColor = isBright ? bright : Color.Parse("#FF0B0C10");

            stripes.Children.Add(new Border
            {
                Width = 28,
                Height = 620,
                Background = new SolidColorBrush(stripeColor),
                Opacity = isBright ? 0.38 : 0.3
            });
        }

        root.Children.Add(stripes);

        root.Children.Add(new TextBlock
        {
            Text = "MASK",
            FontSize = 180,
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#14FFFFFF")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(48, 0, 0, 58)
        });

        root.Children.Add(new Rectangle
        {
            Fill = new RadialGradientBrush
            {
                Center = new RelativePoint(0.5, 0.4, RelativeUnit.Relative),
                GradientOrigin = new RelativePoint(0.5, 0.4, RelativeUnit.Relative),
                RadiusX = new RelativeScalar(0.95, RelativeUnit.Relative),
                RadiusY = new RelativeScalar(0.95, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#00000000"), 0),
                    new GradientStop(Color.Parse("#4D000000"), 1),
                }
            }
        });

        return root;
    }

    private static void SaveWindow(string path, Control content)
    {
        var window = new Window
        {
            Width = 1920,
            Height = 1080,
            Content = content
        };

        window.Show();

        try
        {
            // First frame triggers backdrop capture; second should use it.
            _ = window.CaptureRenderedFrame();
            _ = window.CaptureRenderedFrame();
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);

            frame!.Save(path);
            Assert.True(File.Exists(path), $"Expected screenshot at {path}");
        }
        finally
        {
            window.Close();
        }
    }

    private static string ResolveOutputDirectory(string outputDir)
    {
        if (System.IO.Path.IsPathRooted(outputDir))
            return outputDir;

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(System.IO.Path.Combine(dir.FullName, "LiquidGlassAvaloniaUI.sln")))
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(dir.FullName, outputDir));

            dir = dir.Parent;
        }

        return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, outputDir));
    }
}
