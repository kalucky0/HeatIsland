using Microsoft.Extensions.Logging;
using HeatIsland.Analyzer;

namespace HeatIsland;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
            var data = DataLoader.Load(@"D:\Libraries\Downloads\78989_1475484_M-34-64-D-d-2-1-3-3.laz");
            DataProcessor.ProcessData(data);
            var renderer = new TileRenderer(data);
            renderer.DrawImage(@"D:\output.png");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
