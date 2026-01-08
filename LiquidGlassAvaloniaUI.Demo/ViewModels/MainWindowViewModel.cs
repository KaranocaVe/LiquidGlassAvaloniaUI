using System;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private double _refractionHeight = 12.0;
    private double _refractionAmount = 24.0;
    private double _blurRadius = 2.0;
    private double _vibrancy = 1.5;
    private double _brightness = 0.0;
    private double _contrast = 1.0;
    private double _exposureEv = 0.0;
    private double _gammaPower = 1.0;
    private double _backdropOpacity = 1.0;
    private bool _depthEffect;
    private bool _chromaticAberration;

    private bool _highlightEnabled = true;
    private double _highlightWidth = 0.5;
    private double _highlightBlurRadius = 0.25;
    private double _highlightOpacity = 0.5;
    private double _highlightAngle = 45.0;
    private double _highlightFalloff = 1.0;

    private bool _shadowEnabled = true;
    private double _shadowRadius = 24.0;
    private double _shadowOffsetX = 0.0;
    private double _shadowOffsetY = 4.0;
    private double _shadowOpacity = 1.0;
    private Color _shadowColor = Color.FromArgb(26, 0, 0, 0);

    private bool _innerShadowEnabled;
    private double _innerShadowRadius = 24.0;
    private double _innerShadowOffsetX = 0.0;
    private double _innerShadowOffsetY = 24.0;
    private double _innerShadowOpacity = 1.0;
    private Color _innerShadowColor = Color.FromArgb(38, 0, 0, 0);

    private bool _isInteractive = true;
    private bool _interactiveHighlightEnabled = true;
    private double _interactiveMaxScaleDip = 4.0;

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

    public double Brightness
    {
        get => _brightness;
        set => this.RaiseAndSetIfChanged(ref _brightness, value);
    }

    public double Contrast
    {
        get => _contrast;
        set => this.RaiseAndSetIfChanged(ref _contrast, value);
    }

    public double ExposureEv
    {
        get => _exposureEv;
        set => this.RaiseAndSetIfChanged(ref _exposureEv, value);
    }

    public double GammaPower
    {
        get => _gammaPower;
        set => this.RaiseAndSetIfChanged(ref _gammaPower, value);
    }

    public double BackdropOpacity
    {
        get => _backdropOpacity;
        set => this.RaiseAndSetIfChanged(ref _backdropOpacity, value);
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

    public double HighlightWidth
    {
        get => _highlightWidth;
        set => this.RaiseAndSetIfChanged(ref _highlightWidth, value);
    }

    public double HighlightBlurRadius
    {
        get => _highlightBlurRadius;
        set => this.RaiseAndSetIfChanged(ref _highlightBlurRadius, value);
    }

    public double HighlightOpacity
    {
        get => _highlightOpacity;
        set => this.RaiseAndSetIfChanged(ref _highlightOpacity, value);
    }

    public double HighlightAngle
    {
        get => _highlightAngle;
        set => this.RaiseAndSetIfChanged(ref _highlightAngle, value);
    }

    public double HighlightFalloff
    {
        get => _highlightFalloff;
        set => this.RaiseAndSetIfChanged(ref _highlightFalloff, value);
    }

    public bool ShadowEnabled
    {
        get => _shadowEnabled;
        set => this.RaiseAndSetIfChanged(ref _shadowEnabled, value);
    }

    public double ShadowRadius
    {
        get => _shadowRadius;
        set => this.RaiseAndSetIfChanged(ref _shadowRadius, value);
    }

    public double ShadowOffsetX
    {
        get => _shadowOffsetX;
        set
        {
            this.RaiseAndSetIfChanged(ref _shadowOffsetX, value);
            this.RaisePropertyChanged(nameof(ShadowOffset));
        }
    }

    public double ShadowOffsetY
    {
        get => _shadowOffsetY;
        set
        {
            this.RaiseAndSetIfChanged(ref _shadowOffsetY, value);
            this.RaisePropertyChanged(nameof(ShadowOffset));
        }
    }

    public Vector ShadowOffset => new Vector(ShadowOffsetX, ShadowOffsetY);

    public double ShadowOpacity
    {
        get => _shadowOpacity;
        set => this.RaiseAndSetIfChanged(ref _shadowOpacity, value);
    }

    public Color ShadowColor
    {
        get => _shadowColor;
        set => this.RaiseAndSetIfChanged(ref _shadowColor, value);
    }

    public bool InnerShadowEnabled
    {
        get => _innerShadowEnabled;
        set => this.RaiseAndSetIfChanged(ref _innerShadowEnabled, value);
    }

    public double InnerShadowRadius
    {
        get => _innerShadowRadius;
        set => this.RaiseAndSetIfChanged(ref _innerShadowRadius, value);
    }

    public double InnerShadowOffsetX
    {
        get => _innerShadowOffsetX;
        set
        {
            this.RaiseAndSetIfChanged(ref _innerShadowOffsetX, value);
            this.RaisePropertyChanged(nameof(InnerShadowOffset));
        }
    }

    public double InnerShadowOffsetY
    {
        get => _innerShadowOffsetY;
        set
        {
            this.RaiseAndSetIfChanged(ref _innerShadowOffsetY, value);
            this.RaisePropertyChanged(nameof(InnerShadowOffset));
        }
    }

    public Vector InnerShadowOffset => new Vector(InnerShadowOffsetX, InnerShadowOffsetY);

    public double InnerShadowOpacity
    {
        get => _innerShadowOpacity;
        set => this.RaiseAndSetIfChanged(ref _innerShadowOpacity, value);
    }

    public Color InnerShadowColor
    {
        get => _innerShadowColor;
        set => this.RaiseAndSetIfChanged(ref _innerShadowColor, value);
    }

    public bool IsInteractive
    {
        get => _isInteractive;
        set => this.RaiseAndSetIfChanged(ref _isInteractive, value);
    }

    public bool InteractiveHighlightEnabled
    {
        get => _interactiveHighlightEnabled;
        set => this.RaiseAndSetIfChanged(ref _interactiveHighlightEnabled, value);
    }

    public double InteractiveMaxScaleDip
    {
        get => _interactiveMaxScaleDip;
        set => this.RaiseAndSetIfChanged(ref _interactiveMaxScaleDip, value);
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
