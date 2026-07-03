using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 工作区设置页。
/// </summary>
public partial class WorkspaceSettingsView : UserControl
{
    /// <summary>
    /// 初始化工作区设置页。
    /// </summary>
    public WorkspaceSettingsView()
    {
        InitializeComponent();
    }

    private async void RoomInfo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel ||
            sender is not Control { DataContext: LiveRoomTabViewModel room })
        {
            return;
        }

        await viewModel.SaveRoomInfoAsync(room);
    }

    private async void ExportWorkspace_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel ||
            TopLevel.GetTopLevel(this) is not TopLevel topLevel)
        {
            return;
        }

        var workspace = viewModel.SelectedWorkspace;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = "导出工作区",
            SuggestedFileName = viewModel.GetExportFileName(workspace),
            FileTypeChoices =
            [
                new FilePickerFileType("DanmuNeo Workspace")
                {
                    Patterns = ["*.dnworkspace.json"]
                }
            ]
        });

        if (file is null)
        {
            return;
        }

        await viewModel.ExportWorkspaceAsync(workspace, file.Path.LocalPath);
    }
}