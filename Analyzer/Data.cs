namespace HeatIsland.Analyzer;

internal struct Data
{
    public int Width { get; set; }
    public int Height { get; set; }
    public (double Latitude, double Longitude) MinExtent { get; set; }
    public (double Latitude, double Longitude) MaxExtent { get; set; }
    public Dictionary<int, int> GreenPoints { get; set; }
    public Dictionary<int, int> RoutePoints { get; set; }
    public double BuildingsFootprint { get; set; }
    public double AverageBuildingHeight { get; set; }
}
