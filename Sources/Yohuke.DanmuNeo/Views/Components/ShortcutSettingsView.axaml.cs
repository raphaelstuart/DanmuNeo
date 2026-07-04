using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 快捷键设置视图。
/// </summary>
public partial class ShortcutSettingsView : UserControl
{
    /// <summary>
    /// 初始化快捷键设置视图。
    /// </summary>
    public ShortcutSettingsView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, ShortcutSettingsView_OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    [RelayCommand]
    private void BeginRecordShortcut(ShortcutBindingViewModel? binding)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.BeginRecordShortcut(binding);
        }
    }

    [RelayCommand]
    private void ClearShortcut(ShortcutBindingViewModel? binding)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ClearShortcut(binding);
        }
    }

    [RelayCommand]
    private void ResetShortcutToDefault(ShortcutBindingViewModel? binding)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ResetShortcutToDefault(binding);
        }
    }

    private void RecordShortcut_OnClick(object? sender, RoutedEventArgs e)
    {
        Focus();
    }

    private void ShortcutSettingsView_OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel ||
            !viewModel.IsRecordingShortcut ||
            !viewModel.ApplyRecordedShortcut(e.Key, e.KeyModifiers))
        {
            return;
        }

        e.Handled = true;
    }
}
