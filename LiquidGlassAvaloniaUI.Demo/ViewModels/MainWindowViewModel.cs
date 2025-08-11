using LiquidGlassAvaloniaUI;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private double _displacementScale = 20.0;
    private double _blurAmount = 0.0625;
    private double _saturation = 140.0;
    private double _aberrationIntensity = 2.0;
    private double _elasticity = 0.4;
    private double _cornerRadius = 25.0;
    private LiquidGlassMode _mode = LiquidGlassMode.Standard;
    private bool _overLight = false;

    public string Greeting { get; } = "Welcome to Avalonia!";

    /// <summary>
    /// 位移缩放强度 (0-200)
    /// </summary>
    public double DisplacementScale
    {
        get => _displacementScale;
        set => this.RaiseAndSetIfChanged(ref _displacementScale, value);
    }

    /// <summary>
    /// 模糊量 (0-1)
    /// </summary>
    public double BlurAmount
    {
        get => _blurAmount;
        set => this.RaiseAndSetIfChanged(ref _blurAmount, value);
    }

    /// <summary>
    /// 饱和度 (0-300)
    /// </summary>
    public double Saturation
    {
        get => _saturation;
        set => this.RaiseAndSetIfChanged(ref _saturation, value);
    }

    /// <summary>
    /// 色差强度 (0-10)
    /// </summary>
    public double AberrationIntensity
    {
        get => _aberrationIntensity;
        set => this.RaiseAndSetIfChanged(ref _aberrationIntensity, value);
    }

    /// <summary>
    /// 弹性强度 (0-1) - 仅用于按钮
    /// </summary>
    public double Elasticity
    {
        get => _elasticity;
        set => this.RaiseAndSetIfChanged(ref _elasticity, value);
    }

    /// <summary>
    /// 圆角半径 (0-50)
    /// </summary>
    public double CornerRadius
    {
        get => _cornerRadius;
        set => this.RaiseAndSetIfChanged(ref _cornerRadius, value);
    }

    /// <summary>
    /// 液态玻璃模式
    /// </summary>
    public LiquidGlassMode Mode
    {
        get => _mode;
        set 
        {
            var oldValue = _mode;
            this.RaiseAndSetIfChanged(ref _mode, value);
            if (oldValue != _mode)
            {
                // 当Mode改变时，也要通知ModeIndex改变
                this.RaisePropertyChanged(nameof(ModeIndex));
            }
        }
    }

    /// <summary>
    /// 模式索引 - 用于ComboBox绑定
    /// </summary>
    public int ModeIndex
    {
        get => (int)_mode;
        set 
        { 
            var newMode = (LiquidGlassMode)value;
            if (_mode != newMode)
            {
                _mode = newMode;
                this.RaisePropertyChanged(nameof(Mode));
                this.RaisePropertyChanged(nameof(ModeIndex));
            }
        }
    }

    /// <summary>
    /// 是否在亮色背景上
    /// </summary>
    public bool OverLight
    {
        get => _overLight;
        set => this.RaiseAndSetIfChanged(ref _overLight, value);
    }

    #region 悬浮拖拽卡片属性

    private double _floatingCardX = 50.0;
    private double _floatingCardY = 50.0;
    private bool _showFloatingCard = true;

    /// <summary>
    /// 悬浮卡片X位置
    /// </summary>
    public double FloatingCardX
    {
        get => _floatingCardX;
        set => this.RaiseAndSetIfChanged(ref _floatingCardX, value);
    }

    /// <summary>
    /// 悬浮卡片Y位置
    /// </summary>
    public double FloatingCardY
    {
        get => _floatingCardY;
        set => this.RaiseAndSetIfChanged(ref _floatingCardY, value);
    }

    /// <summary>
    /// 是否显示悬浮卡片
    /// </summary>
    public bool ShowFloatingCard
    {
        get => _showFloatingCard;
        set => this.RaiseAndSetIfChanged(ref _showFloatingCard, value);
    }

    #endregion
}
