using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Views.Components;

/// <summary>
/// 单个直播间工作页面。
/// </summary>
public partial class LiveRoomWorkspaceView : UserControl
{
    private LiveRoomTabViewModel? currentRoom;

    /// <summary>
    /// 初始化直播间工作页面。
    /// </summary>
    public LiveRoomWorkspaceView()
    {
        InitializeComponent();
        DataContextChanged += LiveRoomWorkspaceView_OnDataContextChanged;
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
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

    private void AttachDanmuItems(LiveRoomTabViewModel? room)
    {
        DetachDanmuItems();
        currentRoom = room;

        if (currentRoom is null)
        {
            return;
        }

        currentRoom.DanmuItems.CollectionChanged += DanmuItems_OnCollectionChanged;
        ScrollDanmuToLatest();
    }

    private void DetachDanmuItems()
    {
        if (currentRoom is null)
        {
            return;
        }

        currentRoom.DanmuItems.CollectionChanged -= DanmuItems_OnCollectionChanged;
        currentRoom = null;
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
}