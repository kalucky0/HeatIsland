using HeatIsland.Analyzer;
using System.Diagnostics;
using System.Text.Json;
using System.Web;

namespace HeatIsland;

public partial class MainPage : ContentPage
{
    private WebView webView;
    private List<Data> tiles = [];

    public MainPage()
    {
        InitializeComponent();
        LoadWebPage();
    }

    private async Task LoadWebPage()
    {
        var contents = await LoadResource("index.html");
        webView = new WebView
        {
            Source = new HtmlWebViewSource
            {
                Html = contents
            }
        };

        webView.BackgroundColor = Color.FromRgb(0, 0, 0);
        webView.Navigating += OnWebViewNavigating;
        Content = webView;
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine($"Navigating to {e.Url}");
        if (e.Url.StartsWith("cs://"))
        {
            var uri = new Uri(e.Url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var method = uri.Host.Substring(0, 1).ToUpper() + uri.Host.Substring(1);
            var args = JsonSerializer.Deserialize<List<string>>(query.Get("args"));
            GetType().GetMethod(method)?.Invoke(this, args?.ToArray());
            e.Cancel = true;
        }
    }

    private async Task<string?> LoadResource(string resourceName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Getdata(string id, string pressure, string temperature, string temperatureDelta, string costPerKWh)
    {
        var result = DataProcessor.ProcessData(tiles[int.Parse(id)], double.Parse(pressure), double.Parse(temperature), double.Parse(temperatureDelta), double.Parse(costPerKWh));
        webView.EvaluateJavaScriptAsync(
            $"onNewData(" +
            $"{id}," +
            $"{result.Points}," +
            $"{result.VegetationCoverage}," +
            $"{result.BuildingsFootprint}," +
            $"{result.AverageBuildingHeight}," +
            $"{result.Energy}," +
            $"{result.EnergyCost}," +
            $"{result.EnergySaving}," +
            $"{result.EnergyCostSaving}" +
            $")"
        );
    }

    public void Ready(string path, string pressure, string temperature, string temperatureDelta, string costPerKWh)
    {
        Task.Run(() => LoadData(path, double.Parse(pressure), double.Parse(temperature), double.Parse(temperatureDelta), double.Parse(costPerKWh)));
    }

    private async Task LoadData(string path, double pressure, double temperature, double temperatureDelta, double costPerKWh)
    {
        var data = DataLoader.Load(
            path,
            (info) => MainThread.BeginInvokeOnMainThread(async () =>
            {
                await webView.EvaluateJavaScriptAsync($"updateProgress({info.progress}, {info.total})");
            })
        );
        var result = DataProcessor.ProcessData(data, pressure, temperature, temperatureDelta, costPerKWh);

        var tempPath = Path.GetTempFileName();
        var renderer = new TileRenderer(data);
        renderer.DrawImage(tempPath);

        byte[] imageArray = await File.ReadAllBytesAsync(tempPath);
        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
        File.Delete(tempPath);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            tiles.Add(data);
            await webView.EvaluateJavaScriptAsync(
                $"addImage(" +
                $"{tiles.Count - 1}," +
                $"'data:image/png;base64,{base64ImageRepresentation}'," +
                $"{data.MinExtent.Longitude}," +
                $"{data.MinExtent.Latitude}," +
                $"{data.MaxExtent.Longitude}," +
                $"{data.MaxExtent.Latitude}," +
                $"{result.Points}," +
                $"{result.VegetationCoverage}," +
                $"{result.BuildingsFootprint}," +
                $"{result.AverageBuildingHeight}," +
                $"{result.Energy}," +
                $"{result.EnergyCost}," +
                $"{result.EnergySaving}," +
                $"{result.EnergyCostSaving}" +
                $")"
            );
        });
    }

    public void Print(string msg)
    {
        Debug.WriteLine(msg);
    }
}
