using System.Diagnostics;
using System.Net;

namespace HeatIsland.Utils;

public class MapDownloader
{
    private string GenerateLinkToWebsite(Point a, Point b)
    {
        return
            $"https://mapy.geoportal.gov.pl/wss/service/PZGIK/DanePomiaroweNMT/WMS/SkorowidzeUkladEVRF2007?SERVICE=WMS&request=GetFeatureInfo&version=1.1.1&layers=SkorowidzeLIDAR2023&srs=EPSG%3A2180&bbox={a.Y},{a.X},{b.Y},{b.X}&width=2&height=2&format=image%2Fpng&x&y&INFO_FORMAT=text%2Fhtml&query_layers=SkorowidzeLIDAR2023";
    }

    private string GetDownloadLink(string response)
    {
        return response.Split("skor_dane_pom_NMT_wg_akt.push({url:\"")[1].Split("\",godlo")[0];
    }
    public async void GetMap(Point a, Point b)
    {
        try
        {
            HttpClient client = new HttpClient();
            string website = await client.GetStringAsync(GenerateLinkToWebsite(a, b));
            string downloadLink = GetDownloadLink(website);
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
            //var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName;
            if (projectDirectory == null) return;

            var folderPath = Path.Combine(projectDirectory, "Maps");
            var filePath = Path.Combine(folderPath, $"{a.Y}-{a.X}-{b.Y}-{b.X}");
            await using var response = await client.GetStreamAsync(downloadLink);
            // check if Maps folder exists and create it if not
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            //await using var fs = new MemoryStream();
            Console.WriteLine("Start");
            var progress = new Progress();
            var cancellationToken = new CancellationToken();
            await client.DownloadAsync(downloadLink, fs, progress, cancellationToken);
            Console.WriteLine("End");
            // await response.CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public class Progress
{
    public void Report(float progress)
    {
        Console.WriteLine(progress);
    }
}