using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;
using Yohuke.DanmuNeo.ViewModels;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 单个直播间工作页面。
/// </summary>
public partial class LiveRoomWorkspaceView : UserControl
{
    private LiveRoomTabViewModel? currentRoom;
    private MainWindowViewModel? mainWindowViewModel;
    private NativeWebView? liveWebView;

    /// <summary>
    /// 初始化直播间工作页面。
    /// </summary>
    public LiveRoomWorkspaceView()
    {
        InitializeComponent();
        DataContextChanged += LiveRoomWorkspaceView_OnDataContextChanged;
        AttachedToVisualTree += LiveRoomWorkspaceView_OnAttachedToVisualTree;
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        currentRoom?.StopLivePlayer();
        DetachMainWindowViewModel();
        DetachDanmuItems();
        base.OnDetachedFromVisualTree(e);
    }

    private void InputDraft_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not LiveRoomTabViewModel room)
        {
            return;
        }

        if (!room.SendDraftCommand.CanExecute(null))
        {
            return;
        }

        e.Handled = true;
        room.SendDraftCommand.Execute(null);
    }

    private void Danmu_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: DanmuFeedItem item } ||
            DataContext is not LiveRoomTabViewModel room)
        {
            return;
        }

        InsertFeedContent(room, item.Content);
    }

    private void SuperChat_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SuperChatItem item } ||
            DataContext is not LiveRoomTabViewModel room)
        {
            return;
        }

        InsertFeedContent(room, item.Content);
    }

    private void InsertFeedContent(LiveRoomTabViewModel room, string content)
    {
        var currentDraft = InputDraftTextBox.Text ?? "";
        room.InputDraft = currentDraft;
        var nextCaretIndex = room.InsertDanmuContent(content, InputDraftTextBox.CaretIndex);

        Dispatcher.UIThread.Post(() =>
        {
            InputDraftTextBox.Text = room.InputDraft;
            InputDraftTextBox.Focus();
            InputDraftTextBox.CaretIndex = Math.Min(nextCaretIndex, InputDraftTextBox.Text?.Length ?? 0);
        }, DispatcherPriority.Background);
    }

    private void LiveRoomWorkspaceView_OnDataContextChanged(object? sender, EventArgs e)
    {
        AttachDanmuItems(DataContext as LiveRoomTabViewModel);
    }

    private void LiveRoomWorkspaceView_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachMainWindowViewModel();
        ApplySavedWorkspaceColumnWidths();
    }

    private void AttachMainWindowViewModel()
    {
        var viewModel = GetMainWindowViewModel();

        if (mainWindowViewModel == viewModel)
        {
            return;
        }

        DetachMainWindowViewModel();
        mainWindowViewModel = viewModel;

        if (mainWindowViewModel is not null)
        {
            mainWindowViewModel.PropertyChanged += MainWindowViewModel_OnPropertyChanged;
        }
    }

    private void DetachMainWindowViewModel()
    {
        if (mainWindowViewModel is not null)
        {
            mainWindowViewModel.PropertyChanged -= MainWindowViewModel_OnPropertyChanged;
            mainWindowViewModel = null;
        }
    }

    private void AttachDanmuItems(LiveRoomTabViewModel? room)
    {
        DetachDanmuItems();
        currentRoom = room;

        if (currentRoom is null)
        {
            return;
        }

        currentRoom.DanmuItems.CollectionChanged += DanmuItems_OnCollectionChanged;
        currentRoom.PropertyChanged += CurrentRoom_OnPropertyChanged;
        UpdateLivePlayerHost();
        ScrollDanmuToLatest();
    }

    private void DetachDanmuItems()
    {
        if (currentRoom is null)
        {
            return;
        }

        currentRoom.DanmuItems.CollectionChanged -= DanmuItems_OnCollectionChanged;
        currentRoom.PropertyChanged -= CurrentRoom_OnPropertyChanged;
        currentRoom = null;
        ClearLivePlayerHost();
    }

    private void CurrentRoom_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LiveRoomTabViewModel.LivePlayerUrl) or
            nameof(LiveRoomTabViewModel.IsLivePlayerVisible))
        {
            Dispatcher.UIThread.Post(UpdateLivePlayerHost, DispatcherPriority.Background);
        }
    }

    private void MainWindowViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.IsSettingsOpen) or
            nameof(MainWindowViewModel.IsConfirmDeleteDialogOpen))
        {
            Dispatcher.UIThread.Post(UpdateLivePlayerHost, DispatcherPriority.Background);
        }
    }

    private void UpdateLivePlayerHost()
    {
        if (currentRoom is null ||
            IsBlockingNativeWebView() ||
            !currentRoom.IsLivePlayerVisible ||
            string.IsNullOrWhiteSpace(currentRoom.LivePlayerUrl) ||
            !Uri.TryCreate(currentRoom.LivePlayerUrl, UriKind.Absolute, out var uri))
        {
            ClearLivePlayerHost();
            return;
        }

        if (liveWebView is null)
        {
            liveWebView = new();
            liveWebView.EnvironmentRequested += LiveWebView_OnEnvironmentRequested;
            LivePlayerHost.Child = liveWebView;
        }

        liveWebView.Source = uri;
    }

    private bool IsBlockingNativeWebView()
    {
        AttachMainWindowViewModel();
        return mainWindowViewModel is { IsSettingsOpen: true } or { IsConfirmDeleteDialogOpen: true };
    }

    private void ClearLivePlayerHost()
    {
        if (liveWebView is not null)
        {
            liveWebView.Source = new("about:blank");
            liveWebView.EnvironmentRequested -= LiveWebView_OnEnvironmentRequested;
            liveWebView = null;
        }

        LivePlayerHost.Child = null;
    }

    private void LiveWebView_OnEnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs e)
    {
        e.EnableDevTools = false;

        if (e is AppleWKWebViewEnvironmentRequestedEventArgs apple)
        {
            apple.NonPersistentDataStore = true;
        }
    }

    private void DanmuItems_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Reset)
        {
            ScrollDanmuToLatest();
        }
    }

    private void ScrollDanmuToLatest()
    {
        Dispatcher.UIThread.Post(() => { DanmuScrollViewer.Offset = new Vector(DanmuScrollViewer.Offset.X, 0); },
            DispatcherPriority.Background);
    }

    private async void WorkspaceSplitter_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        await SaveWorkspaceColumnWidthsAsync();
    }

    private void ApplySavedWorkspaceColumnWidths()
    {
        if (GetMainWindowViewModel() is not { } viewModel)
        {
            return;
        }

        var liveWidth = viewModel.Settings.WorkspaceLiveColumnWidth;
        var toolWidth = viewModel.Settings.WorkspaceToolColumnWidth;

        if (liveWidth <= 0 || toolWidth <= 0)
        {
            return;
        }

        GetLiveColumn().Width = new(AppStateService.ClampWorkspaceColumnWidth(liveWidth));
        GetToolColumn().Width = new(AppStateService.ClampWorkspaceColumnWidth(toolWidth));
    }

    private async Task SaveWorkspaceColumnWidthsAsync()
    {
        if (GetMainWindowViewModel() is not { } viewModel)
        {
            return;
        }

        viewModel.Settings.WorkspaceLiveColumnWidth =
            AppStateService.ClampWorkspaceColumnWidth(GetLiveColumn().ActualWidth);
        viewModel.Settings.WorkspaceToolColumnWidth =
            AppStateService.ClampWorkspaceColumnWidth(GetToolColumn().ActualWidth);
        GetLiveColumn().Width = new(viewModel.Settings.WorkspaceLiveColumnWidth);
        GetToolColumn().Width = new(viewModel.Settings.WorkspaceToolColumnWidth);
        await viewModel.SaveAsync();
    }

    private MainWindowViewModel? GetMainWindowViewModel()
    {
        return this.FindAncestorOfType<Window>()?.DataContext as MainWindowViewModel;
    }

    private ColumnDefinition GetLiveColumn()
    {
        return WorkspaceColumns.ColumnDefinitions[0];
    }

    private ColumnDefinition GetToolColumn()
    {
        return WorkspaceColumns.ColumnDefinitions[2];
    }
}