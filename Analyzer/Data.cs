namespace HeatIsland.Analyzer;

internal struct Data
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Dictionary<int, int> GreenPoints { get; set; }
    public Dictionary<int, int> RoutePoints { get; set; }
}
