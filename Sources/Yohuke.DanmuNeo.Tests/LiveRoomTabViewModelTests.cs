using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Tests;

public class LiveRoomTabViewModelTests
{
    [Fact]
    public void CopySuperChatInsertsWrappedContentToInputDraft()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());

        viewModel.CopySuperChat(new SuperChatItem
        {
            Content = "弹幕内容"
        });

        Assert.Equal("\"弹幕内容\"", viewModel.InputDraft);
    }

    [Fact]
    public void InsertDanmuContentWritesWrappedContentToEmptyDraft()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());

        var nextCaretIndex = viewModel.InsertDanmuContent("弹幕内容");

        Assert.Equal("\"弹幕内容\"", viewModel.InputDraft);
        Assert.Equal(viewModel.InputDraft.Length, nextCaretIndex);
    }

    [Fact]
    public void InsertDanmuContentInsertsAtSpecifiedPositionWithoutReplacing()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            InputDraft = "开头结尾"
        };

        var nextCaretIndex = viewModel.InsertDanmuContent("弹幕内容", 2);

        Assert.Equal("开头\"弹幕内容\"结尾", viewModel.InputDraft);
        Assert.Equal("开头\"弹幕内容\"".Length, nextCaretIndex);
    }

    [Fact]
    public void InsertDanmuContentUsesCustomMarks()
    {
        var settings = new AppSettings
        {
            DanmuInsertOpenMark = "「",
            DanmuInsertCloseMark = "」"
        };
        var viewModel = CreateConfiguredViewModel(settings);

        viewModel.InsertDanmuContent("弹幕内容");

        Assert.Equal("「弹幕内容」", viewModel.InputDraft);
    }

    [Fact]
    public void ApplyLyricParsesInput()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricInput = "[00:01]歌词"
        };
        viewModel.Configure(
            _ => null,
            (_, _) => null,
            () => new AppSettings(),
            () => new MarkSymbolGroup(),
            () => [],
            _ => [],
            new DanmuSendService(),
            new AvatarCacheService(
                new AppDirectoryService(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")))),
            () => Task.CompletedTask);

        viewModel.ApplyLyric();

        Assert.Single(viewModel.Lyrics);
        Assert.Equal("歌词", viewModel.Lyrics[0].Content);
    }

    [Fact]
    public void LyricPlaybackRateDefaultsToOne()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());

        Assert.Equal(1.0, viewModel.LyricPlaybackRate);
        Assert.Equal(1.0, viewModel.State.LyricPlaybackRate);
    }

    [Fact]
    public void LyricPlaybackRateNormalizesLegacyZero()
    {
        var state = new LiveRoomTabState
        {
            LyricPlaybackRate = 0
        };

        var viewModel = new LiveRoomTabViewModel(state);

        Assert.Equal(1.0, viewModel.LyricPlaybackRate);
        Assert.Equal(1.0, state.LyricPlaybackRate);
    }

    [Fact]
    public void LyricPlaybackRateClampsToSupportedRange()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());

        viewModel.LyricPlaybackRate = 0.1;

        Assert.Equal(0.25, viewModel.LyricPlaybackRate);
        Assert.Equal(0.25, viewModel.State.LyricPlaybackRate);

        viewModel.LyricPlaybackRate = 9;

        Assert.Equal(3.0, viewModel.LyricPlaybackRate);
        Assert.Equal(3.0, viewModel.State.LyricPlaybackRate);
    }

    [Fact]
    public void CalculateLyricPlaybackWaitScalesDelayByRate()
    {
        var wait = LiveRoomTabViewModel.CalculateLyricPlaybackWait(
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(1),
            2.0);

        Assert.Equal(TimeSpan.FromSeconds(4), wait);
    }

    [Fact]
    public void CurrentLyricTitleTextFallsBackWhenEmpty()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());

        Assert.Equal("未选择歌词", viewModel.CurrentLyricTitleText);

        viewModel.LyricTitle = "歌曲";

        Assert.Equal("歌曲", viewModel.CurrentLyricTitleText);
    }

    [Fact]
    public void ForwardRuleIgnoresNullSourceRoomKey()
    {
        var state = new DanmuForwardRuleState
        {
            SourceWorkspaceId = "workspace",
            SourceRoomStateId = "room"
        };
        var changed = false;
        var viewModel = new DanmuForwardRuleViewModel(state, _ => changed = true);

        viewModel.SourceRoomKey = null;

        Assert.False(changed);
        Assert.Equal("workspace", state.SourceWorkspaceId);
        Assert.Equal("room", state.SourceRoomStateId);
    }

    [Fact]
    public void ForwardRuleUpdatesSourceRoomKey()
    {
        var state = new DanmuForwardRuleState
        {
            SourceWorkspaceId = "workspace",
            SourceRoomStateId = "room"
        };
        var changed = false;
        var viewModel = new DanmuForwardRuleViewModel(state, _ => changed = true);

        viewModel.SourceRoomKey = "next-workspace|next-room";

        Assert.True(changed);
        Assert.Equal("next-workspace", state.SourceWorkspaceId);
        Assert.Equal("next-room", state.SourceRoomStateId);
    }

    [Fact]
    public void RefreshForwardSourceRoomsKeepsExistingItemsWhenSourceKeysDoNotChange()
    {
        var sources = new[]
        {
            new ForwardSourceRoomOption
            {
                WorkspaceId = "workspace",
                WorkspaceName = "工作区",
                RoomStateId = "room",
                RoomId = "100",
                RoomName = "直播间"
            }
        };
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());
        viewModel.Configure(
            _ => null,
            (_, _) => null,
            () => new AppSettings(),
            () => new MarkSymbolGroup(),
            () => [],
            _ => sources,
            new DanmuSendService(),
            new AvatarCacheService(
                new AppDirectoryService(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")))),
            () => Task.CompletedTask);

        viewModel.RefreshForwardSourceRooms();
        var firstItem = viewModel.ForwardSourceRooms[0];

        viewModel.RefreshForwardSourceRooms();

        Assert.Same(firstItem, viewModel.ForwardSourceRooms[0]);
    }

    [Fact]
    public void RoomInfoPropertiesWriteState()
    {
        var state = new LiveRoomTabState
        {
            RoomId = "100",
            RoomName = "旧直播间",
            OwnerUid = "1"
        };
        var viewModel = new LiveRoomTabViewModel(state)
        {
            RoomId = "200",
            RoomName = "新直播间",
            OwnerUid = "2"
        };

        Assert.Equal("200", state.RoomId);
        Assert.Equal("新直播间", state.RoomName);
        Assert.Equal("2", state.OwnerUid);
    }

    [Fact]
    public void AvatarPlaceholderUsesRoomNameInitialAndStableRoomColor()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState
        {
            RoomId = "12345",
            RoomName = "测试直播间"
        });

        var firstColor = LiveRoomTabViewModel.CreateAvatarPlaceholderColor("12345");
        var secondColor = LiveRoomTabViewModel.CreateAvatarPlaceholderColor("12345");
        var otherColor = LiveRoomTabViewModel.CreateAvatarPlaceholderColor("54321");

        Assert.Equal("测", viewModel.AvatarPlaceholderText);
        Assert.Equal(firstColor, secondColor);
        Assert.NotEqual(firstColor, otherColor);
    }

    [Fact]
    public void AvatarPlaceholderFallsBackToRoomIdFirstCharacter()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState
        {
            RoomId = "67890",
            RoomName = ""
        });

        Assert.Equal("6", viewModel.AvatarPlaceholderText);
    }

    [Fact]
    public async Task ChaseLiveRevokesOldPlayerTokenAndCreatesNewUrl()
    {
        var liveStreamService = new FakeBilibiliLiveStreamService();
        var livePlayerService = new FakeLivePlayerService();
        var viewModel = CreateConfiguredViewModel(new AppSettings(), liveStreamService, livePlayerService);

        await viewModel.PlayLiveAsync();
        var firstUrl = viewModel.LivePlayerUrl;

        await viewModel.ChaseLiveAsync();

        Assert.Equal("http://127.0.0.1/player?token=token-2", viewModel.LivePlayerUrl);
        Assert.NotEqual(firstUrl, viewModel.LivePlayerUrl);
        Assert.Equal(["token-1"], livePlayerService.RevokedTokens);
    }

    private static LiveRoomTabViewModel CreateConfiguredViewModel(
        AppSettings settings,
        IBilibiliLiveStreamService? liveStreamService = null,
        ILivePlayerService? livePlayerService = null)
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState());
        viewModel.Configure(
            _ => null,
            (_, _) => null,
            () => settings,
            () => MarkSymbolService.CreateDefaultGroup(settings),
            () => [],
            _ => [],
            new DanmuSendService(),
            new AvatarCacheService(
                new AppDirectoryService(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")))),
            () => Task.CompletedTask,
            liveStreamService,
            livePlayerService);

        return viewModel;
    }
}