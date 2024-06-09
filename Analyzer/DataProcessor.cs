using System.Diagnostics;

namespace HeatIsland.Analyzer;

internal sealed class DataProcessor
{
    public static void ProcessData(Data data)
    {
        double percent = CalculatePercentage(data.Width, data.Height, data.GreenPoints);
        double average = CalculateAverage(data.Width, data.Height, data.GreenPoints);

        Debug.WriteLine($"Min Latitude: {data.MinExtent.Latitude}, Min Longitude: {data.MinExtent.Longitude}");
        Debug.WriteLine($"Max Latitude: {data.MaxExtent.Latitude}, Max Longitude: {data.MaxExtent.Longitude}");

        Debug.WriteLine($"Green points: {percent}% of the area, average: {average} points per m2");
        Debug.WriteLine($"Buildings footprint: {data.BuildingsFootprint} m2");
        Debug.WriteLine($"Average building height: {data.AverageBuildingHeight} m");
        var energy = CalculateEnergy(data.BuildingsFootprint * data.AverageBuildingHeight, 100800, 26, 0.3);
        var cost = CalculateEnergyCost(energy, 1.12);
        Debug.WriteLine($"Energy cost: {cost} PLN per hour");
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

    /// <summary>
    /// Calculate energy cost
    /// </summary>
    /// <param name="energy">Energy in Joules</param>
    /// <param name="costPerKWh">Cost per kWh</param>
    /// <returns></returns>
    private static double CalculateEnergyCost(double energy, double costPerKWh)
    {
        return energy / 3600000 * costPerKWh;
    }   

    /// <summary>
    /// Calculate energy in Joules
    /// </summary>
    /// <param name="airVolume">Air volume in m3</param>
    /// <param name="pressure">Air pressure in Pa</param>
    /// <param name="temperature">Air temperature in Celsius</param>
    /// <param name="temperatureDelta">Air temperature delta in Celsius</param>
    /// <returns>Energy required to change air temperature in Joules</returns>
    private static double CalculateEnergy(double airVolume, double pressure, double temperature, double temperatureDelta)
    {
        var density = CalculateAirDensity(temperature, pressure);
        var mass = density * airVolume;
        return mass * 1006 * temperatureDelta;
    }

    /// <summary>
    /// Calculate air density in kg/m3
    /// </summary>
    /// <param name="temperature">Air temperature in Celsius</param>
    /// <param name="pressure">Air pressure in Pa</param>
    /// <returns>Air density in kg/m3</returns>
    private static double CalculateAirDensity(double temperature, double pressure)
    {
        return pressure / (287.05 * (temperature + 273.15));
    }
}
