using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SkiaSharp;

namespace LiquidGlassAvaloniaUI
{
    internal static class LiquidGlassBackdropProvider
    {
        // Target capture rate while visuals are being rendered. (Avoids constant polling when idle.)
        private static readonly long s_minCaptureIntervalTicks = TimeSpan.FromMilliseconds(33).Ticks;

        private sealed class BackdropState
        {
            public bool CaptureQueued;
            public LiquidGlassBackdropSnapshot? Snapshot;
            public ulong SnapshotHash;
            public PixelSize SnapshotPixelSize;
            public PixelPoint SnapshotOriginInPixels;
            public double SnapshotScaling;
            public long LastCaptureTicksUtc;
            public bool HasLastClipRect;
            public Rect LastClipRect;
            public List<WeakReference<Control>> Subscribers { get; } = new();

            public RenderTargetBitmap? ScratchBitmap;

            public IRenderer? Renderer;
            public EventInfo? SceneInvalidatedEvent;
            public Delegate? SceneInvalidatedDelegate;
        }

        private static readonly ConditionalWeakTable<TopLevel, BackdropState> s_states = new();
        private static int s_captureDepth;
        private static PropertyInfo? s_dirtyRectProperty;

        public static bool IsCapturing
        {
            get => s_captureDepth > 0;
        }

        public static LiquidGlassBackdropSnapshot? TryGetSnapshot(Control control)
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(control);
            if (topLevel is null)
                return null;

            return s_states.TryGetValue(topLevel, out BackdropState? state)
                ? System.Threading.Volatile.Read(ref state.Snapshot)
                : null;
        }

        public static void EnsureSnapshot(Control control)
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(control);
            if (topLevel is null)
                return;

            BackdropState? state = s_states.GetOrCreateValue(topLevel);
            TrackSubscriber(state, control);
            CleanupSubscribers(state);
            EnsureRendererSubscription(topLevel, state);

            double scaling = topLevel.RenderScaling;

            bool shouldCapture =
                state.Snapshot is null
                || !state.SnapshotScaling.Equals(scaling);

