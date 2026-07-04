using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 全局设置页。
/// </summary>
public partial class GlobalSettingsView : UserControl
{
    /// <summary>
    /// 初始化全局设置页。
    /// </summary>
    public GlobalSettingsView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task SelectThemeModeAsync(ThemeModeOption? option)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SelectThemeModeAsync(option);
        }
    }
}
