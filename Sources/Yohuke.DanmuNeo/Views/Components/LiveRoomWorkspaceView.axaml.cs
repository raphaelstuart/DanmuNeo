using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
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
    private NativeWebView? liveWebView;
    private DispatcherTimer? lyricSeekRepeatTimer;
    private Key? lyricSeekRepeatKey;
    private double lyricSeekRepeatOffsetSeconds;
    private bool isLyricSeekRepeating;

    /// <summary>
    /// 初始化直播间工作页面。
    /// </summary>
    public LiveRoomWorkspaceView()
    {
        InitializeComponent();
        DataContextChanged += LiveRoomWorkspaceView_OnDataContextChanged;
        AttachedToVisualTree += LiveRoomWorkspaceView_OnAttachedToVisualTree;
        AddHandler(KeyDownEvent, LiveRoomWorkspaceView_OnPreviewKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, LiveRoomWorkspaceView_OnPreviewKeyUp, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        currentRoom?.StopLivePlayer();
        DetachDanmuItems();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// 通过快捷键聚焦同传输入框。
    /// </summary>
    public void FocusInputDraftFromShortcut()
    {
        FocusInputDraft();
    }

    /// <summary>
    /// 通过快捷键清空同传输入框。
    /// </summary>
    public void ClearInputDraftFromShortcut()
    {
        ClearInputDraft();
    }

    private async void ExportTranslateHistory_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not LiveRoomTabViewModel room ||
            TopLevel.GetTopLevel(this) is not TopLevel topLevel ||
            !room.HasTranslateHistory)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = "导出同传历史",
            SuggestedFileName = $"{CreateTranslateHistoryFileName(room.Header)}.xlsx",
            FileTypeChoices =
            [
                new FilePickerFileType("Excel Workbook")
                {
                    Patterns = ["*.xlsx"],
                    MimeTypes = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]
                }
            ]
        });

        if (file is null)
        {
            return;
        }

        var filePath = EnsureWorkbookExtension(file.Path.LocalPath);
        await room.ExportTranslateHistoryToFileAsync(filePath);
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

    private static string CreateTranslateHistoryFileName(string roomHeader)
    {
        var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
        var safeHeader = new string(roomHeader
            .Select(character => invalidChars.Contains(character) ? '_' : character)
            .ToArray());

        if (string.IsNullOrWhiteSpace(safeHeader))
        {
            safeHeader = "live-room";
        }

        return $"translate-history-{safeHeader}-{DateTime.Now:yyyyMMdd-HHmmss}";
    }

    private static string EnsureWorkbookExtension(string filePath)
    {
        return string.IsNullOrWhiteSpace(Path.GetExtension(filePath)) ? $"{filePath}.xlsx" : filePath;
    }

    private void Danmu_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is not Button { DataContext: DanmuFeedItem item } button ||
            DataContext is not LiveRoomTabViewModel sourceRoom)
        {
            return;
        }

        e.Handled = true;
        CreateDanmuContextMenu(item, sourceRoom).Open(button);
    }

    private ContextMenu CreateDanmuContextMenu(DanmuFeedItem item, LiveRoomTabViewModel sourceRoom)
    {
        var copyTextItem = new MenuItem
        {
            Header = "复制文本"
        };
        copyTextItem.Click += async (_, _) => await CopyToClipboardAsync(item.Content);

        var copyUserUidItem = new MenuItem
        {
            Header = "复制用户 UID",
            IsEnabled = !string.IsNullOrWhiteSpace(item.UserUid)
        };
        copyUserUidItem.Click += async (_, _) => await CopyToClipboardAsync(item.UserUid);

        var forwardItem = new MenuItem
        {
            Header = "引入到工作区转发",
            IsEnabled = !string.IsNullOrWhiteSpace(item.UserUid)
        };
        forwardItem.ItemsSource = CreateForwardTargetMenuItems(item, sourceRoom).ToList();

        return new()
        {
            ItemsSource = new object[]
            {
                copyTextItem,
                copyUserUidItem,
                new Separator(),
                forwardItem
            }
        };
    }

    private IEnumerable<MenuItem> CreateForwardTargetMenuItems(DanmuFeedItem item, LiveRoomTabViewModel sourceRoom)
    {
        if (GetMainWindowViewModel() is not { } viewModel)
        {
            yield break;
        }

        foreach (var workspace in viewModel.Workspaces)
        {
            var roomItems = workspace.Rooms
                .Where(room => room.Id != sourceRoom.Id)
                .Select(room => CreateForwardRoomMenuItem(viewModel, sourceRoom, item, room))
                .ToList();

            yield return new()
            {
                Header = workspace.Name,
                ItemsSource = roomItems,
                IsEnabled = roomItems.Count > 0
            };
        }
    }

    private static MenuItem CreateForwardRoomMenuItem(
        MainWindowViewModel viewModel,
        LiveRoomTabViewModel sourceRoom,
        DanmuFeedItem item,
        LiveRoomTabViewModel targetRoom)
    {
        var menuItem = new MenuItem
        {
            Header = targetRoom.Header
        };
        menuItem.Click += async (_, _) =>
            await viewModel.AddDanmuForwardRuleFromFeedAsync(sourceRoom, item, targetRoom);
        return menuItem;
    }

    private async Task CopyToClipboardAsync(string text)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
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

    private void LyricLine_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as Control)?.DataContext is not LyricLineState line ||
            DataContext is not LiveRoomTabViewModel room)
        {
            return;
        }

        room.SeekLyricLineStart(line);
        e.Handled = true;
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
        ApplySavedWorkspaceColumnWidths();
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
            return;
        }

        if (e.PropertyName is nameof(LiveRoomTabViewModel.LivePlayerVolumePercent) or
            nameof(LiveRoomTabViewModel.IsLivePlayerMuted))
        {
            Dispatcher.UIThread.Post(SyncLivePlayerAudio, DispatcherPriority.Background);
            return;
        }

        if (e.PropertyName == nameof(LiveRoomTabViewModel.ActiveLyricLine))
        {
            Dispatcher.UIThread.Post(ScrollActiveLyricIntoView, DispatcherPriority.Background);
        }
    }

    private void LiveRoomWorkspaceView_OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (!TryHandleShortcut(e))
        {
            return;
        }

        e.Handled = true;
    }

    private bool TryHandleShortcut(KeyEventArgs e)
    {
        if (currentRoom is null ||
            GetMainWindowViewModel() is not { } viewModel)
        {
            return false;
        }

        var actionKey = ShortcutBindingService.GetTriggeredActionKey(
            viewModel.Settings.ShortcutBindings,
            e.Key,
            e.KeyModifiers);

        if (actionKey is null || IsTextInputEvent(e) && actionKey != ShortcutActionKeys.INPUT_CLEAR_DRAFT)
        {
            return false;
        }

        return ExecuteShortcut(actionKey, e.Key);
    }

    private void LiveRoomWorkspaceView_OnPreviewKeyUp(object? sender, KeyEventArgs e)
    {
        if (lyricSeekRepeatKey == e.Key)
        {
            StopLyricSeekRepeat();
        }
    }

    private bool ExecuteShortcut(string actionKey, Key key)
    {
        if (currentRoom is null)
        {
            return false;
        }

        switch (actionKey)
        {
            case ShortcutActionKeys.LIVE_ROOM_START_LISTENING:
                currentRoom.StartListening();
                return true;
            case ShortcutActionKeys.LIVE_ROOM_STOP_LISTENING:
                currentRoom.StopListening();
                return true;
            case ShortcutActionKeys.LIVE_PLAYER_START:
                _ = currentRoom.PlayLiveAsync();
                return true;
            case ShortcutActionKeys.LIVE_PLAYER_STOP:
                currentRoom.StopLivePlayer();
                return true;
            case ShortcutActionKeys.LIVE_PLAYER_CHASE:
                _ = currentRoom.ChaseLiveAsync();
                return true;
            case ShortcutActionKeys.INPUT_FOCUS_DRAFT:
                FocusInputDraft();
                return true;
            case ShortcutActionKeys.INPUT_CLEAR_DRAFT:
                ClearInputDraft();
                return true;
            case ShortcutActionKeys.LYRIC_START_SENDING:
                _ = currentRoom.StartAutoLyricAsync();
                return true;
            case ShortcutActionKeys.LYRIC_STOP_SENDING:
                currentRoom.StopAutoLyric();
                return true;
            case ShortcutActionKeys.LYRIC_SEEK_BACKWARD:
                StartLyricSeekShortcut(key, -0.5);
                return true;
            case ShortcutActionKeys.LYRIC_SEEK_FORWARD:
                StartLyricSeekShortcut(key, 0.5);
                return true;
            default:
                return false;
        }
    }

    private void FocusInputDraft()
    {
        Dispatcher.UIThread.Post(() =>
        {
            InputDraftTextBox.Focus();
            InputDraftTextBox.CaretIndex = InputDraftTextBox.Text?.Length ?? 0;
        }, DispatcherPriority.Background);
    }

    private void ClearInputDraft()
    {
        currentRoom?.ClearInputDraft();
        InputDraftTextBox.Text = "";
        InputDraftTextBox.CaretIndex = 0;
    }

    private void StartLyricSeekShortcut(Key key, double offsetSeconds)
    {
        if (lyricSeekRepeatTimer?.IsEnabled == true && lyricSeekRepeatKey == key)
        {
            return;
        }

        currentRoom?.AdjustLyricPlaybackPosition(offsetSeconds);
        StartLyricSeekRepeat(key, offsetSeconds);
    }

    private static bool IsTextInputEvent(KeyEventArgs e)
    {
        if (e.Source is TextBox)
        {
            return true;
        }

        return e.Source is Control control && control.FindAncestorOfType<TextBox>() is not null;
    }

    private void StartLyricSeekRepeat(Key key, double offsetSeconds)
    {
        lyricSeekRepeatKey = key;
        lyricSeekRepeatOffsetSeconds = offsetSeconds;
        isLyricSeekRepeating = false;
        lyricSeekRepeatTimer ??= new();
        lyricSeekRepeatTimer.Stop();
        lyricSeekRepeatTimer.Interval = TimeSpan.FromSeconds(1);
        lyricSeekRepeatTimer.Tick -= LyricSeekRepeatTimer_OnTick;
        lyricSeekRepeatTimer.Tick += LyricSeekRepeatTimer_OnTick;
        lyricSeekRepeatTimer.Start();
    }

    private void StopLyricSeekRepeat()
    {
        lyricSeekRepeatTimer?.Stop();
        lyricSeekRepeatKey = null;
        lyricSeekRepeatOffsetSeconds = 0;
        isLyricSeekRepeating = false;
    }

    private void LyricSeekRepeatTimer_OnTick(object? sender, EventArgs e)
    {
        if (!isLyricSeekRepeating)
        {
            isLyricSeekRepeating = true;
            lyricSeekRepeatTimer!.Interval = TimeSpan.FromMilliseconds(250);
        }

        currentRoom?.AdjustLyricPlaybackPosition(lyricSeekRepeatOffsetSeconds);
    }

    private void ScrollActiveLyricIntoView()
    {
        if (currentRoom?.ActiveLyricLine is null)
        {
            return;
        }

        LyricListBox.ScrollIntoView(currentRoom.ActiveLyricLine);
    }

    private void UpdateLivePlayerHost()
    {
        if (currentRoom is null ||
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
            liveWebView.NavigationCompleted += LiveWebView_OnNavigationCompleted;
            LivePlayerHost.Child = liveWebView;
        }

        liveWebView.Source = uri;
        SyncLivePlayerAudio();
    }

    private void ClearLivePlayerHost()
    {
        if (liveWebView is not null)
        {
            liveWebView.Source = new("about:blank");
            liveWebView.EnvironmentRequested -= LiveWebView_OnEnvironmentRequested;
            liveWebView.NavigationCompleted -= LiveWebView_OnNavigationCompleted;
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

    private void LiveWebView_OnNavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
    {
        SyncLivePlayerAudio();
    }

    private void SyncLivePlayerAudio()
    {
        if (currentRoom is null || liveWebView is null)
        {
            return;
        }

        var volume = Math.Clamp(currentRoom.LivePlayerVolumePercent / 100, 0, 1)
            .ToString("0.###", CultureInfo.InvariantCulture);
        var muted = currentRoom.IsLivePlayerMuted ? "true" : "false";

        try
        {
            liveWebView.InvokeScript(
                $"window.yohukeSetLivePlayerAudio && window.yohukeSetLivePlayerAudio({volume}, {muted});");
        }
        catch
        {
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
