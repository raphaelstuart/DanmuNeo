using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 删除确认对话框。
/// </summary>
public partial class ConfirmDeleteDialogView : UserControl
{
    /// <summary>
    /// 初始化删除确认对话框。
    /// </summary>
    public ConfirmDeleteDialogView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void CloseDeleteDialog()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.CloseDeleteDialog();
        }
    }

    [RelayCommand]
    private async Task ConfirmDeleteDialogAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.ConfirmDeleteDialogAsync();
        }
    }
}
