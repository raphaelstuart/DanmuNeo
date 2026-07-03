using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
        Dispatcher.UIThread.Post(() =>
        {
            DanmuScrollViewer.Offset = new Vector(DanmuScrollViewer.Offset.X, 0);
        }, DispatcherPriority.Background);
    }
}
