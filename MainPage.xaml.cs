using System.Diagnostics;
using System.Text.Json;
using System.Web;

namespace HeatIsland;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        LoadWebPage();
    }

    private async Task LoadWebPage()
    {
        var contents = await LoadResource("index.html");

        WebView webView = new WebView
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

    public void Print(string msg)
    {
        Debug.WriteLine(msg);
    }
}
