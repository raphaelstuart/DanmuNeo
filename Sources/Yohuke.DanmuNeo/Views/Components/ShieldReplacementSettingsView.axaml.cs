using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 屏蔽词替换库设置视图。
/// </summary>
public partial class ShieldReplacementSettingsView : UserControl
{
    /// <summary>
    /// 初始化屏蔽词替换库设置视图。
    /// </summary>
    public ShieldReplacementSettingsView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task AddShieldReplacementRuleAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.AddShieldReplacementRuleAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteShieldReplacementRuleAsync(ShieldReplacementRule? rule)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.DeleteShieldReplacementRuleAsync(rule);
        }
    }

    [RelayCommand]
    private async Task MoveShieldReplacementRuleUpAsync(ShieldReplacementRule? rule)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.MoveShieldReplacementRuleUpAsync(rule);
        }
    }

    [RelayCommand]
    private async Task MoveShieldReplacementRuleDownAsync(ShieldReplacementRule? rule)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.MoveShieldReplacementRuleDownAsync(rule);
        }
    }
}
