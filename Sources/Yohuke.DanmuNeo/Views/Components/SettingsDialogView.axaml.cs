using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 设置对话框。
/// </summary>
public partial class SettingsDialogView : UserControl
{
    /// <summary>
    /// 初始化设置对话框。
    /// </summary>
    public SettingsDialogView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task CloseSettingsAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.CloseSettingsAsync();
        }
    }

    [RelayCommand]
    private void SelectSettingsSection(SettingsSectionOption? option)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectSettingsSection(option);
        }
    }
}
