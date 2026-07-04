using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Yohuke.DanmuNeo.ViewModels;

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
