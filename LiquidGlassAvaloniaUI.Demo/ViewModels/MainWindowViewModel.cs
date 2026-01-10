using System;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        ApplyPresetShowcase();
    }

    private int _backgroundIndex = 0; // 0 = wallpaper, 1 = photo, 2 = distortion
    private bool _showColorGlows = true;
    private bool _showVignette = true;

    private double _backdropZoom = 1.0;
    private double _backdropOffsetX = 0.0;
    private double _backdropOffsetY = 0.0;

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

    private bool _progressiveBlurEnabled;
    private double _progressiveBlurStart = 0.5;
    private double _progressiveBlurEnd = 1.0;
    private double _progressiveTintIntensity = 0.8;
    private bool _enableProgressiveTint = true;
    private int _progressiveTintPresetIndex = 1;

    private bool _adaptiveLuminanceEnabled;
    private double _adaptiveLuminanceUpdateIntervalMs = 250.0;
    private double _adaptiveLuminanceSmoothing = 0.2;

    public int BackgroundIndex
    {
        get => _backgroundIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _backgroundIndex, value);
            this.RaisePropertyChanged(nameof(UseWallpaperBackground));
            this.RaisePropertyChanged(nameof(UsePhotoBackground));
            this.RaisePropertyChanged(nameof(UseDistortionBackground));
        }
    }

    public bool UseWallpaperBackground => BackgroundIndex == 0;
    public bool UsePhotoBackground => BackgroundIndex == 1;
    public bool UseDistortionBackground => BackgroundIndex == 2;

    public bool ShowColorGlows
    {
        get => _showColorGlows;
        set => this.RaiseAndSetIfChanged(ref _showColorGlows, value);
    }

    public bool ShowVignette
    {
        get => _showVignette;
        set => this.RaiseAndSetIfChanged(ref _showVignette, value);
    }

    public double BackdropZoom
    {
        get => _backdropZoom;
        set => this.RaiseAndSetIfChanged(ref _backdropZoom, value);
    }

    public double BackdropOffsetX
    {
        get => _backdropOffsetX;
        set
        {
            this.RaiseAndSetIfChanged(ref _backdropOffsetX, value);
            this.RaisePropertyChanged(nameof(BackdropOffset));
        }
    }

    public double BackdropOffsetY
    {
        get => _backdropOffsetY;
        set
        {
            this.RaiseAndSetIfChanged(ref _backdropOffsetY, value);
            this.RaisePropertyChanged(nameof(BackdropOffset));
        }
    }

    public Vector BackdropOffset => new Vector(BackdropOffsetX, BackdropOffsetY);


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

    public bool ProgressiveBlurEnabled
    {
        get => _progressiveBlurEnabled;
        set => this.RaiseAndSetIfChanged(ref _progressiveBlurEnabled, value);
    }

    public double ProgressiveBlurStart
    {
        get => _progressiveBlurStart;
        set => this.RaiseAndSetIfChanged(ref _progressiveBlurStart, value);
    }

    public double ProgressiveBlurEnd
    {
        get => _progressiveBlurEnd;
        set => this.RaiseAndSetIfChanged(ref _progressiveBlurEnd, value);
    }

    public double ProgressiveTintIntensity
    {
        get => _progressiveTintIntensity;
        set => this.RaiseAndSetIfChanged(ref _progressiveTintIntensity, value);
    }

    public bool EnableProgressiveTint
    {
        get => _enableProgressiveTint;
        set
        {
            this.RaiseAndSetIfChanged(ref _enableProgressiveTint, value);
            this.RaisePropertyChanged(nameof(ProgressiveTintColor));
        }
    }

    public int ProgressiveTintPresetIndex
    {
        get => _progressiveTintPresetIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _progressiveTintPresetIndex, value);
            this.RaisePropertyChanged(nameof(ProgressiveTintColor));
        }
    }

    public Color ProgressiveTintColor => EnableProgressiveTint
        ? ProgressiveTintPresets[Math.Clamp(ProgressiveTintPresetIndex, 0, ProgressiveTintPresets.Length - 1)]
        : Colors.Transparent;

    public bool AdaptiveLuminanceEnabled
    {
        get => _adaptiveLuminanceEnabled;
        set => this.RaiseAndSetIfChanged(ref _adaptiveLuminanceEnabled, value);
    }

    public double AdaptiveLuminanceUpdateIntervalMs
    {
        get => _adaptiveLuminanceUpdateIntervalMs;
        set => this.RaiseAndSetIfChanged(ref _adaptiveLuminanceUpdateIntervalMs, value);
    }

    public double AdaptiveLuminanceSmoothing
    {
        get => _adaptiveLuminanceSmoothing;
        set => this.RaiseAndSetIfChanged(ref _adaptiveLuminanceSmoothing, value);
    }

    private static readonly Color[] TintPresets =
    {
        Color.FromRgb(0, 136, 255),   // blue
        Color.FromRgb(255, 0, 122),   // pink
        Color.FromRgb(0, 200, 160),   // teal
        Color.FromRgb(255, 141, 40),  // amber
    };

    private static readonly Color[] SurfacePresets =
    {
        Color.FromArgb(77, 255, 255, 255), // ~0.3
        Color.FromArgb(77, 0, 0, 0),
    };

    private static readonly Color[] ProgressiveTintPresets =
    {
        Colors.White,
        Color.FromRgb(128, 128, 128),
    };

    public void ApplyPresetDefaults()
    {
        EnableTint = false;
        TintPresetIndex = 0;
        EnableSurface = false;
        SurfacePresetIndex = 0;

        BackdropZoom = 1.0;
        BackdropOffsetX = 0.0;
        BackdropOffsetY = 0.0;

        RefractionHeight = 12.0;
        RefractionAmount = 24.0;
        DepthEffect = false;
        ChromaticAberration = false;

        BlurRadius = 2.0;
        Vibrancy = 1.5;
        Brightness = 0.0;
        Contrast = 1.0;
        ExposureEv = 0.0;
        GammaPower = 1.0;
        BackdropOpacity = 1.0;

        ProgressiveBlurEnabled = false;
        ProgressiveBlurStart = 0.5;
        ProgressiveBlurEnd = 1.0;
        ProgressiveTintIntensity = 0.8;
        EnableProgressiveTint = true;
        ProgressiveTintPresetIndex = 1;

        AdaptiveLuminanceEnabled = false;
        AdaptiveLuminanceUpdateIntervalMs = 250.0;
        AdaptiveLuminanceSmoothing = 0.2;

        HighlightEnabled = true;
        HighlightWidth = 0.5;
        HighlightBlurRadius = 0.25;
        HighlightOpacity = 0.5;
        HighlightAngle = 45.0;
        HighlightFalloff = 1.0;

        ShadowEnabled = true;
        ShadowRadius = 24.0;
        ShadowOffsetX = 0.0;
        ShadowOffsetY = 4.0;
        ShadowOpacity = 1.0;
        ShadowColor = Color.FromArgb(26, 0, 0, 0);

        InnerShadowEnabled = false;
        InnerShadowRadius = 24.0;
        InnerShadowOffsetX = 0.0;
        InnerShadowOffsetY = 24.0;
        InnerShadowOpacity = 1.0;
        InnerShadowColor = Color.FromArgb(38, 0, 0, 0);
    }

    public void ApplyPresetShowcase()
    {
        BackgroundIndex = 0;
        ShowColorGlows = true;
        ShowVignette = true;

        EnableTint = true;
        TintPresetIndex = 0;
        EnableSurface = true;
        SurfacePresetIndex = 0;

        BackdropZoom = 1.0;
        BackdropOffsetX = 0.0;
        BackdropOffsetY = 0.0;

        RefractionHeight = 12.0;
        RefractionAmount = 28.0;
        DepthEffect = true;
        ChromaticAberration = true;

        BlurRadius = 8.0;
        Vibrancy = 1.5;
        Brightness = 0.08;
        Contrast = 1.0;
        ExposureEv = 0.0;
        GammaPower = 1.0;
        BackdropOpacity = 1.0;

        ProgressiveBlurEnabled = false;
        AdaptiveLuminanceEnabled = false;

        HighlightEnabled = true;
        HighlightWidth = 0.7;
        HighlightBlurRadius = 0.45;
        HighlightOpacity = 0.6;
        HighlightAngle = 55.0;
        HighlightFalloff = 1.0;

        ShadowEnabled = true;
        ShadowRadius = 28.0;
        ShadowOffsetX = 0.0;
        ShadowOffsetY = 10.0;
        ShadowOpacity = 1.0;
        ShadowColor = Color.FromArgb(70, 0, 0, 0);

        InnerShadowEnabled = false;
    }

    public void ApplyPresetMagnifier()
    {
        EnableTint = false;
        TintPresetIndex = 0;
        EnableSurface = false;
        SurfacePresetIndex = 0;

        BackdropZoom = 1.5;
        BackdropOffsetX = 0.0;
        BackdropOffsetY = -80.0;

        RefractionHeight = 8.0;
        RefractionAmount = 24.0;
        DepthEffect = true;
        ChromaticAberration = true;

        BlurRadius = 0.0;
        Vibrancy = 1.0;
        Brightness = 0.0;
        Contrast = 1.0;
        ExposureEv = 0.0;
        GammaPower = 1.0;
        BackdropOpacity = 1.0;

        ProgressiveBlurEnabled = false;
        AdaptiveLuminanceEnabled = false;

        HighlightEnabled = true;
        HighlightOpacity = 0.4;
        HighlightWidth = 0.5;
        HighlightBlurRadius = 0.35;
        HighlightAngle = 55.0;
        HighlightFalloff = 1.2;

        ShadowEnabled = true;
        ShadowRadius = 26.0;
        ShadowOffsetX = 0.0;
        ShadowOffsetY = 12.0;
        ShadowOpacity = 1.0;
        ShadowColor = Color.FromArgb(90, 0, 0, 0);

        InnerShadowEnabled = true;
        InnerShadowRadius = 16.0;
        InnerShadowOffsetX = 0.0;
        InnerShadowOffsetY = 10.0;
        InnerShadowOpacity = 1.0;
        InnerShadowColor = Color.FromArgb(110, 0, 0, 0);
    }

    public void ApplyPresetProgressiveBlur()
    {
        EnableTint = false;
        TintPresetIndex = 0;
        EnableSurface = false;
        SurfacePresetIndex = 0;

        BackdropZoom = 1.0;
        BackdropOffsetX = 0.0;
        BackdropOffsetY = 0.0;

        RefractionHeight = 0.0;
        RefractionAmount = 0.0;
        DepthEffect = false;
        ChromaticAberration = false;

        BlurRadius = 4.0;
        Vibrancy = 1.0;
        Brightness = 0.0;
        Contrast = 1.0;
        ExposureEv = 0.0;
        GammaPower = 1.0;
        BackdropOpacity = 1.0;

        ProgressiveBlurEnabled = true;
        ProgressiveBlurStart = 0.5;
        ProgressiveBlurEnd = 1.0;
        EnableProgressiveTint = true;
        ProgressiveTintPresetIndex = 1;
        ProgressiveTintIntensity = 0.8;

        AdaptiveLuminanceEnabled = false;

        HighlightEnabled = false;
        ShadowEnabled = false;
        InnerShadowEnabled = false;
    }

    public void ApplyPresetDistortion()
    {
        BackgroundIndex = 2;
        ShowColorGlows = false;
        ShowVignette = false;

        EnableTint = false;
        TintPresetIndex = 0;
        EnableSurface = false;
        SurfacePresetIndex = 0;

        BackdropZoom = 1.0;
        BackdropOffsetX = 0.0;
        BackdropOffsetY = 0.0;

        RefractionHeight = 18.0;
        RefractionAmount = 46.0;
        DepthEffect = true;
        ChromaticAberration = true;

        BlurRadius = 0.0;
        Vibrancy = 1.0;
        Brightness = 0.0;
        Contrast = 1.0;
        ExposureEv = 0.0;
        GammaPower = 1.0;
        BackdropOpacity = 1.0;

        ProgressiveBlurEnabled = false;
        AdaptiveLuminanceEnabled = false;

        HighlightEnabled = false;
        ShadowEnabled = false;
        InnerShadowEnabled = false;
    }

    public void ApplyPresetAdaptiveLuminance()
    {
        EnableTint = false;
        TintPresetIndex = 0;
        EnableSurface = false;
        SurfacePresetIndex = 0;

        BackdropZoom = 1.0;
        BackdropOffsetX = 0.0;
        BackdropOffsetY = 0.0;

        // Lens stays on; brightness/contrast/blur are overridden.
        RefractionHeight = 24.0;
        RefractionAmount = 28.0;
        DepthEffect = true;
        ChromaticAberration = false;

        BlurRadius = 8.0;
        Vibrancy = 1.5;
        Brightness = 0.0;
        Contrast = 1.0;
        ExposureEv = 0.0;
        GammaPower = 1.0;
        BackdropOpacity = 1.0;

        ProgressiveBlurEnabled = false;

        AdaptiveLuminanceEnabled = true;
        AdaptiveLuminanceUpdateIntervalMs = 250.0;
        AdaptiveLuminanceSmoothing = 0.25;

        HighlightEnabled = true;
        HighlightOpacity = 0.5;

        ShadowEnabled = true;
        ShadowRadius = 24.0;
        ShadowOffsetX = 0.0;
        ShadowOffsetY = 8.0;
        ShadowOpacity = 1.0;
        ShadowColor = Color.FromArgb(70, 0, 0, 0);

        InnerShadowEnabled = false;
    }
}
