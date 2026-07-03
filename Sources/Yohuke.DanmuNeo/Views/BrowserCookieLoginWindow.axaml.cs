using Avalonia.Controls;
using Avalonia.Platform;
using Yohuke.DanmuNeo.Models.BrowserLogin;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Views;

/// <summary>
/// 内嵌浏览器登录窗口。
/// </summary>
public partial class BrowserCookieLoginWindow : Window
{
    private readonly BrowserLoginPlatform platform;
    private readonly BrowserCookieLoginService loginService = new();
    private NativeWebView? webView;

    /// <summary>
    /// 初始化内嵌浏览器登录窗口。
    /// </summary>
    public BrowserCookieLoginWindow()
        : this(BrowserLoginPlatform.Bilibili)
    {
    }

    /// <summary>
    /// 初始化内嵌浏览器登录窗口。
    /// </summary>
    public BrowserCookieLoginWindow(BrowserLoginPlatform platform)
    {
        this.platform = platform;
        InitializeComponent();
        Opened += BrowserCookieLoginWindow_OnOpened;
        ConfigureShell();
    }

    private void ConfigureShell()
    {
        var platformName = loginService.GetPlatformName(platform);

        Title = $"{platformName} 浏览器登录";
        HeaderText.Text = $"{platformName} 浏览器登录";
        StatusText.Text = "登录完成后点击“完成并抓取 Cookie”。Cookie 只会保存当前 API 需要的字段。";
    }

    private async void BrowserCookieLoginWindow_OnOpened(object? sender, EventArgs e)
    {
        await Task.Yield();
        CreateBrowser();
    }

    private void CreateBrowser()
    {
        try
        {
            var loginUri = loginService.GetLoginUri(platform);
            webView = new()
            {
                Source = loginUri
            };

            webView.EnvironmentRequested += WebView_OnEnvironmentRequested;
            webView.NavigationCompleted += (_, _) =>
            {
                StatusText.Text = webView.Source?.ToString() ?? loginUri.ToString();
            };
            WebViewHost.Child = webView;
            WebViewPlaceholderText.IsVisible = false;
            StartupLog.Append($"Browser login WebView created platform={platform}");
        }
        catch (Exception exception)
        {
            var message = "当前平台 WebView 初始化失败，无法使用内嵌浏览器登录。";
            StatusText.Text = $"{message} {exception.Message}";
            WebViewPlaceholderText.Text = message;
            WebViewPlaceholderText.IsVisible = true;
            StartupLog.Append($"Browser login WebView create failed platform={platform} exception={exception}");
        }
    }

    private void WebView_OnEnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs e)
    {
        e.EnableDevTools = false;

        var profileDirectory = loginService.GetProfileDirectory(platform);
        var cacheDirectory = Path.Combine(profileDirectory, "Cache");
        Directory.CreateDirectory(profileDirectory);
        Directory.CreateDirectory(cacheDirectory);

        switch (e)
        {
            case WindowsWebView2EnvironmentRequestedEventArgs windows:
                windows.UserDataFolder = profileDirectory;
                windows.ProfileName = $"YohukeDanmuNeo{platform}";
                break;
            case AppleWKWebViewEnvironmentRequestedEventArgs apple:
                apple.NonPersistentDataStore = true;
                break;
            case GtkWebViewEnvironmentRequestedEventArgs gtk:
                gtk.BaseDataDirectory = profileDirectory;
                gtk.BaseCacheDirectory = cacheDirectory;
                break;
            case LinuxWpeWebViewEnvironmentRequestedEventArgs wpe:
                wpe.DataDirectory = profileDirectory;
                wpe.CacheDirectory = cacheDirectory;
                break;
        }
    }

    private void Back_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        webView?.GoBack();
    }

    private void Forward_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        webView?.GoForward();
    }

    private void Refresh_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        webView?.Refresh();
    }

    private void Cancel_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(null);
    }

    private async void Capture_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var currentWebView = webView;

            if (currentWebView is null)
            {
                StatusText.Text = "内嵌浏览器尚未初始化完成。";
                return;
            }

            var cookieManager = currentWebView.TryGetCookieManager();

            if (cookieManager is null)
            {
                StatusText.Text = "当前平台 WebView 暂不支持读取 Cookie。";
                return;
            }

            var cookies = await cookieManager.GetCookiesAsync();
            var result = loginService.CreateResult(platform, cookies);
            Close(result);
        }
        catch (Exception exception)
        {
            StatusText.Text = exception.Message;
            StartupLog.Append($"Browser login capture failed platform={platform} exception={exception}");
        }
    }
}
