using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 歌词在线搜索与本地导入控件。
/// </summary>
public partial class LyricOnlineImportView : UserControl
{
    /// <summary>
    /// 是否导入后加载到当前直播间。
    /// </summary>
    public static readonly StyledProperty<bool> LoadToCurrentRoomProperty =
        AvaloniaProperty.Register<LyricOnlineImportView, bool>(nameof(LoadToCurrentRoom));

    /// <summary>
    /// 是否显示导入标题和说明。
    /// </summary>
    public static readonly StyledProperty<bool> ShowHeaderProperty =
        AvaloniaProperty.Register<LyricOnlineImportView, bool>(nameof(ShowHeader), true);

    /// <summary>
    /// 是否显示本地歌词导入按钮。
    /// </summary>
    public static readonly StyledProperty<bool> ShowLocalImportButtonProperty =
        AvaloniaProperty.Register<LyricOnlineImportView, bool>(nameof(ShowLocalImportButton), true);

    /// <summary>
    /// 是否在控件内部滚动在线搜索结果。
    /// </summary>
    public static readonly StyledProperty<bool> EnableResultScrollingProperty =
        AvaloniaProperty.Register<LyricOnlineImportView, bool>(nameof(EnableResultScrolling));

    /// <summary>
    /// 初始化歌词在线搜索与本地导入控件。
    /// </summary>
    public LyricOnlineImportView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 是否导入后加载到当前直播间。
    /// </summary>
    public bool LoadToCurrentRoom
    {
        get => GetValue(LoadToCurrentRoomProperty);
        set => SetValue(LoadToCurrentRoomProperty, value);
    }

    /// <summary>
    /// 是否显示导入标题和说明。
    /// </summary>
    public bool ShowHeader
    {
        get => GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    /// <summary>
    /// 是否显示本地歌词导入按钮。
    /// </summary>
    public bool ShowLocalImportButton
    {
        get => GetValue(ShowLocalImportButtonProperty);
        set => SetValue(ShowLocalImportButtonProperty, value);
    }

    /// <summary>
    /// 是否在控件内部滚动在线搜索结果。
    /// </summary>
    public bool EnableResultScrolling
    {
        get => GetValue(EnableResultScrollingProperty);
        set => SetValue(EnableResultScrollingProperty, value);
    }

    private async void MusicLyricSearch_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !IsEffectivelyVisible || !SearchMusicLyricsCommand.CanExecute(null))
        {
            return;
        }

        e.Handled = true;
        await SearchMusicLyricsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void SelectNetEaseMusicLyricSource()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectNetEaseMusicLyricSource();
        }
    }

    [RelayCommand]
    private void SelectQQMusicLyricSource()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectQQMusicLyricSource();
        }
    }

    [RelayCommand]
    private async Task SearchMusicLyricsAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SearchMusicLyricsAsync();
        }
    }

    [RelayCommand]
    private async Task ImportMusicLyricAsync(MusicLyricSearchResult? result)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.ImportMusicLyricAsync(result);
        }
    }
}
