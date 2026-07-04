using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 新建工作区或直播间对话框。
/// </summary>
public partial class AddWorkspaceDialogView : UserControl
{
    /// <summary>
    /// 初始化新增对话框。
    /// </summary>
    public AddWorkspaceDialogView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void CloseAddDialog()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.CloseAddDialog();
        }
    }

    [RelayCommand]
    private async Task ConfirmAddDialogAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.ConfirmAddDialogAsync();
        }
    }
}
