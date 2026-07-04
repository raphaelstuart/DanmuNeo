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
    public void ClearInputDraftClearsState()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            InputDraft = "待发送"
        };

        viewModel.ClearInputDraft();

        Assert.Equal("", viewModel.InputDraft);
        Assert.Equal("", viewModel.State.InputDraft);
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
    public void LivePlayerVolumeClampsToSupportedRange()
    {
        var state = new LiveRoomTabState
        {
            LivePlayerVolumePercent = 150
        };
        var viewModel = new LiveRoomTabViewModel(state);

        Assert.Equal(100, viewModel.LivePlayerVolumePercent);
        Assert.Equal(100, state.LivePlayerVolumePercent);

        viewModel.LivePlayerVolumePercent = -10;

        Assert.Equal(0, viewModel.LivePlayerVolumePercent);
        Assert.Equal(0, state.LivePlayerVolumePercent);

        viewModel.LivePlayerVolumePercent = double.NaN;

        Assert.Equal(100, viewModel.LivePlayerVolumePercent);
        Assert.Equal(100, state.LivePlayerVolumePercent);
    }

    [Fact]
    public void ToggleLivePlayerMutedWritesState()
    {
        var state = new LiveRoomTabState();
        var viewModel = new LiveRoomTabViewModel(state);

        viewModel.ToggleLivePlayerMuted();

        Assert.True(viewModel.IsLivePlayerMuted);
        Assert.True(state.IsLivePlayerMuted);
        Assert.False(viewModel.IsLivePlayerAudible);
    }

    [Fact]
    public async Task ToggleLivePlayerStopsWhenVisible()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            IsLivePlayerVisible = true,
            LivePlayerUrl = "http://127.0.0.1/player"
        };

        await viewModel.ToggleLivePlayerAsync();

        Assert.False(viewModel.IsLivePlayerVisible);
        Assert.Equal("", viewModel.LivePlayerUrl);
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
    public void ApplyLyricCalculatesDurationsAndResetsSentState()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricInput = """
                         [00:01.00]第一句
                         [00:03.00]第二句
                         """
        };

        viewModel.ApplyLyric();
        viewModel.Lyrics[0].IsSent = true;
        viewModel.ApplyLyric();

        Assert.Equal(2, viewModel.Lyrics.Count);
        Assert.Equal(2, viewModel.Lyrics[0].DurationSeconds);
        Assert.Equal(3, viewModel.Lyrics[1].DurationSeconds);
        Assert.False(viewModel.Lyrics[0].IsSent);
        Assert.Equal(viewModel.Lyrics[0], viewModel.ActiveLyricLine);
    }

    [Fact]
    public void ClearLyricClearsInputTitleLinesAndPlaybackState()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricTitle = "歌曲",
            LyricInput = "[00:01.00]歌词"
        };
        viewModel.ApplyLyric();
        viewModel.AdjustLyricPlaybackPosition(0.5);

        viewModel.ClearLyric();

        Assert.Equal("", viewModel.LyricTitle);
        Assert.Equal("", viewModel.LyricInput);
        Assert.Empty(viewModel.Lyrics);
        Assert.Null(viewModel.ActiveLyricLine);
        Assert.Equal(0, viewModel.LyricPlaybackPositionSeconds);
    }

    [Fact]
    public void RewindAndFastForwardLyricAdjustByHalfSecond()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricInput = """
                         [00:01.00]第一句
                         [00:03.00]第二句
                         """
        };
        viewModel.ApplyLyric();

        viewModel.FastForwardLyric();
        viewModel.FastForwardLyric();
        viewModel.FastForwardLyric();

        Assert.Equal(2.5, viewModel.LyricPlaybackPositionSeconds);
        Assert.Equal(viewModel.Lyrics[0], viewModel.ActiveLyricLine);
        Assert.Equal(0.75, viewModel.Lyrics[0].Progress);

        viewModel.FastForwardLyric();

        Assert.Equal(3, viewModel.LyricPlaybackPositionSeconds);
        Assert.Equal(viewModel.Lyrics[1], viewModel.ActiveLyricLine);

        viewModel.RewindLyric();

        Assert.Equal(2.5, viewModel.LyricPlaybackPositionSeconds);
        Assert.Equal(viewModel.Lyrics[0], viewModel.ActiveLyricLine);
    }

    [Fact]
    public void SeekLyricLineStartMovesPlaybackToSelectedLine()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricInput = """
                         [00:01.00]第一句
                         [00:03.00]第二句
                         [00:07.00]第三句
                         """
        };
        viewModel.ApplyLyric();

        viewModel.SeekLyricLineStart(viewModel.Lyrics[1]);

        Assert.Equal(3, viewModel.LyricPlaybackPositionSeconds);
        Assert.Equal(viewModel.Lyrics[1], viewModel.ActiveLyricLine);
        Assert.Equal(0, viewModel.Lyrics[1].Progress);
    }

    [Fact]
    public void SeekLyricLineStartMovesPlaybackToLastLineStart()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricInput = """
                         [00:01.00]第一句
                         [00:03.00]第二句
                         [00:07.00]第三句
                         """
        };
        viewModel.ApplyLyric();

        viewModel.SeekLyricLineStart(viewModel.Lyrics[2]);

        Assert.Equal(7, viewModel.LyricPlaybackPositionSeconds);
        Assert.Equal(viewModel.Lyrics[2], viewModel.ActiveLyricLine);
        Assert.Equal(0, viewModel.Lyrics[2].Progress);
    }

    [Fact]
    public void SeekLyricLineStartIgnoresLineWithoutTimeline()
    {
        var viewModel = new LiveRoomTabViewModel(new LiveRoomTabState())
        {
            LyricInput = """
                         [00:01.00]第一句
                         无时间轴
                         """
        };
        viewModel.ApplyLyric();
        viewModel.AdjustLyricPlaybackPosition(1);
        var untimedLine = viewModel.Lyrics.Single(line => line.TimeSeconds < 0);

        viewModel.SeekLyricLineStart(untimedLine);

        Assert.Equal(2, viewModel.LyricPlaybackPositionSeconds);
        Assert.Equal(viewModel.Lyrics[0], viewModel.ActiveLyricLine);
    }

    [Fact]
    public async Task SendCurrentLyricMarksLineSent()
    {
        var sendService = new FakeDanmuSendService();
        var viewModel = CreateConfiguredViewModel(new AppSettings(), sendService);
        viewModel.RoomId = "100";
        viewModel.LyricInput = "[00:01.00]歌词";
        viewModel.ApplyLyric();

        await viewModel.SendCurrentLyricAsync();

        Assert.True(viewModel.Lyrics[0].IsSent);
        Assert.Equal(["【♪歌词】"], sendService.SentMessages);
    }

    [Fact]
    public async Task AutoLyricSkipsAlreadySentLineByDefault()
    {
        var sendService = new FakeDanmuSendService();
        var viewModel = CreateConfiguredViewModel(new AppSettings(), sendService);
        viewModel.RoomId = "100";
        viewModel.LyricInput = "[00:01.00]歌词";
        viewModel.ApplyLyric();
        viewModel.Lyrics[0].IsSent = true;

        var autoTask = viewModel.ToggleAutoLyricAsync();
        await Task.Delay(100);
        viewModel.StopAutoLyric();
        await autoTask;

        Assert.Empty(sendService.SentMessages);
    }

    [Fact]
    public async Task StartAutoLyricStartsWithoutTogglingExistingSessionOff()
    {
        var sendService = new FakeDanmuSendService();
        var viewModel = CreateConfiguredViewModel(new AppSettings(), sendService);
        viewModel.RoomId = "100";
        viewModel.LyricInput = "[00:05.00]歌词";
        viewModel.ApplyLyric();
        viewModel.Lyrics[0].IsSent = true;

        var autoTask = viewModel.StartAutoLyricAsync();
        await Task.Delay(100);

        Assert.True(viewModel.IsLyricAutoSending);

        await viewModel.StartAutoLyricAsync();

        Assert.True(viewModel.IsLyricAutoSending);

        viewModel.StopAutoLyric();
        await autoTask;

        Assert.Empty(sendService.SentMessages);
    }

    [Fact]
    public async Task AutoLyricCanRepeatAlreadySentLineWhenEnabled()
    {
        var sendService = new FakeDanmuSendService();
        var state = new LiveRoomTabState
        {
            PreventRepeatedLyricSend = false,
            LyricText = "[00:01.00]歌词"
        };
        var viewModel = new LiveRoomTabViewModel(state);
        viewModel.Configure(
            _ => null,
            (_, _) => null,
            () => new AppSettings(),
            () => new MarkSymbolGroup(),
            () => [],
            _ => [],
            sendService,
            new AvatarCacheService(
                new AppDirectoryService(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")))),
            () => Task.CompletedTask);
        viewModel.RoomId = "100";
        viewModel.ApplyLyric();
        viewModel.Lyrics[0].IsSent = true;

        var autoTask = viewModel.ToggleAutoLyricAsync();
        await Task.Delay(100);
        viewModel.StopAutoLyric();
        await autoTask;

        Assert.Equal(["【♪歌词】"], sendService.SentMessages);
        Assert.False(state.PreventRepeatedLyricSend);
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
        var viewModel = CreateConfiguredViewModel(
            new AppSettings(),
            liveStreamService: liveStreamService,
            livePlayerService: livePlayerService);

        await viewModel.PlayLiveAsync();
        var firstUrl = viewModel.LivePlayerUrl;

        await viewModel.ChaseLiveAsync();

        Assert.Equal("http://127.0.0.1/player?token=token-2", viewModel.LivePlayerUrl);
        Assert.NotEqual(firstUrl, viewModel.LivePlayerUrl);
        Assert.Equal(["token-1"], livePlayerService.RevokedTokens);
    }

    [Fact]
    public async Task PlayLiveAttemptsListeningWhenAutoStartEnabled()
    {
        var viewModel = CreateConfiguredViewModel(
            new()
            {
                AutoStartListeningWithLivePlayer = true
            },
            liveStreamService: new FakeBilibiliLiveStreamService(),
            livePlayerService: new FakeLivePlayerService());
        viewModel.RoomId = "100";

        await viewModel.PlayLiveAsync();

        Assert.True(viewModel.IsLivePlayerVisible);
        Assert.Equal("缺少账号或房间", viewModel.ConnectionStatus);
    }

    [Fact]
    public async Task PlayLiveDoesNotStartListeningWhenAutoStartDisabled()
    {
        var viewModel = CreateConfiguredViewModel(
            new()
            {
                AutoStartListeningWithLivePlayer = false
            },
            liveStreamService: new FakeBilibiliLiveStreamService(),
            livePlayerService: new FakeLivePlayerService());
        viewModel.RoomId = "100";

        await viewModel.PlayLiveAsync();

        Assert.True(viewModel.IsLivePlayerVisible);
        Assert.Equal("未连接", viewModel.ConnectionStatus);
    }

    private static LiveRoomTabViewModel CreateConfiguredViewModel(
        AppSettings settings,
        IDanmuSendService? sendService = null,
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
            sendService ?? new DanmuSendService(),
            new AvatarCacheService(
                new AppDirectoryService(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")))),
            () => Task.CompletedTask,
            liveStreamService,
            livePlayerService);

        return viewModel;
    }
}
