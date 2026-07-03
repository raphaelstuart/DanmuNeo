using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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

    private async void ImportLocalLyricFile_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel ||
            topLevel.DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "导入歌词文件",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Lyric")
                {
                    Patterns = ["*.lrc", "*.txt", "*.xml"]
                },
                new FilePickerFileType("All")
                {
                    Patterns = ["*"]
                }
            ]
        });

        var file = files.FirstOrDefault();

        if (file is null)
        {
            return;
        }

        if (LoadToCurrentRoom)
        {
            if (await viewModel.ImportLyricFileAsync(file.Path.LocalPath))
            {
                viewModel.CloseLyricLibraryPicker();
            }

            return;
        }

        await viewModel.ImportLyricFileToLibraryAsync(file.Path.LocalPath);
    }
}
