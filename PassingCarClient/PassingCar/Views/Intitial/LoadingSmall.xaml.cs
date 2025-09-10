using System.Diagnostics;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PassingCar.Views
{
    public partial class LoadingSmall : ContentView
    {
        private readonly SKCanvasView canvas;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private float OvalStartAngle = 90; // Outer arc start angle
        private float SecondOvalStartAngle = 270; // Outer arc start angle
        private readonly float OvalSweepAngle = 80; // Outer arc sweep angle

        private float InnerOvalStartAngle = 90; // Inner arc start angle
        private float InnerSecondOvalStartAngle = 270; // Inner arc start angle
        private readonly float InnerOvalSweepAngle = 80; // Inner arc sweep angle

        /// <summary>
        /// Outer arc paint style: stroke, color, width, and antialias.
        /// </summary>
        private readonly SKPaint firstArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.FromHsl(0, 0, 100),
            StrokeWidth = 10,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        /// <summary>
        /// Inner arc paint style: stroke, color, width, and antialias.
        /// </summary>
        private readonly SKPaint secondArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.FromHsl(358, 90, 55),
            StrokeWidth = 8,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        public LoadingSmall()
        {
            InitializeComponent();
            canvas = new SKCanvasView();
            canvas.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvas;

            stopwatch.Start();
            // Set timer to refresh every 16 milliseconds (~60 FPS)
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerClik);
        }

        /// <summary>
        /// Timer click event to increment angles and invalidate the canvas.
        /// </summary>
        public bool OnTimerClik()
        {
            try
            {
                OvalStartAngle += 2;
                SecondOvalStartAngle += 2;
                InnerOvalStartAngle += 8;
                InnerSecondOvalStartAngle += 8;

                // Request canvas redraw
                canvas.InvalidateSurface();
                return true; // Continue the timer
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false; // Stop the timer on failure
            }
        }

        /// <summary>
        /// Method to handle SkiaSharp canvas drawing.
        /// </summary>
        public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            try
            {
                SKImageInfo info = args.Info;
                SKSurface surface = args.Surface;
                SKCanvas canvas = surface.Canvas;

                canvas.Clear();

                float left, right;
                float top, bottom;
                right = left = (info.Width - 150) / 2;  // Adjust for different device sizes
                top = bottom = (info.Height - 150) / 2; // Adjust for different device sizes

                // First Arc: Outer Arc
                SKRect outerRect = new SKRect(left, top, info.Width - right, info.Height - bottom);

                // Inner Arc
                SKRect innerRect = new SKRect(left + 50, top + 50, info.Width - right - 50, info.Height - bottom - 50);

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
                Console.WriteLine($"Error in OnCanvasViewPaintSurface: {ex.Message}");
            }
        }
    }
}
