using HeatIsland.Analyzer;
using System.Diagnostics;
using System.Text.Json;
using System.Web;

namespace HeatIsland;

public partial class MainPage : ContentPage
{
    private WebView webView;

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

    public void Ready(string path) => Task.Run(() => LoadData(path));

    private async Task LoadData(string path)
    {
        var data = DataLoader.Load(
            path,
            (info) => MainThread.BeginInvokeOnMainThread(async () =>
            {
                await webView.EvaluateJavaScriptAsync($"updateProgress({info.progress}, {info.total})");
            })
        );
        DataProcessor.ProcessData(data);

        var tempPath = Path.GetTempFileName();
        var renderer = new TileRenderer(data);
        renderer.DrawImage(tempPath);

        byte[] imageArray = await File.ReadAllBytesAsync(tempPath);
        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
        File.Delete(tempPath);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await webView.EvaluateJavaScriptAsync(
                $"addImage(" +
                $"'data:image/png;base64,{base64ImageRepresentation}'," +
                $"{data.MinExtent.Longitude}," +
                $"{data.MinExtent.Latitude}," +
                $"{data.MaxExtent.Longitude}," +
                $"{data.MaxExtent.Latitude})"
            );
        });
    }

    public void Print(string msg)
    {
        Debug.WriteLine(msg);
    }
}
