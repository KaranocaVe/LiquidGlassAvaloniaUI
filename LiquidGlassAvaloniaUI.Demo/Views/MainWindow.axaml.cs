using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
    private Border? _perfPanel;
    private TextBlock? _perfCaptureText;
    private TextBlock? _perfSkipText;
    private TextBlock? _perfCopyText;
    private TextBlock? _perfFilterText;
    private TextBlock? _perfDirtyText;
    private TextBlock? _perfCaptureRateText;
    private TextBlock? _perfSkipRateText;
    private TextBlock? _perfCopyRateText;
    private TextBlock? _perfFilterRateText;
    private PerfChart? _perfChart;
    private DispatcherTimer? _perfTimer;
    private LiquidGlassDiagnosticsSnapshot _lastPerfSnapshot;
    private DateTime _lastPerfSampleTimeUtc;
    private bool _hasLastPerfSample;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _overlayCanvas = this.FindControl<Canvas>("OverlayCanvas");
        _floatingCard = this.FindControl<LiquidGlassSurface>("FloatingCard");
        _perfPanel = this.FindControl<Border>("PerfPanel");
        _perfCaptureText = this.FindControl<TextBlock>("PerfCaptureText");
        _perfSkipText = this.FindControl<TextBlock>("PerfSkipText");
        _perfCopyText = this.FindControl<TextBlock>("PerfCopyText");
        _perfFilterText = this.FindControl<TextBlock>("PerfFilterText");
        _perfDirtyText = this.FindControl<TextBlock>("PerfDirtyText");
        _perfCaptureRateText = this.FindControl<TextBlock>("PerfCaptureRateText");
        _perfSkipRateText = this.FindControl<TextBlock>("PerfSkipRateText");
        _perfCopyRateText = this.FindControl<TextBlock>("PerfCopyRateText");
        _perfFilterRateText = this.FindControl<TextBlock>("PerfFilterRateText");
        _perfChart = this.FindControl<PerfChart>("PerfChart");

        _perfTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _perfTimer.Tick += (_, _) => UpdatePerfPanel();
    }

    protected override void OnClosed(EventArgs e)
    {
        _perfTimer?.Stop();
        base.OnClosed(e);
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

    private void OnPerfToggleClick(object? sender, RoutedEventArgs e)
    {
        if (_perfPanel is null)
            return;

        bool show = !_perfPanel.IsVisible;
        _perfPanel.IsVisible = show;

        if (show)
        {
            _hasLastPerfSample = false;
            UpdatePerfPanel();
            _perfTimer?.Start();
        }
        else
        {
            _perfTimer?.Stop();
        }
    }

    private void OnPerfResetClick(object? sender, RoutedEventArgs e)
    {
        LiquidGlassDiagnostics.Reset();
        _perfChart?.Clear();
        _lastPerfSnapshot = LiquidGlassDiagnostics.Snapshot;
        _lastPerfSampleTimeUtc = DateTime.UtcNow;
        _hasLastPerfSample = true;
        UpdatePerfPanel();
    }

    private void UpdatePerfPanel()
    {
        LiquidGlassDiagnosticsSnapshot s = LiquidGlassDiagnostics.Snapshot;
        DateTime now = DateTime.UtcNow;

        double captureRate = 0.0;
        double skipRate = 0.0;
        double copyRateMb = 0.0;
        double filterMissRate = 0.0;

        if (_hasLastPerfSample)
        {
            double seconds = Math.Max(0.001, (now - _lastPerfSampleTimeUtc).TotalSeconds);
            long skips = Delta(s.CapturesSkippedByCadence, _lastPerfSnapshot.CapturesSkippedByCadence)
                         + Delta(s.CapturesSkippedByDirtyRect, _lastPerfSnapshot.CapturesSkippedByDirtyRect)
                         + Delta(s.CapturesSkippedByHash, _lastPerfSnapshot.CapturesSkippedByHash);
            long copyBytes = Delta(s.FullBitmapCopyBytes, _lastPerfSnapshot.FullBitmapCopyBytes)
                             + Delta(s.SampledHashBytes, _lastPerfSnapshot.SampledHashBytes);

            captureRate = Delta(s.CapturesPublished, _lastPerfSnapshot.CapturesPublished) / seconds;
            skipRate = skips / seconds;
            copyRateMb = copyBytes / (1024.0 * 1024.0) / seconds;
            filterMissRate = Delta(s.FilterCacheMisses, _lastPerfSnapshot.FilterCacheMisses) / seconds;
            _perfChart?.AddSample(captureRate, skipRate, copyRateMb, filterMissRate);
        }

        _lastPerfSnapshot = s;
        _lastPerfSampleTimeUtc = now;
        _hasLastPerfSample = true;

        SetText(_perfCaptureRateText, $"captures {captureRate:F1}/s");
        SetText(_perfSkipRateText, $"skips {skipRate:F1}/s");
        SetText(_perfCopyRateText, $"copy {copyRateMb:F1} MB/s");
        SetText(_perfFilterRateText, $"miss {filterMissRate:F1}/s");
        SetText(_perfCaptureText, $"capture: queued {s.CaptureQueueRequests} / coalesced {s.CaptureQueueCoalesced} / started {s.CapturesStarted} / published {s.CapturesPublished}");
        SetText(_perfSkipText, $"skip: cadence {s.CapturesSkippedByCadence} / dirty {s.CapturesSkippedByDirtyRect} / hash {s.CapturesSkippedByHash} / empty {s.CapturesSkippedEmptyClip}");
        SetText(_perfCopyText, $"copy: full {s.FullBitmapCopies} ({FormatBytes(s.FullBitmapCopyBytes)}) / sampled {s.SampledHashChecks} ({FormatBytes(s.SampledHashBytes)})");
        SetText(_perfFilterText, $"filter: identity {s.FilterIdentityHits} / hit {s.FilterCacheHits} / miss {s.FilterCacheMisses} / gpu {s.FilterGpuSurfaces} / cpu {s.FilterCpuSurfaces}");
        SetText(_perfDirtyText, $"dirty: invalidations {s.RendererInvalidations} / rect {s.DirtyRectAvailable} / no-rect {s.DirtyRectUnavailable} / subscriber invalidates {s.SubscriberInvalidations}");
    }

    private static void SetText(TextBlock? textBlock, string text)
    {
        if (textBlock is not null)
            textBlock.Text = text;
    }

    private static string FormatBytes(long bytes)
    {
        const double kb = 1024.0;
        const double mb = kb * 1024.0;
        if (bytes >= mb)
            return $"{bytes / mb:F1} MB";
        if (bytes >= kb)
            return $"{bytes / kb:F1} KB";
        return $"{bytes} B";
    }

    private static long Delta(long current, long previous)
    {
        return Math.Max(0, current - previous);
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
