using Avalonia.Controls;
using Yohuke.DanmuNeo.Models.BrowserLogin;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Services;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.Views;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 账号登录设置页。
/// </summary>
public partial class AccountSettingsView : UserControl
{
    /// <summary>
    /// 初始化账号登录设置页。
    /// </summary>
    public AccountSettingsView()
    {
        InitializeComponent();
    }

    private async void BilibiliBrowserLogin_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var account = (sender as Control)?.DataContext as BilibiliAccount;
        var result = await ShowBrowserLoginAsync(BrowserLoginPlatform.Bilibili);

        if (result is not null)
        {
            await viewModel.ApplyBilibiliBrowserLoginAsync(result, account);
        }
    }

    private async void QQMusicBrowserLogin_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var result = await ShowBrowserLoginAsync(BrowserLoginPlatform.QQMusic);

        if (result is not null)
        {
            await viewModel.ApplyQQMusicBrowserLoginAsync(result);
        }
    }

    private async Task<BrowserCookieLoginResult?> ShowBrowserLoginAsync(BrowserLoginPlatform platform)
    {
        try
        {
            if (TopLevel.GetTopLevel(this) is not Window owner)
            {
                return null;
            }

            var window = new BrowserCookieLoginWindow(platform);
            return await window.ShowDialog<BrowserCookieLoginResult?>(owner);
        }
        catch (Exception exception)
        {
            StartupLog.Append($"Show browser login failed platform={platform} exception={exception}");

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.StatusMessage = $"浏览器登录不可用：{exception.Message}";
            }

            return null;
        }
    }
}
