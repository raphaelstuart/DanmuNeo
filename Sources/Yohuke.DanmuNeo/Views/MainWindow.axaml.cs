using Avalonia.Controls;
using Avalonia.Input;
using Yohuke.DanmuNeo.Services;
using Yohuke.DanmuNeo.ViewModels;

namespace Yohuke.DanmuNeo.Views;

/// <summary>
/// 主窗口。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 初始化主窗口。
    /// </summary>
    public MainWindow()
    {
        StartupLog.Append("MainWindow ctor begin");
        InitializeComponent();
        StartupLog.Append("MainWindow InitializeComponent end");
        DataContextChanged += OnDataContextChanged;
        Closing += async (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                SaveWindowSize(viewModel);
                viewModel.Settings.SidebarWidth = AppStateService.ClampSidebarWidth(GetSidebarColumn().ActualWidth);
                await viewModel.SaveAsync();
            }
        };
        Closed += (_, _) =>
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        };
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            Width = AppStateService.ClampWindowWidth(viewModel.Settings.WindowWidth);
            Height = AppStateService.ClampWindowHeight(viewModel.Settings.WindowHeight);
            GetSidebarColumn().Width = new(AppStateService.ClampSidebarWidth(viewModel.Settings.SidebarWidth));
        }
    }

    private async void SidebarSplitter_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Settings.SidebarWidth = AppStateService.ClampSidebarWidth(GetSidebarColumn().ActualWidth);
            GetSidebarColumn().Width = new(viewModel.Settings.SidebarWidth);
            await viewModel.SaveAsync();
        }
    }

    private ColumnDefinition GetSidebarColumn()
    {
        return MainColumns.ColumnDefinitions[0];
    }

    private void SaveWindowSize(MainWindowViewModel viewModel)
    {
        if (WindowState != WindowState.Normal)
        {
            return;
        }

        viewModel.Settings.WindowWidth = AppStateService.ClampWindowWidth(Width);
        viewModel.Settings.WindowHeight = AppStateService.ClampWindowHeight(Height);
    }
}