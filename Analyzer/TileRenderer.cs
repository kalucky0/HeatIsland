using SkiaSharp;

namespace HeatIsland.Analyzer;

internal sealed class TileRenderer(Data data)
{
    public void DrawImage(string path)
    {
        using var surface = SKSurface.Create(new SKImageInfo(data.Width, data.Height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.Scale(1, -1, 0, data.Height / 2f);

        foreach (var point in data.RoutePoints)
        {
            var (x, y) = Utils.DecomposeId(point.Key);
            var count = point.Value;

            if (count < 4) continue;

            paint.Color = SKColor.FromHsl(10, 89, 66, 128);
            canvas.DrawPoint(x, y, paint);
        }

        foreach (var point in data.GreenPoints)
        {
            var (x, y) = Utils.DecomposeId(point.Key);
            var count = point.Value;

            paint.Color = SKColor.FromHsl(150, 50, count * 0.6f + 10f);
            canvas.DrawPoint(x, y, paint);
        }

        paint.Color = SKColors.Black;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2;
        canvas.DrawRect(0, 0, data.Width, data.Height, paint);

        using var image = surface.Snapshot();
        using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        imageData.SaveTo(stream);
    }
}
