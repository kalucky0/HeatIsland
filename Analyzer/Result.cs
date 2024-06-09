namespace HeatIsland.Analyzer;

internal struct Result
{
    public double Points { get; set; }
    public double VegetationCoverage { get; set; }
    public double BuildingsFootprint { get; set; }
    public double AverageBuildingHeight { get; set; }
    public double EnergyCost { get; set; }
}
