# LiquidGlassAvaloniaUI

An AvaloniaUI “liquid glass” backdrop effect inspired by `AndroidLiquidGlass` (Kyant/backdrop): vibrancy → blur → rounded-rect lens refraction (optional dispersion) → edge highlight.

## 🎯 Overview

This project recreates the `AndroidLiquidGlass` backdrop pipeline using AvaloniaUI + SkiaSharp (`SKRuntimeEffect`) on Avalonia’s Skia renderer.

![Preview](/Screenshot.png)



https://github.com/user-attachments/assets/b65269fe-d695-425a-81f7-118e58583341



*The video above shows the visual effect with only the distortion applied. It looks quite impressive.The related code can be found in the branch*
## ⚠️ Current Status

This is an experimental implementation. Expect tuning work and performance tradeoffs (runtime shaders + backdrop snapshots).

## 🚀 Quick Start

1. Clone the repository
2. Open the solution in your IDE
3. Run the `LiquidGlassAvaloniaUI.Demo` project to see the effects in action

To validate rendering headlessly and optionally emit PNGs:

- `dotnet test LiquidGlassAvaloniaUI/LiquidGlassAvaloniaUI.Tests/LiquidGlassAvaloniaUI.Tests.csproj -c Release`
- (optional) `LIQUIDGLASS_TEST_OUTPUT_DIR=./artifacts` to write `with-glass.png` / `without-glass.png`

## 📖 Usage

Recommended (new API):

- `LiquidGlassSurface` - A `Decorator` that draws the Android-style pipeline behind its child and clips to `CornerRadius`.
- `LiquidGlassInteractiveSurface` - Adds AndroidLiquidGlass-style press/drag deformation + interactive highlight.

Legacy wrappers (kept for compatibility; parameter semantics do **not** match the new lens model 1:1):

- `LiquidGlassControl` - Basic liquid glass container
- `LiquidGlassButton` - Interactive button with liquid glass effects
- `LiquidGlassCard` - Card component with glass morphing
- `DraggableLiquidGlassCard` - Draggable card with dynamic effects

## 🙏 Credits

- Inspired by [liquid-glass-react](https://github.com/rdev/liquid-glass-react/tree/master)
- Shader approach inspired by `AndroidLiquidGlass` (Kyant/backdrop lens + highlight runtime shaders)
- Built with [AvaloniaUI](https://avaloniaui.net/)
- Powered by [SkiaSharp](https://github.com/mono/SkiaSharp)

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.
