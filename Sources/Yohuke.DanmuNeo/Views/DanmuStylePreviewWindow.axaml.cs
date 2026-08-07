using Avalonia.Controls;
using Avalonia.Styling;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views;

/// <summary>
/// 弹幕与 SC 样式测试窗口。
/// </summary>
public partial class DanmuStylePreviewWindow : Window
{
    private readonly DanmuStylePreviewViewModel viewModel = new();

    /// <summary>
    /// 初始化弹幕与 SC 样式测试窗口。
    /// </summary>
    public DanmuStylePreviewWindow()
    {
        InitializeComponent();
        DataContext = viewModel;
        ThemeModeComboBox.SelectedIndex = 0;
    }

    private void ThemeMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox)
        {
            return;
        }

        RequestedThemeVariant = comboBox.SelectedIndex switch
        {
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private void AddDanmu_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewModel.AddDanmu();
    }

    private void AddSuperChat_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewModel.AddSuperChat();
    }

    private void ResetSamples_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewModel.ResetSamples();
    }

    private void ClearSamples_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewModel.ClearSamples();
    }
}
