using System;
using Avalonia.Media;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private double _refractionHeight = 12.0;
    private double _refractionAmount = 24.0;
    private double _blurRadius = 2.0;
    private double _vibrancy = 1.5;
    private bool _depthEffect;
    private bool _chromaticAberration;

    private bool _highlightEnabled = true;
    private double _highlightOpacity = 0.5;

    private bool _enableTint;
    private int _tintPresetIndex = 0;
    private bool _enableSurface;
    private int _surfacePresetIndex = 0;

    public double RefractionHeight
    {
        get => _refractionHeight;
        set => this.RaiseAndSetIfChanged(ref _refractionHeight, value);
    }

    public double RefractionAmount
    {
        get => _refractionAmount;
        set => this.RaiseAndSetIfChanged(ref _refractionAmount, value);
    }

    public double BlurRadius
    {
        get => _blurRadius;
        set => this.RaiseAndSetIfChanged(ref _blurRadius, value);
    }

    public double Vibrancy
    {
        get => _vibrancy;
        set => this.RaiseAndSetIfChanged(ref _vibrancy, value);
    }

    public bool DepthEffect
    {
        get => _depthEffect;
        set => this.RaiseAndSetIfChanged(ref _depthEffect, value);
    }

    public bool ChromaticAberration
    {
        get => _chromaticAberration;
        set => this.RaiseAndSetIfChanged(ref _chromaticAberration, value);
    }

    public bool HighlightEnabled
    {
        get => _highlightEnabled;
        set => this.RaiseAndSetIfChanged(ref _highlightEnabled, value);
    }

    public double HighlightOpacity
    {
        get => _highlightOpacity;
        set => this.RaiseAndSetIfChanged(ref _highlightOpacity, value);
    }

    public bool EnableTint
    {
        get => _enableTint;
        set
        {
            this.RaiseAndSetIfChanged(ref _enableTint, value);
            this.RaisePropertyChanged(nameof(TintColor));
        }
    }

    public int TintPresetIndex
    {
        get => _tintPresetIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _tintPresetIndex, value);
            this.RaisePropertyChanged(nameof(TintColor));
        }
    }

    public Color TintColor => EnableTint ? TintPresets[Math.Clamp(TintPresetIndex, 0, TintPresets.Length - 1)] : Colors.Transparent;

    public bool EnableSurface
    {
        get => _enableSurface;
        set
        {
            this.RaiseAndSetIfChanged(ref _enableSurface, value);
            this.RaisePropertyChanged(nameof(SurfaceColor));
        }
    }

    public int SurfacePresetIndex
    {
        get => _surfacePresetIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _surfacePresetIndex, value);
            this.RaisePropertyChanged(nameof(SurfaceColor));
        }
    }

    public Color SurfaceColor => EnableSurface ? SurfacePresets[Math.Clamp(SurfacePresetIndex, 0, SurfacePresets.Length - 1)] : Colors.Transparent;

    private static readonly Color[] TintPresets =
    {
        Color.FromRgb(0, 136, 255),   // blue
        Color.FromRgb(255, 0, 122),   // pink
        Color.FromRgb(0, 200, 160),   // teal
        Color.FromRgb(255, 141, 40),  // amber (Android catalog: 0xFFFF8D28)
    };

    private static readonly Color[] SurfacePresets =
    {
        Color.FromArgb(77, 255, 255, 255), // ~0.3
        Color.FromArgb(77, 0, 0, 0),
    };
}
