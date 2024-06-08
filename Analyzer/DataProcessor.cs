using System.Diagnostics;

namespace HeatIsland.Analyzer;

internal sealed class DataProcessor
{
    public static void ProcessData(Data data)
    {
        double percent = CalculatePercentage(data.Width, data.Height, data.GreenPoints);
        double average = CalculateAverage(data.Width, data.Height, data.GreenPoints);
        Debug.WriteLine($"Green points: {percent}% of the area, average: {average} points per m2");
    }

    private static double CalculatePercentage(int width, int height, Dictionary<int, int> points)
    {
        var count = width * height * 1.0;
        var total = points.Count * 1.0;
        return (total / count) * 100.0;
    }

    private static double CalculateAverage(int width, int height, Dictionary<int, int> points)
    {
        var count = width * height * 1.0;
        var total = 0;

        foreach (var point in points)
        {
            total += point.Value;
        }

        return total * 1.0 / count;
    }
}
