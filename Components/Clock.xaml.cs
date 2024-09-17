using SkiaSharp;
using SkiaSharp.Views.Maui;
using System;
using System.Timers;
using Microsoft.Maui.Controls;

namespace MauiApp3
{
    public class ClockComponent : SKCanvasView
    {
        private readonly Timer _timer;

        public ClockComponent()
        {
            _timer = new Timer(1000); // Update every second
            _timer.Elapsed += (s, e) => OnCanvasInvalidate();
            _timer.Start();

            PaintSurface += OnPaintSurface;
        }

        private void OnCanvasInvalidate()
        {
            // Request to redraw the canvas
            InvalidateSurface();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            var width = e.Info.Width;
            var height = e.Info.Height;
            var centerX = width / 2;
            var centerY = height / 2;
            var radius = Math.Min(centerX, centerY) * 0.8f;

            var now = DateTime.Now;
            var hour = now.Hour % 12;
            var minute = now.Minute;
            var second = now.Second;

            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Black;
                paint.StrokeWidth = 4;
                paint.Style = SKPaintStyle.Stroke;

                // Draw clock circle
                canvas.DrawCircle(centerX, centerY, radius, paint);

                // Draw hour hand
                var hourAngle = (float)((hour + minute / 60.0) * 30 * Math.PI / 180);
                canvas.DrawLine(centerX, centerY, centerX + radius * 0.5f * (float)Math.Cos(hourAngle - Math.PI / 2), centerY + radius * 0.5f * (float)Math.Sin(hourAngle - Math.PI / 2), paint);

                // Draw minute hand
                var minuteAngle = (float)(minute * 6 * Math.PI / 180);
                canvas.DrawLine(centerX, centerY, centerX + radius * 0.7f * (float)Math.Cos(minuteAngle - Math.PI / 2), centerY + radius * 0.7f * (float)Math.Sin(minuteAngle - Math.PI / 2), paint);

                // Draw second hand
                paint.Color = SKColors.Red;
                var secondAngle = (float)(second * 6 * Math.PI / 180);
                canvas.DrawLine(centerX, centerY, centerX + radius * 0.9f * (float)Math.Cos(secondAngle - Math.PI / 2), centerY + radius * 0.9f * (float)Math.Sin(secondAngle - Math.PI / 2), paint);
            }
        }
    }
}