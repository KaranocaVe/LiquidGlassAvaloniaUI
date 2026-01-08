using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace LiquidGlassAvaloniaUI
{
    internal sealed class LiquidGlassInteractiveOverlay : Control
    {
        public override void Render(DrawingContext context)
        {
            if (LiquidGlassBackdropProvider.IsCapturing)
                return;

            if (TemplatedParent is not LiquidGlassInteractiveSurface surface)
                return;

            if (!surface.InteractiveHighlightEnabled)
                return;

            var progress = surface.GetInteractiveHighlightProgress();
            if (progress <= 0.001)
                return;

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            var parameters = surface.CreateDrawParameters();
            parameters.InteractiveProgress = progress;
            parameters.InteractivePosition = surface.GetInteractiveHighlightPosition();

            context.Custom(new LiquidGlassDrawOperation(bounds, parameters, snapshot: null, LiquidGlassDrawPass.InteractiveHighlight));
        }
    }

    internal sealed class LiquidGlassFrontOverlay : Control
    {
        public override void Render(DrawingContext context)
        {
            if (LiquidGlassBackdropProvider.IsCapturing)
                return;

            if (TemplatedParent is not LiquidGlassSurface surface)
                return;

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            var parameters = surface.CreateDrawParameters();

            if (surface.HighlightEnabled)
                context.Custom(new LiquidGlassDrawOperation(bounds, parameters, snapshot: null, LiquidGlassDrawPass.Highlight));

            if (surface.InnerShadowEnabled && parameters.InnerShadowOpacity > 0.001 && parameters.InnerShadowColor.A > 0)
                context.Custom(new LiquidGlassInnerShadowDrawOperation(bounds, parameters));
        }
    }
}

