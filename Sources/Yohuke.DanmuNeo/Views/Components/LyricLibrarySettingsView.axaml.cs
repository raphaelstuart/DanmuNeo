using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 歌词库设置页。
/// </summary>
public partial class LyricLibrarySettingsView : UserControl
{
    /// <summary>
    /// 初始化歌词库设置页。
    /// </summary>
    public LyricLibrarySettingsView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task SaveCurrentLyricToLibraryAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SaveCurrentLyricToLibraryAsync();
        }
    }
}
