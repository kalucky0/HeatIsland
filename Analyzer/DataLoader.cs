using laszip.net;

namespace HeatIsland.Analyzer;

internal sealed class DataLoader
{
    public static Data Load(string path, Action<(int progress, int total)> info)
    {
        var lazReader = new laszip_dll();
        var compressed = false;
        lazReader.laszip_open_reader(path, ref compressed);
        var numberOfPoints = lazReader.header.number_of_point_records;

        double[] coordinates = new double[3];
        Dictionary<int, int> greenPoints = [];
        Dictionary<int, int> routePoints = [];
        Dictionary<int, double> buildingsPoints = [];
        List<double> groundHeights = [];

        for (int i = 0; i < numberOfPoints; i++)
        {
            lazReader.laszip_read_point();
            lazReader.laszip_get_coordinates(coordinates);
            byte classification = lazReader.point.classification;

            var x = (int)Math.Floor(coordinates[0] - lazReader.header.min_x);
            var y = (int)Math.Floor(coordinates[1] - lazReader.header.min_y);
            var id = Utils.CompoundId(x, y);

            if (classification == 0)
            {
                if (routePoints.ContainsKey(id))
                    routePoints[id]++;
                else
                    routePoints[id] = 1;
            }
            else if (classification == 2)
            {
                var groundHeight = coordinates[2];
                groundHeights.Add(groundHeight);
            }
            else if (classification == 6)
            {
                var buildingHeight = coordinates[2];
                buildingsPoints[id] = buildingHeight;
            }
            else if (classification > 2 && classification < 6)
            {
                if (greenPoints.ContainsKey(id))
                    greenPoints[id]++;
                else
                    greenPoints[id] = 1;
            }

            if (i % 10_000 == 0)
                info((i, (int)numberOfPoints));
        }

        info(((int)numberOfPoints, (int)numberOfPoints));

        var width = (int)Math.Ceiling(lazReader.header.max_x - lazReader.header.min_x);
        var height = (int)Math.Ceiling(lazReader.header.max_y - lazReader.header.min_y);

        var minExtent = Utils.ConvertEPSG2180ToWGS84(lazReader.header.min_x, lazReader.header.min_y);
        var maxExtent = Utils.ConvertEPSG2180ToWGS84(lazReader.header.max_x, lazReader.header.max_y);

        lazReader.laszip_clean();
        lazReader.laszip_close_reader();

        return new Data         {
            Width = width,
            Height = height,
            GreenPoints = greenPoints,
            RoutePoints = routePoints,
            BuildingsFootprint = buildingsPoints.Count,
            MinExtent = (minExtent[0], minExtent[1]),
            MaxExtent = (maxExtent[0], maxExtent[1]),
            AverageBuildingHeight = buildingsPoints.Count > 0 ? buildingsPoints.Values.Average() - groundHeights.Average() : 0,
        };
    }
}