            if (shouldCapture)
                QueueCapture(topLevel, state);
        }

        private static void CleanupSubscribers(BackdropState state)
        {
            for (int i = state.Subscribers.Count - 1; i >= 0; i--)
            {
                if (!state.Subscribers[i].TryGetTarget(out _))
                    state.Subscribers.RemoveAt(i);
            }
        }

        private static void QueueCapture(TopLevel topLevel, BackdropState state)
        {
            if (state.CaptureQueued)
                return;

            state.CaptureQueued = true;

            Dispatcher.UIThread.Post(() =>
            {
                state.CaptureQueued = false;
                Capture(topLevel, state);
            }, DispatcherPriority.Background);
        }

        private static void Capture(TopLevel topLevel, BackdropState state)
        {
            if (IsCapturing)
                return;

            CleanupSubscribers(state);
            if (state.Subscribers.Count == 0)
            {
                DetachRendererSubscription(state);
                System.Threading.Volatile.Read(ref state.Snapshot)?.RequestDispose();
                System.Threading.Volatile.Write(ref state.Snapshot, null);
                state.ScratchBitmap?.Dispose();
                state.ScratchBitmap = null;
                return;
            }

            double scaling = topLevel.RenderScaling;
            BackdropClip clip = CalculateBackdropClip(topLevel, state, scaling);
            if (clip.DipRect.Width <= 0 || clip.DipRect.Height <= 0)
                return;

            PixelSize pixelSize = clip.PixelRect.Size;
            state.LastClipRect = clip.DipRect;
            state.HasLastClipRect = true;

            s_captureDepth++;
            try
            {
                Vector dpi = new(96 * scaling, 96 * scaling);
                if (state.ScratchBitmap is null || state.ScratchBitmap.PixelSize != pixelSize || !state.SnapshotScaling.Equals(scaling))
                {
                    state.ScratchBitmap?.Dispose();
                    state.ScratchBitmap = new RenderTargetBitmap(pixelSize, dpi);
                }

                long nowTicks = DateTime.UtcNow.Ticks;
                HashSet<Visual> excludedRoots = GetExcludedRoots(topLevel, state);
                RenderVisualWithClip(state.ScratchBitmap, topLevel, clip.DipRect, excludedRoots);
                PixelPoint originInPixels = clip.PixelRect.Position;

                LiquidGlassBackdropSnapshot? currentSnapshot = System.Threading.Volatile.Read(ref state.Snapshot);
                bool isSameConfig =
                    currentSnapshot is not null
                    && state.SnapshotScaling.Equals(scaling)
                    && state.SnapshotPixelSize == pixelSize
                    && state.SnapshotOriginInPixels == originInPixels;

                (SKImage? snapshotImage, ulong hash) = CreateSkImageWithHash(state.ScratchBitmap, isSameConfig, state.SnapshotHash);

                if (isSameConfig && state.SnapshotHash == hash)
                {
                    state.LastCaptureTicksUtc = nowTicks;
                    return;
                }

                if (snapshotImage is null)
                    return;

                LiquidGlassBackdropSnapshot snapshot = new(snapshotImage, originInPixels, pixelSize, scaling);
                state.SnapshotHash = hash;
                state.SnapshotPixelSize = pixelSize;
                state.SnapshotOriginInPixels = originInPixels;
                state.SnapshotScaling = scaling;
                state.LastCaptureTicksUtc = nowTicks;

                // Publish the new snapshot before disposing the old one. Disposing first can leave a short
                // window where the render thread observes a disposed snapshot and falls back to a white fill.
                System.Threading.Volatile.Write(ref state.Snapshot, snapshot);
                currentSnapshot?.RequestDispose();
            }
            finally
            {
                s_captureDepth--;
            }

            InvalidateSubscribers(state);
        }

        private static void EnsureRendererSubscription(TopLevel topLevel, BackdropState state)
        {
            if (state.SceneInvalidatedDelegate is not null)
                return;

            if (topLevel is not IRenderRoot renderRoot)
                return;

            IRenderer? renderer = renderRoot.Renderer;
            if (renderer is null)
                return;

            EventInfo? evt = renderer.GetType().GetEvent("SceneInvalidated", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (evt is null)
                return;

            EventHandler<SceneInvalidatedEventArgs> handler = (_, args) =>
            {
                if (IsCapturing)
                    return;

                if (!topLevel.IsVisible)
                    return;

                bool hasDirtyRect = TryGetDirtyRect(args, out Rect dirtyRect);
                Dispatcher.UIThread.Post(() =>
                {
                    if (IsCapturing)
                        return;

                    if (!topLevel.IsVisible)
                        return;

                    if (!s_states.TryGetValue(topLevel, out BackdropState? current) || !ReferenceEquals(current, state))
                        return;

                    CleanupSubscribers(state);
                    if (state.Subscribers.Count == 0)
                    {
                        DetachRendererSubscription(state);
                        System.Threading.Volatile.Read(ref state.Snapshot)?.RequestDispose();
                        System.Threading.Volatile.Write(ref state.Snapshot, null);
                        state.ScratchBitmap?.Dispose();
                        state.ScratchBitmap = null;
                        return;
                    }

                    // If the required clip (inflated by blur/refraction) grows beyond the last capture,
                    // don't wait for the cadence timer — otherwise interactive slider drags can render
                    // with an undersized snapshot and appear to "shift" as the blur kernel clamps.
                    double scaling = topLevel.RenderScaling;
                    BackdropClip desiredClip = CalculateBackdropClip(topLevel, state, scaling);
                    bool needsClipGrowthCapture =
                        desiredClip.DipRect.Width > 0
                        && desiredClip.DipRect.Height > 0
                        && (!state.HasLastClipRect || !RectContains(state.LastClipRect, desiredClip.DipRect));

                    long nowTicks = DateTime.UtcNow.Ticks;
                    if (!needsClipGrowthCapture && nowTicks - state.LastCaptureTicksUtc < s_minCaptureIntervalTicks)
                        return;

                    if (!needsClipGrowthCapture && hasDirtyRect && state.HasLastClipRect && !dirtyRect.Intersects(state.LastClipRect))
                        return;

                    QueueCapture(topLevel, state);
                }, DispatcherPriority.Background);
            };

            state.Renderer = renderer;
            state.SceneInvalidatedEvent = evt;
            state.SceneInvalidatedDelegate = handler;
            evt.AddEventHandler(renderer, handler);
        }

        private static void DetachRendererSubscription(BackdropState state)
        {
            if (state.Renderer is null || state.SceneInvalidatedEvent is null || state.SceneInvalidatedDelegate is null)
            {
                state.Renderer = null;
                state.SceneInvalidatedEvent = null;
                state.SceneInvalidatedDelegate = null;
                return;
            }

            state.SceneInvalidatedEvent.RemoveEventHandler(state.Renderer, state.SceneInvalidatedDelegate);
            state.Renderer = null;
            state.SceneInvalidatedEvent = null;
            state.SceneInvalidatedDelegate = null;
        }

        private static bool RectContains(Rect outer, Rect inner)
        {
            return inner.X >= outer.X
                   && inner.Y >= outer.Y
                   && inner.Right <= outer.Right
                   && inner.Bottom <= outer.Bottom;
        }

        private static BackdropClip CalculateBackdropClip(TopLevel topLevel, BackdropState state, double scaling)
        {
            Rect? union = null;
            TopLevel root = topLevel;
            Rect clientRect = new(topLevel.ClientSize);

            for (int i = state.Subscribers.Count - 1; i >= 0; i--)
            {
                if (!state.Subscribers[i].TryGetTarget(out Control? control))
                {
                    state.Subscribers.RemoveAt(i);
                    continue;
                }

                if (!control.IsVisible || control.Bounds.Width <= 0 || control.Bounds.Height <= 0)
                    continue;

                if (!ReferenceEquals(control.GetVisualRoot(), root))
                    continue;

                TransformedBounds? transformed = control.GetTransformedBounds();
                if (transformed is null)
                    continue;

                Rect globalBounds = transformed.Value.Bounds.TransformToAABB(transformed.Value.Transform);

                // Inflate by an approximate sampling margin (blur + refraction). We scale the inflation
                // by the visual's render transform to keep it conservative for interactive transforms.
                double localInflate = GetBackdropInflate(control);
                double scaleX = Math.Sqrt(transformed.Value.Transform.M11 * transformed.Value.Transform.M11 + transformed.Value.Transform.M21 * transformed.Value.Transform.M21);
                double scaleY = Math.Sqrt(transformed.Value.Transform.M12 * transformed.Value.Transform.M12 + transformed.Value.Transform.M22 * transformed.Value.Transform.M22);
                double inflateX = localInflate * Math.Max(1.0, scaleX);
                double inflateY = localInflate * Math.Max(1.0, scaleY);

                globalBounds = globalBounds.Inflate(new Thickness(inflateX, inflateY));

                union = union is null ? globalBounds : union.Value.Union(globalBounds);
            }

            if (union is null)
                return default;

            Rect clip = union.Value.Intersect(clientRect);
            if (clip.Width <= 0 || clip.Height <= 0)
                return default;

            // Snap to pixel grid to keep shader sampling stable and avoid shimmering on fractional DPI.
            int pixelLeft = (int)Math.Floor(clip.X * scaling);
            int pixelTop = (int)Math.Floor(clip.Y * scaling);
            int pixelRight = (int)Math.Ceiling(clip.Right * scaling);
            int pixelBottom = (int)Math.Ceiling(clip.Bottom * scaling);
            PixelRect pixelRect = new(new PixelPoint(pixelLeft, pixelTop), new PixelPoint(pixelRight, pixelBottom));

            Rect dipRect = new(
                pixelRect.X / scaling,
                pixelRect.Y / scaling,
                pixelRect.Width / scaling,
                pixelRect.Height / scaling);

            return new BackdropClip(dipRect, pixelRect);
        }

        private readonly struct BackdropClip
        {
            public BackdropClip(Rect dipRect, PixelRect pixelRect)
            {
                DipRect = dipRect;
                PixelRect = pixelRect;
            }

            public Rect DipRect { get; }

            public PixelRect PixelRect { get; }
        }

        private static double GetBackdropInflate(Control control)
        {
            // Conservative but small default: enough for 2 DIP blur + 24 DIP refraction.
            const double minInflate = 32.0;

            switch (control)
            {
                case LiquidGlassSurface surface:
                    // Blur is applied as a Gaussian (sigma ~= BlurRadius), so sampling reaches ~3*sigma.
                    double zoomValue = surface.BackdropZoom;
                    if (zoomValue <= 0.0005 || double.IsNaN(zoomValue) || double.IsInfinity(zoomValue))
                        zoomValue = 1.0;

                    double zoom = Clamp(zoomValue, 0.1, 10.0);

                    // BackdropTransform can shift sampling outside the visible bounds (offset / zoom),
                    // and zoom < 1 expands the sampled region further beyond the control bounds.
                    Vector offset = surface.BackdropOffset;
                    double offsetMargin = Math.Max(Math.Abs(offset.X), Math.Abs(offset.Y)) / zoom;

                    double zoomOutMargin = 0.0;
                    if (zoom < 1.0)
                    {
                        double halfMaxSize = Math.Max(surface.Bounds.Width, surface.Bounds.Height) * 0.5;
                        zoomOutMargin = (1.0 / zoom - 1.0) * halfMaxSize;
                    }

                    // Chromatic aberration can sample up to ~2x refractionAmount in the worst case,
                    // so capture a wider border to avoid clamping artifacts.
                    double refractionMargin = Math.Abs(surface.RefractionAmount) * (surface.ChromaticAberration ? 2.0 : 1.0);

                    return Math.Max(
                        minInflate,
                        refractionMargin + surface.BlurRadius * 3.0 + 6.0 + offsetMargin + zoomOutMargin);
                default:
                    return minInflate;
            }
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static HashSet<Visual> GetExcludedRoots(TopLevel topLevel, BackdropState state)
        {
            HashSet<Visual> excluded = new();

            for (int i = state.Subscribers.Count - 1; i >= 0; i--)
            {
                if (!state.Subscribers[i].TryGetTarget(out Control? control))
                    continue;

                if (!control.IsVisible)
                    continue;

                if (!ReferenceEquals(control.GetVisualRoot(), topLevel))
                    continue;

                excluded.Add(control);
            }

            return excluded;
        }

        private static bool TryGetDirtyRect(object args, out Rect dirtyRect)
        {
            dirtyRect = default;
            if (args is null)
                return false;

            if (s_dirtyRectProperty is null || s_dirtyRectProperty.DeclaringType != args.GetType())
            {
                s_dirtyRectProperty = args.GetType().GetProperty(
                    "DirtyRect",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (s_dirtyRectProperty?.PropertyType != typeof(Rect))
                return false;

            if (s_dirtyRectProperty.GetValue(args) is not Rect value)
                return false;

            dirtyRect = value;
            return true;
        }

        private static void RenderVisualWithClip(RenderTargetBitmap target, Visual visual, Rect clipRect, ISet<Visual>? excludedRoots)
        {
            using DrawingContext ctx = target.CreateDrawingContext();
            LiquidGlassVisualRenderer.Render(ctx, visual, clipRect, excludedRoots);
        }

        private static void TrackSubscriber(BackdropState state, Control control)
        {
            for (int i = state.Subscribers.Count - 1; i >= 0; i--)
            {
                if (!state.Subscribers[i].TryGetTarget(out Control? existing))
                {
                    state.Subscribers.RemoveAt(i);
                    continue;
                }

                if (ReferenceEquals(existing, control))
                    return;
            }

            state.Subscribers.Add(new WeakReference<Control>(control));
        }

        private static void InvalidateSubscribers(BackdropState state)
        {
            for (int i = state.Subscribers.Count - 1; i >= 0; i--)
            {
                if (!state.Subscribers[i].TryGetTarget(out Control? control))
                {
                    state.Subscribers.RemoveAt(i);
                    continue;
                }

                control.InvalidateVisual();
            }
        }

        private static (SKImage? image, ulong hash) CreateSkImageWithHash(Bitmap bitmap, bool isSameConfig, ulong previousHash)
        {
            PixelFormat format = bitmap.Format ?? throw new NotSupportedException("Bitmap pixel format is not readable.");
            AlphaFormat alpha = bitmap.AlphaFormat ?? throw new NotSupportedException("Bitmap alpha format is not readable.");

            SKColorType colorType = format == PixelFormat.Bgra8888 ? SKColorType.Bgra8888 :
                format == PixelFormat.Rgba8888 ? SKColorType.Rgba8888 :
                throw new NotSupportedException($"Unsupported pixel format: {format}");

            SKAlphaType alphaType = alpha == AlphaFormat.Premul ? SKAlphaType.Premul :
                alpha == AlphaFormat.Unpremul ? SKAlphaType.Unpremul :
                SKAlphaType.Unpremul;

            SKImageInfo info = new(bitmap.PixelSize.Width, bitmap.PixelSize.Height, colorType, alphaType);
            int rowBytes = info.Width * info.BytesPerPixel;
            int length = rowBytes * info.Height;
            byte[]? bytes = ArrayPool<byte>.Shared.Rent(length);

            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    bitmap.CopyPixels(new PixelRect(bitmap.PixelSize), (IntPtr)ptr, length, rowBytes);
                }
            }

            try
            {
                ulong hash = ComputeBackdropHash(bytes, rowBytes, info.Width, info.Height);
                if (isSameConfig && hash == previousHash)
                    return (null, hash);

                return (SKImage.FromPixelCopy(info, bytes, rowBytes), hash);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        private static ulong ComputeBackdropHash(byte[] bytes, int rowBytes, int width, int height)
        {
            // Fast, stable “good enough” fingerprint (8x8 samples) to avoid redundant invalidations.
            const ulong fnvOffset = 14695981039346656037UL;
            const ulong fnvPrime = 1099511628211UL;

            ulong hash = fnvOffset;
            const int samplesX = 8;
            const int samplesY = 8;
            int maxX = Math.Max(1, width) - 1;
            int maxY = Math.Max(1, height) - 1;

            for (int sy = 0; sy < samplesY; sy++)
            {
                int y = samplesY == 1 ? 0 : (int)((long)sy * maxY / (samplesY - 1));
                int row = y * rowBytes;

                for (int sx = 0; sx < samplesX; sx++)
                {
                    int x = samplesX == 1 ? 0 : (int)((long)sx * maxX / (samplesX - 1));
                    int offset = row + x * 4;

                    for (int i = 0; i < 4; i++)
                    {
                        hash = (hash ^ bytes[offset + i]) * fnvPrime;
                    }
                }
            }

            hash = (hash ^ (ulong)width) * fnvPrime;
            hash = (hash ^ (ulong)height) * fnvPrime;
            return hash;
        }
    }
}
