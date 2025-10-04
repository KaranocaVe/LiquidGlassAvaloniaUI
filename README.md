# LiquidGlassAvaloniaUI

An AvaloniaUI implementation of Apple's liquid glass morphing effect, inspired by [liquid-glass-react](https://github.com/rdev/liquid-glass-react/tree/master).

**I will update this project once I see a liquidglass shader that satisfies me.**

## 🎯 Overview

This project recreates the beautiful liquid glass visual effect from Apple's design language using AvaloniaUI and SkiaSharp. The effect creates smooth, morphing glass-like distortions that respond to user interactions.

![Preview](/Screenshot.png)



https://github.com/user-attachments/assets/b65269fe-d695-425a-81f7-118e58583341



*The video above shows the visual effect with only the distortion applied. It looks quite impressive.The related code can be found in the branch*
## ⚠️ Current Status

This is an experimental implementation with several known issues:

- **Global dispersion effects** - Color dispersion applies globally rather than being localized
- **Parameter initialization problems** - Some effect parameters don't initialize correctly
- **Performance optimization needed** - Effects may impact performance on some devices

## 🚀 Quick Start

1. Clone the repository
2. Open the solution in your IDE
3. Run the `LiquidGlassAvaloniaUI.Demo` project to see the effects in action

## 📖 Usage

See the demo application for examples of how to use the liquid glass controls:

- `LiquidGlassControl` - Basic liquid glass container
- `LiquidGlassButton` - Interactive button with liquid glass effects
- `LiquidGlassCard` - Card component with glass morphing
- `DraggableLiquidGlassCard` - Draggable card with dynamic effects

## 🙏 Credits

- Inspired by [liquid-glass-react](https://github.com/rdev/liquid-glass-react/tree/master)
- Built with [AvaloniaUI](https://avaloniaui.net/)
- Powered by [SkiaSharp](https://github.com/mono/SkiaSharp)

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.
