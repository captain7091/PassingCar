using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

namespace PassingCar.Views
{
    public partial class LoadingArcs : ContentView
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private float OvalStartAngle = 90; // Outer arc start angle
        private float SecondOvalStartAngle = 270; // Second outer arc start angle
        private readonly float OvalSweepAngle = 80; // Outer arc sweep angle

        private float InnerOvalStartAngle = 90; // Inner arc start angle
        private float InnerSecondOvalStartAngle = 270; // Second inner arc start angle
        private readonly float InnerOvalSweepAngle = 80; // Inner arc sweep angle

        private readonly SKPaint firstArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.FromHsl(0, 0, 100),
            StrokeWidth = 10,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        private readonly SKPaint secondArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.FromHsl(358, 90, 55),
            StrokeWidth = 8,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        public LoadingArcs()
        {
            InitializeComponent();
            stopwatch.Start();
            // Optimize: Reduce animation frequency to save CPU (30 FPS instead of 60 FPS)
            Device.StartTimer(TimeSpan.FromMilliseconds(33), OnTimerClick);
        }

        private bool OnTimerClick()
        {
            try
            {
                // Optimize: Reduce animation steps for smoother performance
                OvalStartAngle += 3;
                SecondOvalStartAngle += 3;
                InnerOvalStartAngle += 12;
                InnerSecondOvalStartAngle += 12;
                canvas.InvalidateSurface();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Timer Error: {ex.Message}");
                return false;
            }
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            try
            {
                SKImageInfo info = args.Info;
                SKSurface surface = args.Surface;
                SKCanvas canvas = surface.Canvas;

                canvas.Clear();

                float left, right, top, bottom;
                right = left = (info.Width - 150) / 2;
                top = bottom = (info.Height - 150) / 2;

                // Define the rectangles for the arcs
                SKRect outerRect = new SKRect(left, top, info.Width - right, info.Height - bottom);
                SKRect innerRect = new SKRect(left + 50, top + 50, info.Width - right - 50, info.Height - bottom - 50);

                // Draw the first set of outer arcs
                using (SKPath path = new SKPath())
                {
                    path.AddArc(outerRect, OvalStartAngle, OvalSweepAngle);
                    canvas.DrawPath(path, firstArcPaint);
                }

                using (SKPath path = new SKPath())
                {
                    path.AddArc(outerRect, SecondOvalStartAngle, OvalSweepAngle);
                    canvas.DrawPath(path, firstArcPaint);
                }

                // Draw the inner arcs
                using (SKPath path = new SKPath())
                {
                    path.AddArc(innerRect, InnerOvalStartAngle, InnerOvalSweepAngle);
                    canvas.DrawPath(path, secondArcPaint);
                }

                using (SKPath path = new SKPath())
                {
                    path.AddArc(innerRect, InnerSecondOvalStartAngle, InnerOvalSweepAngle);
                    canvas.DrawPath(path, secondArcPaint);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Draw Error: {ex.Message}");
            }
        }
    }
}
