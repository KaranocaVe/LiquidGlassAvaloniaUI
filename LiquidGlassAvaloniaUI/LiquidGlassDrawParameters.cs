using Avalonia;
using Avalonia.Media;

namespace LiquidGlassAvaloniaUI
{
    internal enum LiquidGlassDrawPass
    {
        Lens,
        InteractiveHighlight,
        Highlight,
    }

    internal struct LiquidGlassDrawParameters
    {
        public CornerRadius CornerRadius { get; set; }

        public double RefractionHeight { get; set; }
        public double RefractionAmount { get; set; }
        public bool DepthEffect { get; set; }
        public bool ChromaticAberration { get; set; }

        public double BlurRadius { get; set; }
        public double Vibrancy { get; set; }

        public Color TintColor { get; set; }
        public Color SurfaceColor { get; set; }

        public bool HighlightEnabled { get; set; }
        public double HighlightWidth { get; set; }
        public double HighlightBlurRadius { get; set; }
        public double HighlightOpacity { get; set; }
        public double HighlightAngleDegrees { get; set; }
        public double HighlightFalloff { get; set; }

        public double InteractiveProgress { get; set; }
        public Point InteractivePosition { get; set; }

        public bool ShadowEnabled { get; set; }
        public double ShadowRadius { get; set; }
        public Vector ShadowOffset { get; set; }
        public Color ShadowColor { get; set; }
        public double ShadowOpacity { get; set; }

        public bool InnerShadowEnabled { get; set; }
        public double InnerShadowRadius { get; set; }
        public Vector InnerShadowOffset { get; set; }
        public Color InnerShadowColor { get; set; }
        public double InnerShadowOpacity { get; set; }
    }
}
