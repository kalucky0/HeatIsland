using laszip.net;

namespace HeatIsland.Analyzer;

internal sealed class DataLoader
{
    public static Data Load(string path)
    {
        var lazReader = new laszip_dll();
        var compressed = false;
        lazReader.laszip_open_reader(path, ref compressed);
        var numberOfPoints = lazReader.header.number_of_point_records;

        double[] coordinates = new double[3];
        Dictionary<int, int> greenPoints = [];
        Dictionary<int, int> routePoints = [];

        for (int i = 0; i < numberOfPoints; i++)
        {
            lazReader.laszip_read_point();
            lazReader.laszip_get_coordinates(coordinates);
            byte classification = lazReader.point.classification;

            if (classification == 0)
            {
                var x = (int)Math.Floor(coordinates[0] - lazReader.header.min_x);
                var y = (int)Math.Floor(coordinates[1] - lazReader.header.min_y);
                var id = Utils.CompoundId(x, y);

                if (routePoints.ContainsKey(id))
                    routePoints[id]++;
                else
                    routePoints[id] = 1;
            }
            else if (classification > 2 && classification < 6)
            {
                var x = (int)Math.Floor(coordinates[0] - lazReader.header.min_x);
                var y = (int)Math.Floor(coordinates[1] - lazReader.header.min_y);
                var id = Utils.CompoundId(x, y);

                if (greenPoints.ContainsKey(id))
                    greenPoints[id]++;
                else
                    greenPoints[id] = 1;
            }
        }

        var width = (int)Math.Ceiling(lazReader.header.max_x - lazReader.header.min_x);
        var height = (int)Math.Ceiling(lazReader.header.max_y - lazReader.header.min_y);

        lazReader.laszip_clean();
        lazReader.laszip_close_reader();

        return new Data         {
            Width = width,
            Height = height,
            GreenPoints = greenPoints,
            RoutePoints = routePoints
        };
    }
}
