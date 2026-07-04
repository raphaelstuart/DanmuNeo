using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.Views;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 歌词库列表控件。
/// </summary>
public partial class LyricLibraryListView : UserControl
{
    /// <summary>
    /// 是否显示修改按钮。
    /// </summary>
    public static readonly StyledProperty<bool> ShowEditButtonProperty =
        AvaloniaProperty.Register<LyricLibraryListView, bool>(nameof(ShowEditButton), true);

    /// <summary>
    /// 初始化歌词库列表控件。
    /// </summary>
    public LyricLibraryListView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 是否显示修改按钮。
    /// </summary>
    public bool ShowEditButton
    {
        get => GetValue(ShowEditButtonProperty);
        set => SetValue(ShowEditButtonProperty, value);
    }

    [RelayCommand]
    private async Task PickLyricLibraryItemAsync(LyricLibraryItem? item)
    {
        if (GetMainWindowViewModel() is MainWindowViewModel viewModel)
        {
            await viewModel.PickLyricLibraryItemAsync(item);
        }
    }

    [RelayCommand]
    private async Task DeleteLyricLibraryItemAsync(LyricLibraryItem? item)
    {
        if (GetMainWindowViewModel() is MainWindowViewModel viewModel)
        {
            await viewModel.DeleteLyricLibraryItemAsync(item);
        }
    }

    private async void EditLyric_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is not LyricLibraryItem item ||
            TopLevel.GetTopLevel(this) is not Window owner ||
            owner.DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var window = new LyricEditDialogWindow(item);
        var saved = await window.ShowDialog<bool>(owner);

        if (saved)
        {
            await viewModel.SaveLyricLibraryEditAsync();
        }
    }

    private MainWindowViewModel? GetMainWindowViewModel()
    {
        return TopLevel.GetTopLevel(this)?.DataContext as MainWindowViewModel;
    }
}
