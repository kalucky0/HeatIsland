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

        foreach (var point in data.GreenPoints)
        {
            var (x, y) = Utils.DecomposeId(point.Key);
            var count = point.Value;

            paint.Color = SKColor.FromHsl(0, 0, count * 1.3f);
            canvas.DrawPoint(x, y, paint);
        }

        foreach (var point in data.RoutePoints)
        {
            var (x, y) = Utils.DecomposeId(point.Key);
            var count = point.Value;

            if (count < 4) continue;

            paint.Color = SKColor.FromHsl(120, 255, count);
            canvas.DrawCircle(x, y, 2, paint);
        }

        using var image = surface.Snapshot();
        using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        imageData.SaveTo(stream);
    }
}
