using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using LiquidGlassAvaloniaUI;
using AvaloniaApplication1;

namespace LiquidGlassAvaloniaUI.Demo.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .UseLiquidGlassPerformanceDefaults()
        .LogToTrace()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
