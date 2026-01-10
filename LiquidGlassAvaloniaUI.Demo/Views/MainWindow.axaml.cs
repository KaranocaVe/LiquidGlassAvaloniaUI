using Avalonia;
using Avalonia.Controls;
#if DEBUG
using Avalonia.Diagnostics;
#endif
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LiquidGlassAvaloniaUI;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShaderDebugger.TestShaderLoading();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void OnPrimaryLiquidButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.EnableTint = true;
        vm.TintPresetIndex = (vm.TintPresetIndex + 1) % 4;
    }

    private void OnSecondaryLiquidButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.EnableSurface = true;
        vm.SurfacePresetIndex = (vm.SurfacePresetIndex + 1) % 2;
    }

    private void OnPresetDefaultsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ApplyPresetDefaults();
    }

    private void OnPresetShowcaseClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ApplyPresetShowcase();
    }

    private void OnPresetMagnifierClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ApplyPresetMagnifier();
    }

    private void OnPresetProgressiveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ApplyPresetProgressiveBlur();
    }

    private void OnPresetDistortionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ApplyPresetDistortion();
    }

    private void OnPresetAdaptiveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.ApplyPresetAdaptiveLuminance();
    }
}
