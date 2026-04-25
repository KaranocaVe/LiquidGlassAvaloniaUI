using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaApplication1.Views
{
    public sealed class PerfChart : Control
    {
        private const int Capacity = 120;
        private readonly PerfSample[] _samples = new PerfSample[Capacity];
        private int _count;
        private int _next;

        private static readonly IBrush s_background = new SolidColorBrush(Color.FromArgb(36, 255, 255, 255));
        private static readonly IPen s_borderPen = new Pen(new SolidColorBrush(Color.FromArgb(42, 255, 255, 255)), 1);
        private static readonly IPen s_gridPen = new Pen(new SolidColorBrush(Color.FromArgb(24, 255, 255, 255)), 1);
        private static readonly IPen s_capturePen = new Pen(new SolidColorBrush(Color.FromRgb(0, 229, 255)), 1.6);
        private static readonly IPen s_skipPen = new Pen(new SolidColorBrush(Color.FromRgb(255, 193, 7)), 1.4);
        private static readonly IPen s_copyPen = new Pen(new SolidColorBrush(Color.FromRgb(255, 64, 129)), 1.4);
        private static readonly IPen s_filterPen = new Pen(new SolidColorBrush(Color.FromRgb(105, 240, 174)), 1.4);

        public void AddSample(double capturesPerSecond, double skipsPerSecond, double copyMegabytesPerSecond, double filterMissesPerSecond)
        {
            _samples[_next] = new PerfSample(
                Math.Max(0.0, capturesPerSecond),
                Math.Max(0.0, skipsPerSecond),
                Math.Max(0.0, copyMegabytesPerSecond),
                Math.Max(0.0, filterMissesPerSecond));

            _next = (_next + 1) % Capacity;
            if (_count < Capacity)
                _count++;

            InvalidateVisual();
        }

        public void Clear()
        {
            _count = 0;
            _next = 0;
            Array.Clear(_samples, 0, _samples.Length);
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            Rect bounds = new(Bounds.Size);
            if (bounds.Width <= 1 || bounds.Height <= 1)
                return;

            context.DrawRectangle(s_background, s_borderPen, bounds, 6, 6);

            double top = bounds.Top + 8;
            double bottom = bounds.Bottom - 8;
            double left = bounds.Left + 8;
            double right = bounds.Right - 8;
            if (right <= left || bottom <= top)
                return;

            for (int i = 1; i <= 3; i++)
            {
                double y = top + (bottom - top) * i / 4.0;
                context.DrawLine(s_gridPen, new Point(left, y), new Point(right, y));
            }

            if (_count < 2)
                return;

            double captureMax = GetMax(s => s.CapturesPerSecond);
            double skipMax = GetMax(s => s.SkipsPerSecond);
            double copyMax = GetMax(s => s.CopyMegabytesPerSecond);
            double filterMax = GetMax(s => s.FilterMissesPerSecond);

            DrawSeries(context, left, top, right, bottom, captureMax, s_capturePen, s => s.CapturesPerSecond);
            DrawSeries(context, left, top, right, bottom, skipMax, s_skipPen, s => s.SkipsPerSecond);
            DrawSeries(context, left, top, right, bottom, copyMax, s_copyPen, s => s.CopyMegabytesPerSecond);
            DrawSeries(context, left, top, right, bottom, filterMax, s_filterPen, s => s.FilterMissesPerSecond);
        }

        private double GetMax(Func<PerfSample, double> selector)
        {
            double max = 0.0001;
            for (int i = 0; i < _count; i++)
                max = Math.Max(max, selector(GetSample(i)));
            return max;
        }

        private void DrawSeries(
            DrawingContext context,
            double left,
            double top,
            double right,
            double bottom,
            double max,
            IPen pen,
            Func<PerfSample, double> selector)
        {
            Point? previous = null;
            double width = right - left;
            double height = bottom - top;

            for (int i = 0; i < _count; i++)
            {
                double x = _count == 1 ? right : left + width * i / (_count - 1);
                double normalized = Math.Clamp(selector(GetSample(i)) / max, 0.0, 1.0);
                double y = bottom - normalized * height;
                Point point = new(x, y);

                if (previous is Point prev)
                    context.DrawLine(pen, prev, point);

                previous = point;
            }
        }

        private PerfSample GetSample(int index)
        {
            int start = (_next - _count + Capacity) % Capacity;
            return _samples[(start + index) % Capacity];
        }

        private readonly struct PerfSample
        {
            public PerfSample(double capturesPerSecond, double skipsPerSecond, double copyMegabytesPerSecond, double filterMissesPerSecond)
            {
                CapturesPerSecond = capturesPerSecond;
                SkipsPerSecond = skipsPerSecond;
                CopyMegabytesPerSecond = copyMegabytesPerSecond;
                FilterMissesPerSecond = filterMissesPerSecond;
            }

            public double CapturesPerSecond { get; }
            public double SkipsPerSecond { get; }
            public double CopyMegabytesPerSecond { get; }
            public double FilterMissesPerSecond { get; }
        }
    }
}
