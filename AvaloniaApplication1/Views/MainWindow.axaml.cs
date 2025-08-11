using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using LiquidGlassAvaloniaUI;
using AvaloniaApplication1.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AvaloniaApplication1.Views
{
    /// <summary>
    /// 液态玻璃测试窗口
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<LiquidGlassButton> _liquidGlassButtons = new();
        private List<LiquidGlassCard> _liquidGlassCards = new();
        private LiquidGlassButton? _demoButton;
        private LiquidGlassCard? _demoCard;
        private Slider? _displacementSlider;
        private Slider? _blurSlider;
        private Slider? _saturationSlider;
        private Slider? _aberrationSlider;
        private Slider? _cornerRadiusSlider;
        private Slider? _elasticitySlider;
        private Slider? _edgeMaskSlider;
        private CheckBox? _overLightCheckBox;
        private ComboBox? _modeComboBox;

        public MainWindow()
        {
            InitializeComponent();
            
            // 设置DataContext为ViewModel，这样绑定就能工作了
            DataContext = new MainWindowViewModel();
            
            this.Loaded += MainWindow_Loaded;
            
            // 重新启用调试器来检查问题
            ShaderDebugger.TestShaderLoading();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // 获取控制面板的控件引用
            _displacementSlider = this.FindControl<Slider>("DisplacementSlider");
            _blurSlider = this.FindControl<Slider>("BlurSlider");
            _saturationSlider = this.FindControl<Slider>("SaturationSlider");
            _aberrationSlider = this.FindControl<Slider>("AberrationSlider");
            _cornerRadiusSlider = this.FindControl<Slider>("CornerRadiusSlider");
            _elasticitySlider = this.FindControl<Slider>("ElasticitySlider");
            _edgeMaskSlider = this.FindControl<Slider>("EdgeMaskSlider");
            _overLightCheckBox = this.FindControl<CheckBox>("OverLightCheckBox");
            _modeComboBox = this.FindControl<ComboBox>("ModeComboBox");
            _demoButton = this.FindControl<LiquidGlassButton>("DemoButton");
            _demoCard = this.FindControl<LiquidGlassCard>("DemoCard");

            // 获取悬浮卡片引用并设置拖拽同步
            var floatingCard = this.FindControl<DraggableLiquidGlassCard>("FloatingCard");
            if (floatingCard != null && DataContext is MainWindowViewModel viewModel)
            {
                // 监听悬浮卡片的位置变化，同步到ViewModel
                floatingCard.PropertyChanged += (s, args) =>
                {
                    if (args.Property == DraggableLiquidGlassCard.XProperty)
                    {
                        viewModel.FloatingCardX = floatingCard.X;
                    }
                    else if (args.Property == DraggableLiquidGlassCard.YProperty)
                    {
                        viewModel.FloatingCardY = floatingCard.Y;
                    }
                };
            }

            // 查找所有液态玻璃控件
            FindLiquidGlassButtons(this);
            FindLiquidGlassCards(this);

            // 绑定事件处理器
            if (_displacementSlider != null)
                _displacementSlider.ValueChanged += OnParameterChanged;
            if (_blurSlider != null)
                _blurSlider.ValueChanged += OnParameterChanged;
            if (_saturationSlider != null)
                _saturationSlider.ValueChanged += OnParameterChanged;
            if (_aberrationSlider != null)
                _aberrationSlider.ValueChanged += OnParameterChanged;
            if (_cornerRadiusSlider != null)
                _cornerRadiusSlider.ValueChanged += OnParameterChanged;
            if (_elasticitySlider != null)
                _elasticitySlider.ValueChanged += OnParameterChanged;
            if (_edgeMaskSlider != null)
                _edgeMaskSlider.ValueChanged += OnParameterChanged;
            if (_overLightCheckBox != null)
                _overLightCheckBox.IsCheckedChanged += OnCheckBoxParameterChanged;
            if (_modeComboBox != null)
                _modeComboBox.SelectionChanged += OnModeChanged;
        }

        /// <summary>
        /// 递归查找所有 LiquidGlassButton 控件
        /// </summary>
        private void FindLiquidGlassButtons(Visual parent)
        {
            foreach (var child in parent.GetVisualChildren())
            {
                if (child is LiquidGlassButton liquidGlassButton)
                {
                    _liquidGlassButtons.Add(liquidGlassButton);
                }
                else if (child is Visual visualChild)
                {
                    FindLiquidGlassButtons(visualChild);
                }
            }
        }

        /// <summary>
        /// 递归查找所有 LiquidGlassCard 控件
        /// </summary>
        private void FindLiquidGlassCards(Visual parent)
        {
            foreach (var child in parent.GetVisualChildren())
            {
                if (child is LiquidGlassCard liquidGlassCard)
                {
                    _liquidGlassCards.Add(liquidGlassCard);
                }
                else if (child is Visual visualChild)
                {
                    FindLiquidGlassCards(visualChild);
                }
            }
        }

        /// <summary>
        /// 参数变化事件处理
        /// </summary>
        private void OnParameterChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateAllLiquidGlassControls();
        }

        /// <summary>
        /// 复选框参数变化事件处理
        /// </summary>
        private void OnCheckBoxParameterChanged(object? sender, RoutedEventArgs e)
        {
            UpdateAllLiquidGlassControls();
        }

        /// <summary>
        /// 模式变化事件处理
        /// </summary>
        private void OnModeChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateAllLiquidGlassControls();
        }

        /// <summary>
        /// 更新所有液态玻璃控件的参数
        /// </summary>
        private void UpdateAllLiquidGlassControls()
        {
            if (_displacementSlider == null || _blurSlider == null || 
                _saturationSlider == null || _aberrationSlider == null || 
                _cornerRadiusSlider == null || _elasticitySlider == null || 
                _overLightCheckBox == null || _modeComboBox == null) 
                return;

            // 获取当前模式
            var selectedMode = _modeComboBox.SelectedIndex switch
            {
                0 => LiquidGlassMode.Standard,
                1 => LiquidGlassMode.Polar,
                2 => LiquidGlassMode.Prominent,
                3 => LiquidGlassMode.Shader,
                _ => LiquidGlassMode.Standard
            };

            // 更新演示控件的参数
            if (_demoButton != null)
            {
                _demoButton.DisplacementScale = _displacementSlider.Value;
                _demoButton.BlurAmount = _blurSlider.Value;
                _demoButton.Saturation = _saturationSlider.Value;
                _demoButton.AberrationIntensity = _aberrationSlider.Value;
                _demoButton.CornerRadius = _cornerRadiusSlider.Value;
                _demoButton.Elasticity = _elasticitySlider.Value;
                _demoButton.OverLight = _overLightCheckBox.IsChecked ?? false;
                _demoButton.Mode = selectedMode;
            }

            if (_demoCard != null)
            {
                _demoCard.DisplacementScale = _displacementSlider.Value;
                _demoCard.BlurAmount = _blurSlider.Value;
                _demoCard.Saturation = _saturationSlider.Value;
                _demoCard.AberrationIntensity = _aberrationSlider.Value;
                _demoCard.CornerRadius = _cornerRadiusSlider.Value;
                _demoCard.OverLight = _overLightCheckBox.IsChecked ?? false;
                _demoCard.Mode = selectedMode;
            }

            // 可选：也更新其他控件（用户可能希望看到全局效果）
            foreach (var button in _liquidGlassButtons)
            {
                if (button != _demoButton) // 避免重复更新演示按钮
                {
                    button.DisplacementScale = _displacementSlider.Value;
                    button.BlurAmount = _blurSlider.Value;
                    button.Saturation = _saturationSlider.Value;
                    button.AberrationIntensity = _aberrationSlider.Value;
                    button.CornerRadius = _cornerRadiusSlider.Value;
                    button.Elasticity = _elasticitySlider.Value;
                    button.OverLight = _overLightCheckBox.IsChecked ?? false;
                    button.Mode = selectedMode;
                }
            }

            foreach (var card in _liquidGlassCards)
            {
                if (card != _demoCard) // 避免重复更新演示卡片
                {
                    card.DisplacementScale = _displacementSlider.Value;
                    card.BlurAmount = _blurSlider.Value;
                    card.Saturation = _saturationSlider.Value;
                    card.AberrationIntensity = _aberrationSlider.Value;
                    card.CornerRadius = _cornerRadiusSlider.Value;
                    card.OverLight = _overLightCheckBox.IsChecked ?? false;
                    card.Mode = selectedMode;
                }
            }
        }
    }
}
