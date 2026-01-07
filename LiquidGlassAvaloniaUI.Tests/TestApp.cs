using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Simple;

namespace LiquidGlassAvaloniaUI.Tests;

public class TestApp : Application
{
    public TestApp()
    {
        Styles.Add(new SimpleTheme());
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        });
}

