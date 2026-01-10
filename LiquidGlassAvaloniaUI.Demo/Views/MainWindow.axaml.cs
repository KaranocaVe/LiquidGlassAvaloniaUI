using System;
using Avalonia;
using Avalonia.Controls;
#if DEBUG
using Avalonia.Diagnostics;
#endif
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LiquidGlassAvaloniaUI;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    private bool _isFloatingCardDragging;
    private Point _floatingCardDragStartPointer;
    private double _floatingCardDragStartLeft;
    private double _floatingCardDragStartTop;
    private Canvas? _overlayCanvas;
    private LiquidGlassSurface? _floatingCard;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _overlayCanvas = this.FindControl<Canvas>("OverlayCanvas");
        _floatingCard = this.FindControl<LiquidGlassSurface>("FloatingCard");

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

    private void OnFloatingCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (_overlayCanvas is null || _floatingCard is null)
            return;

        _isFloatingCardDragging = true;
        _floatingCardDragStartPointer = e.GetPosition(_overlayCanvas);

        var left = Canvas.GetLeft(_floatingCard);
        var top = Canvas.GetTop(_floatingCard);
        _floatingCardDragStartLeft = double.IsNaN(left) ? 0.0 : left;
        _floatingCardDragStartTop = double.IsNaN(top) ? 0.0 : top;

        e.Pointer.Capture(_floatingCard);
        e.Handled = true;
    }

    private void OnFloatingCardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isFloatingCardDragging)
            return;

        if (_overlayCanvas is null || _floatingCard is null)
            return;

        if (!ReferenceEquals(e.Pointer.Captured, _floatingCard))
            return;

        var p = e.GetPosition(_overlayCanvas);
        var dx = p.X - _floatingCardDragStartPointer.X;
        var dy = p.Y - _floatingCardDragStartPointer.Y;

        var left = _floatingCardDragStartLeft + dx;
        var top = _floatingCardDragStartTop + dy;

        var overlayBounds = _overlayCanvas.Bounds;
        var cardBounds = _floatingCard.Bounds;
        if (overlayBounds.Width > 0
            && overlayBounds.Height > 0
            && cardBounds.Width > 0
            && cardBounds.Height > 0)
        {
            var maxLeft = Math.Max(0.0, overlayBounds.Width - cardBounds.Width);
            var maxTop = Math.Max(0.0, overlayBounds.Height - cardBounds.Height);
            left = Math.Clamp(left, 0.0, maxLeft);
            top = Math.Clamp(top, 0.0, maxTop);
        }

        Canvas.SetLeft(_floatingCard, left);
        Canvas.SetTop(_floatingCard, top);
        e.Handled = true;
    }

    private void OnFloatingCardPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isFloatingCardDragging)
            return;

        EndFloatingCardDrag(e.Pointer);
        e.Handled = true;
    }

    private void OnFloatingCardPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_isFloatingCardDragging)
            return;

        EndFloatingCardDrag(pointer: null);
    }

    private void EndFloatingCardDrag(IPointer? pointer)
    {
        _isFloatingCardDragging = false;
        pointer?.Capture(null);
    }
}
