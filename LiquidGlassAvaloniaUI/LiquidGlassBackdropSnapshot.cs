using System;
using Avalonia;
using SkiaSharp;

namespace LiquidGlassAvaloniaUI
{
    internal sealed class LiquidGlassBackdropSnapshot : IDisposable
    {
        private int _leases;
        private int _disposeRequested;
        private int _disposed;

        public LiquidGlassBackdropSnapshot(SKImage image, PixelPoint originInPixels, PixelSize pixelSize, double scaling)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            OriginInPixels = originInPixels;
            PixelSize = pixelSize;
            Scaling = scaling;
        }

        public SKImage Image { get; }

        /// <summary>
        /// Snapshot origin in device pixels (TopLevel coordinate space).
        /// </summary>
        public PixelPoint OriginInPixels { get; }

        public PixelSize PixelSize { get; }

        public double Scaling { get; }

        public bool TryAddLease()
        {
            while (true)
            {
                if (System.Threading.Volatile.Read(ref _disposed) != 0)
                    return false;

                var current = System.Threading.Volatile.Read(ref _leases);
                if (System.Threading.Interlocked.CompareExchange(ref _leases, current + 1, current) == current)
                {
                    if (System.Threading.Volatile.Read(ref _disposed) != 0)
                    {
                        ReleaseLease();
                        return false;
                    }

                    return true;
                }
            }
        }

        public void ReleaseLease()
        {
            var remaining = System.Threading.Interlocked.Decrement(ref _leases);
            if (remaining < 0)
                return;

            if (remaining == 0 && System.Threading.Volatile.Read(ref _disposeRequested) != 0)
                Dispose();
        }

        public void RequestDispose()
        {
            System.Threading.Volatile.Write(ref _disposeRequested, 1);
            if (System.Threading.Volatile.Read(ref _leases) == 0)
                Dispose();
        }

        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            Image.Dispose();
        }
    }
}
