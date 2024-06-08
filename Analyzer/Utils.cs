using DotSpatial.Projections;

namespace HeatIsland.Analyzer;

internal sealed class Utils
{
    public static int CompoundId(int x, int y)
    {
        return x << 16 | y;
    }

    public static (int x, int y) DecomposeId(int id)
    {
        return (id >> 16, id & 0xFFFF);
    }

    public static double[] ConvertEPSG2180ToWGS84(double x, double y)
    {
        ProjectionInfo sourceProj = ProjectionInfo.FromEpsgCode(2180);
        ProjectionInfo destProj = ProjectionInfo.FromEpsgCode(4326);

        double[] xy = [x, y];
        double[] z = [0];
        Reproject.ReprojectPoints(xy, z, sourceProj, destProj, 0, 1);

        return [xy[0], xy[1]];
    }
}
