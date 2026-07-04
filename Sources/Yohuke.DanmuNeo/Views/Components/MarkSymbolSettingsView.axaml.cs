using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 标签组设置页。
/// </summary>
public partial class MarkSymbolSettingsView : UserControl
{
    /// <summary>
    /// 初始化标签组设置页。
    /// </summary>
    public MarkSymbolSettingsView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task AddSymbolGroupAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.AddSymbolGroupAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteSymbolGroupAsync(MarkSymbolGroup? group)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.DeleteSymbolGroupAsync(group);
        }
    }

    [RelayCommand]
    private async Task ToggleSymbolGroupExpandedAsync(MarkSymbolGroup? group)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.ToggleSymbolGroupExpandedAsync(group);
        }
    }

    [RelayCommand]
    private async Task MoveSymbolGroupUpAsync(MarkSymbolGroup? group)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.MoveSymbolGroupUpAsync(group);
        }
    }

    [RelayCommand]
    private async Task MoveSymbolGroupDownAsync(MarkSymbolGroup? group)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.MoveSymbolGroupDownAsync(group);
        }
    }
}
