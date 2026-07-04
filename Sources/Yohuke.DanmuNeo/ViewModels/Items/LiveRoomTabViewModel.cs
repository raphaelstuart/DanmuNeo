using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.Apis;
using Yohuke.DanmuNeo.Apis.Models.Bilibili;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.ViewModels.Items;

/// <summary>
/// 直播间标签页视图模型。
/// </summary>
public partial class LiveRoomTabViewModel : ViewModelBase
{
    private const double MIN_LYRIC_PLAYBACK_RATE = 0.25;
    private const double MAX_LYRIC_PLAYBACK_RATE = 3.0;
    private const double DEFAULT_LYRIC_PLAYBACK_RATE = 1.0;
    private const double LYRIC_SEEK_STEP_SECONDS = 0.5;
    private const double DEFAULT_LAST_LYRIC_DURATION_SECONDS = 3.0;
    private const int LYRIC_PLAYBACK_TICK_MS = 50;
    private const double MIN_LIVE_PLAYER_VOLUME_PERCENT = 0;
    private const double MAX_LIVE_PLAYER_VOLUME_PERCENT = 100;

    private CancellationTokenSource? listenTokenSource;
    private CancellationTokenSource? lyricTokenSource;
    private readonly Dictionary<string, CancellationTokenSource> forwardTokenSources = [];
    private Func<LiveRoomTabViewModel, BilibiliAccount?>? resolveAccount;
    private Func<string, string, BilibiliAccount?>? resolveAccountByRoom;
    private Func<AppSettings>? getSettings;
    private Func<MarkSymbolGroup>? getMarkSymbolGroup;
    private Func<IEnumerable<MarkSymbolGroup>>? getSymbolGroups;
    private Func<LiveRoomTabViewModel, IEnumerable<ForwardSourceRoomOption>>? getForwardSourceRooms;
    private Func<Task>? saveState;
    private IDanmuSendService? sendService;
    private AvatarCacheService? avatarCacheService;
    private IBilibiliLiveStreamService? liveStreamService;
    private ILivePlayerService? livePlayerService;
    private Bitmap? avatarImage;
    private string? livePlayerToken;
    private bool suppressLiveQualityChange;
    private DateTimeOffset lyricPlaybackStartedAt;
    private double lyricPlaybackStartPositionSeconds;
    private int lastAutoEnteredLyricIndex = -1;
    private readonly DanmuForwardService forwardService = new();
    private readonly LyricTimelineService lyricTimelineService = new();

    /// <summary>
    /// 初始化直播间标签页视图模型。
    /// </summary>
    public LiveRoomTabViewModel(LiveRoomTabState state)
    {
        State = state;
        inputDraft = state.InputDraft;
        lyricInput = state.LyricText;
        lyricTitle = state.LyricTitle;
        lyricPlaybackRate = NormalizeLyricPlaybackRate(state.LyricPlaybackRate);
        state.LyricPlaybackRate = lyricPlaybackRate;
        preventRepeatedLyricSend = state.PreventRepeatedLyricSend;
        livePlayerVolumePercent = NormalizeLivePlayerVolumePercent(state.LivePlayerVolumePercent);
        state.LivePlayerVolumePercent = livePlayerVolumePercent;
        isLivePlayerMuted = state.IsLivePlayerMuted;
        avatarPath = state.AvatarPath;
        RefreshAvatarImage();
        ForwardRules =
            new(state.ForwardRules.Select(rule => new DanmuForwardRuleViewModel(rule, OnForwardRuleChanged)));
    }

    /// <summary>
    /// 直播间状态。
    /// </summary>
    public LiveRoomTabState State { get; }

    /// <summary>
    /// 弹幕列表。
    /// </summary>
    public ObservableCollection<DanmuFeedItem> DanmuItems { get; } = [];

    /// <summary>
    /// SC 历史。
    /// </summary>
    public ObservableCollection<SuperChatItem> SuperChats { get; } = [];

    /// <summary>
    /// 歌词行。
    /// </summary>
    public ObservableCollection<LyricLineState> Lyrics { get; } = [];

    /// <summary>
    /// 转发规则。
    /// </summary>
    public ObservableCollection<DanmuForwardRuleViewModel> ForwardRules { get; }

    /// <summary>
    /// 可选转发源直播间。
    /// </summary>
    public ObservableCollection<ForwardSourceRoomOption> ForwardSourceRooms { get; } = [];

    /// <summary>
    /// 直播清晰度选项。
    /// </summary>
    public ObservableCollection<LiveQualityOption> LiveQualityOptions { get; } = [];

    /// <summary>
    /// 标签页 ID。
    /// </summary>
    public string Id => State.Id;

    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId
    {
        get => State.RoomId;
        set
        {
            if (State.RoomId == value)
            {
                return;
            }

            State.RoomId = value;
            AvatarPath = "";
            OnPropertyChanged();
            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(AvatarPlaceholderBrush));
            OnPropertyChanged(nameof(AvatarPlaceholderText));
        }
    }

    /// <summary>
    /// 直播间名称。
    /// </summary>
    public string RoomName
    {
        get => State.RoomName;
        set
        {
            if (State.RoomName == value)
            {
                return;
            }

            State.RoomName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(AvatarPlaceholderText));
        }
    }

    /// <summary>
    /// 主播 UID。
    /// </summary>
    public string OwnerUid
    {
        get => State.OwnerUid;
        set
        {
            if (State.OwnerUid == value)
            {
                return;
            }

            State.OwnerUid = value;
            AvatarPath = "";
            OnPropertyChanged();
            OnPropertyChanged(nameof(AvatarPlaceholderBrush));
        }
    }

    /// <summary>
    /// 标签标题。
    /// </summary>
    public string Header => string.IsNullOrWhiteSpace(RoomName) ? RoomId : RoomName;

    /// <summary>
    /// 账号覆盖 ID。
    /// </summary>
    public string? AccountOverrideId
    {
        get => State.AccountOverrideId;
        set
        {
            if (State.AccountOverrideId == value)
            {
                return;
            }

            State.AccountOverrideId = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty] private string inputDraft = "";

    [ObservableProperty] private string lyricInput = "";

    [ObservableProperty] private string lyricTitle = "";

    [ObservableProperty] private double lyricPlaybackRate = DEFAULT_LYRIC_PLAYBACK_RATE;

    [ObservableProperty] private bool preventRepeatedLyricSend = true;

    [ObservableProperty] private string connectionStatus = "未连接";

    [ObservableProperty] private bool isListening;

    [ObservableProperty] private bool isLyricAutoSending;

    [ObservableProperty] private string avatarPath = "";

    [ObservableProperty] private bool isLivePlayerVisible;

    [ObservableProperty] private string livePlayerUrl = "";

    [ObservableProperty] private double livePlayerVolumePercent;

    [ObservableProperty] private bool isLivePlayerMuted;

    [ObservableProperty] private LiveQualityOption? selectedLiveQuality;

    /// <summary>
    /// 是否未监听。
    /// </summary>
    public bool IsNotListening => !IsListening;

    /// <summary>
    /// 监听切换按钮文本。
    /// </summary>
    public string ListenToggleText => IsListening ? "停止" : "监听";

    /// <summary>
    /// 是否存在主播头像。
    /// </summary>
    public bool HasAvatar => avatarImage is not null;

    /// <summary>
    /// 是否没有主播头像。
    /// </summary>
    public bool HasNoAvatar => !HasAvatar;

    /// <summary>
    /// 是否隐藏直播播放器。
    /// </summary>
    public bool IsLivePlayerHidden => !IsLivePlayerVisible;

    /// <summary>
    /// 直播播放器播放按钮提示。
    /// </summary>
    public string LivePlayerToggleToolTip => IsLivePlayerVisible ? "停止播放" : "播放直播";

    /// <summary>
    /// 直播播放器是否未静音。
    /// </summary>
    public bool IsLivePlayerAudible => !IsLivePlayerMuted;

    /// <summary>
    /// 直播播放器音量文本。
    /// </summary>
    public string LivePlayerVolumeText => $"{Math.Round(LivePlayerVolumePercent)}%";

    /// <summary>
    /// 主播头像位图。
    /// </summary>
    public Bitmap? AvatarImage => avatarImage;

    /// <summary>
    /// 头像占位文字。
    /// </summary>
    public string AvatarPlaceholderText
    {
        get
        {
            var source = string.IsNullOrWhiteSpace(RoomName) ? RoomId : RoomName;
            var character = source.Trim().FirstOrDefault();
            return character == default ? "?" : character.ToString().ToUpperInvariant();
        }
    }

    /// <summary>
    /// 头像占位背景。
    /// </summary>
    public IBrush AvatarPlaceholderBrush => new SolidColorBrush(CreateAvatarPlaceholderColor(GetAvatarIdentity()));

    partial void OnIsListeningChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotListening));
        OnPropertyChanged(nameof(ListenToggleText));
    }

    partial void OnIsLivePlayerVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLivePlayerHidden));
        OnPropertyChanged(nameof(LivePlayerToggleToolTip));
    }

    partial void OnLivePlayerVolumePercentChanged(double value)
    {
        var normalizedValue = NormalizeLivePlayerVolumePercent(value);

        if (ShouldNormalizeLivePlayerVolumePercent(value, normalizedValue))
        {
            LivePlayerVolumePercent = normalizedValue;
            return;
        }

        State.LivePlayerVolumePercent = normalizedValue;
        OnPropertyChanged(nameof(LivePlayerVolumeText));
    }

    partial void OnIsLivePlayerMutedChanged(bool value)
    {
        State.IsLivePlayerMuted = value;
        OnPropertyChanged(nameof(IsLivePlayerAudible));
    }

    partial void OnSelectedLiveQualityChanged(LiveQualityOption? value)
    {
        if (suppressLiveQualityChange || !IsLivePlayerVisible)
        {
            return;
        }

        _ = RefreshLivePlayerAsync();
    }

    [ObservableProperty] private bool isSelected;

    [ObservableProperty] private int activeLyricIndex;

    [ObservableProperty] private LyricLineState? activeLyricLine;

    [ObservableProperty] private double lyricPlaybackPositionSeconds;

    partial void OnInputDraftChanged(string value)
    {
        State.InputDraft = value;
        OnPropertyChanged(nameof(IsInputDraftEmpty));
    }

    /// <summary>
    /// 同传输入是否为空。
    /// </summary>
    public bool IsInputDraftEmpty => string.IsNullOrWhiteSpace(InputDraft);

    /// <summary>
    /// 当前歌词标题展示文本。
    /// </summary>
    public string CurrentLyricTitleText => string.IsNullOrWhiteSpace(LyricTitle) ? "未选择歌词" : LyricTitle;

    partial void OnLyricInputChanged(string value)
    {
        State.LyricText = value;
    }

    partial void OnLyricTitleChanged(string value)
    {
        State.LyricTitle = value;
        OnPropertyChanged(nameof(CurrentLyricTitleText));
    }

    partial void OnLyricPlaybackRateChanged(double value)
    {
        var normalizedValue = NormalizeLyricPlaybackRate(value);

        if (Math.Abs(normalizedValue - value) > double.Epsilon)
        {
            LyricPlaybackRate = normalizedValue;
            return;
        }

        State.LyricPlaybackRate = normalizedValue;
        ResetLyricPlaybackClock(LyricPlaybackPositionSeconds);
    }

    partial void OnPreventRepeatedLyricSendChanged(bool value)
    {
        State.PreventRepeatedLyricSend = value;
    }

    partial void OnActiveLyricIndexChanged(int value)
    {
        RefreshActiveLyric();
    }

    partial void OnAvatarPathChanged(string value)
    {
        State.AvatarPath = value;
        RefreshAvatarImage();
        OnPropertyChanged(nameof(HasAvatar));
        OnPropertyChanged(nameof(HasNoAvatar));
    }

    /// <summary>
    /// 注入运行时服务。
    /// </summary>
    public void Configure(
        Func<LiveRoomTabViewModel, BilibiliAccount?> resolveAccount,
        Func<string, string, BilibiliAccount?> resolveAccountByRoom,
        Func<AppSettings> getSettings,
        Func<MarkSymbolGroup> getMarkSymbolGroup,
        Func<IEnumerable<MarkSymbolGroup>> getSymbolGroups,
        Func<LiveRoomTabViewModel, IEnumerable<ForwardSourceRoomOption>> getForwardSourceRooms,
        IDanmuSendService sendService,
        AvatarCacheService avatarCacheService,
        Func<Task> saveState,
        IBilibiliLiveStreamService? liveStreamService = null,
        ILivePlayerService? livePlayerService = null)
    {
        this.resolveAccount = resolveAccount;
        this.resolveAccountByRoom = resolveAccountByRoom;
        this.getSettings = getSettings;
        this.getMarkSymbolGroup = getMarkSymbolGroup;
        this.getSymbolGroups = getSymbolGroups;
        this.getForwardSourceRooms = getForwardSourceRooms;
        this.sendService = sendService;
        this.avatarCacheService = avatarCacheService;
        this.saveState = saveState;
        this.liveStreamService = liveStreamService;
        this.livePlayerService = livePlayerService;
        sendService.RecordCreated += OnSendRecordCreated;
        ApplyLyric();
    }

    /// <summary>
    /// 加载主播头像。
    /// </summary>
    public async Task<bool> LoadAvatarAsync(bool force = false)
    {
        if (avatarCacheService is null)
        {
            return false;
        }

        if (!force && HasAvatar)
        {
            OnPropertyChanged(nameof(HasAvatar));
            OnPropertyChanged(nameof(HasNoAvatar));
            return true;
        }

        var account = resolveAccount?.Invoke(this);
        var result = await avatarCacheService.GetRoomAvatarResultAsync(RoomId, OwnerUid, account?.Cookie);

        if (string.IsNullOrWhiteSpace(result.AvatarPath))
        {
            return false;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrWhiteSpace(result.OwnerUid) && OwnerUid != result.OwnerUid)
            {
                State.OwnerUid = result.OwnerUid;
                OnPropertyChanged(nameof(OwnerUid));
                OnPropertyChanged(nameof(AvatarPlaceholderBrush));
            }

            if ((string.IsNullOrWhiteSpace(RoomName) || RoomName == RoomId) &&
                !string.IsNullOrWhiteSpace(result.OwnerName))
            {
                RoomName = result.OwnerName;
            }

            AvatarPath = result.AvatarPath;
        });

        return true;
    }

    private void RefreshAvatarImage()
    {
        avatarImage?.Dispose();
        avatarImage = null;

        if (string.IsNullOrWhiteSpace(AvatarPath) || !File.Exists(AvatarPath))
        {
            OnPropertyChanged(nameof(AvatarImage));
            return;
        }

        try
        {
            avatarImage = new(AvatarPath);
        }
        catch
        {
            avatarImage = null;
        }

        OnPropertyChanged(nameof(AvatarImage));
    }

    /// <summary>
    /// 根据直播间 ID 生成稳定头像占位颜色。
    /// </summary>
    public static Color CreateAvatarPlaceholderColor(string roomId)
    {
        var hash = 0;

        foreach (var character in roomId)
        {
            hash = unchecked((hash * 31) + character);
        }

        var hue = Math.Abs(hash) % 360;
        return FromHsl(hue, 0.52, 0.46);
    }

    private string GetAvatarIdentity()
    {
        return string.IsNullOrWhiteSpace(OwnerUid) ? RoomId : OwnerUid;
    }

    private static Color FromHsl(int hue, double saturation, double lightness)
    {
        var chroma = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
        var hueSection = hue / 60.0;
        var x = chroma * (1 - Math.Abs((hueSection % 2) - 1));
        var match = lightness - (chroma / 2);
        (double Red, double Green, double Blue) color = hueSection switch
        {
            >= 0 and < 1 => (chroma, x, 0),
            >= 1 and < 2 => (x, chroma, 0),
            >= 2 and < 3 => (0, chroma, x),
            >= 3 and < 4 => (0, x, chroma),
            >= 4 and < 5 => (x, 0, chroma),
            _ => (chroma, 0, x)
        };

        return Color.FromRgb(
            ToByte(color.Red + match),
            ToByte(color.Green + match),
            ToByte(color.Blue + match));
    }

    private static byte ToByte(double value)
    {
        return (byte)Math.Clamp(Math.Round(value * 255), 0, 255);
    }

    /// <summary>
    /// 解析歌词输入。
    /// </summary>
    [RelayCommand]
    public void ApplyLyric()
    {
        StopAutoLyric(false);
        Lyrics.Clear();

        foreach (var line in lyricTimelineService.Parse(LyricInput))
        {
            Lyrics.Add(line);
        }

        RefreshLyricDurations();
        ResetLyricSession();
    }

    /// <summary>
    /// 清空当前歌词。
    /// </summary>
    [RelayCommand]
    public void ClearLyric()
    {
        StopAutoLyric(false);
        LyricTitle = "";
        LyricInput = "";
        Lyrics.Clear();
        ResetLyricSession();
    }

    /// <summary>
    /// 发送同传输入。
    /// </summary>
    [RelayCommand]
    public async Task SendDraftAsync()
    {
        if (string.IsNullOrWhiteSpace(InputDraft))
        {
            return;
        }

        var settings = getSettings?.Invoke();
        var account = resolveAccount?.Invoke(this);

        if (settings is null || sendService is null)
        {
            return;
        }

        var markGroup = getMarkSymbolGroup?.Invoke() ?? MarkSymbolService.CreateDefaultGroup(settings);
        var message = $"{markGroup.TranslateOpenMark}{InputDraft.Trim()}{markGroup.TranslateCloseMark}";
        await SendMessageAsync(message, account, settings);
        InputDraft = "";
    }

    /// <summary>
    /// 清空同传输入。
    /// </summary>
    [RelayCommand]
    public void ClearInputDraft()
    {
        InputDraft = "";
    }

    /// <summary>
    /// 手动发送当前歌词。
    /// </summary>
    [RelayCommand]
    public async Task SendCurrentLyricAsync()
    {
        if (Lyrics.Count == 0)
        {
            return;
        }

        var line = Lyrics[Math.Clamp(ActiveLyricIndex, 0, Lyrics.Count - 1)];
        await SendLyricLineAsync(line, true);
        SetLyricPlaybackPosition(GetNextLyricPosition(line));
    }

    /// <summary>
    /// 快退歌词播放位置。
    /// </summary>
    [RelayCommand]
    public void RewindLyric()
    {
        AdjustLyricPlaybackPosition(-LYRIC_SEEK_STEP_SECONDS);
    }

    /// <summary>
    /// 快进歌词播放位置。
    /// </summary>
    [RelayCommand]
    public void FastForwardLyric()
    {
        AdjustLyricPlaybackPosition(LYRIC_SEEK_STEP_SECONDS);
    }

    /// <summary>
    /// 调整歌词播放位置。
    /// </summary>
    public void AdjustLyricPlaybackPosition(double offsetSeconds)
    {
        if (!LyricTimelineService.HasTimeline(Lyrics))
        {
            return;
        }

        SetLyricPlaybackPosition(LyricPlaybackPositionSeconds + offsetSeconds);
    }

    /// <summary>
    /// 定位歌词播放位置到指定歌词行开头。
    /// </summary>
    public void SeekLyricLineStart(LyricLineState? line)
    {
        if (line is null || line.TimeSeconds < 0 || !Lyrics.Contains(line))
        {
            return;
        }

        SetLyricPlaybackPosition(line.TimeSeconds);
    }

    /// <summary>
    /// 开始或暂停歌词自动发送。
    /// </summary>
    [RelayCommand]
    public async Task ToggleAutoLyricAsync()
    {
        if (IsLyricAutoSending)
        {
            lyricTokenSource?.Cancel();
            IsLyricAutoSending = false;
            return;
        }

        await StartAutoLyricAsync();
    }

    /// <summary>
    /// 开始歌词自动发送。
    /// </summary>
    [RelayCommand]
    public async Task StartAutoLyricAsync()
    {
        if (IsLyricAutoSending)
        {
            return;
        }

        if (!LyricTimelineService.HasTimeline(Lyrics))
        {
            ApplyLyric();
        }

        if (!LyricTimelineService.HasTimeline(Lyrics))
        {
            return;
        }

        lyricTokenSource = new();
        IsLyricAutoSending = true;
        await RunAutoLyricAsync(lyricTokenSource.Token);
    }

    /// <summary>
    /// 停止歌词自动发送。
    /// </summary>
    [RelayCommand]
    public void StopAutoLyric()
    {
        StopAutoLyric(true);
    }

    private void StopAutoLyric(bool resetPosition)
    {
        lyricTokenSource?.Cancel();
        IsLyricAutoSending = false;

        if (resetPosition)
        {
            SetLyricPlaybackPosition(GetFirstTimedLyricPosition());
        }
    }

    private void RefreshLyricDurations()
    {
        var timedLines = Lyrics.Where(line => line.TimeSeconds >= 0).ToList();

        for (var index = 0; index < Lyrics.Count; index++)
        {
            Lyrics[index].DurationSeconds = 0;
        }

        for (var index = 0; index < timedLines.Count; index++)
        {
            var line = timedLines[index];
            var duration = index < timedLines.Count - 1
                ? timedLines[index + 1].TimeSeconds - line.TimeSeconds
                : DEFAULT_LAST_LYRIC_DURATION_SECONDS;
            line.DurationSeconds = Math.Max(0.1, duration);
        }
    }

    private void ResetLyricSession()
    {
        lastAutoEnteredLyricIndex = -1;

        foreach (var line in Lyrics)
        {
            line.IsSent = false;
            line.Progress = 0;
        }

        SetLyricPlaybackPosition(GetFirstTimedLyricPosition());
    }

    private void SetLyricPlaybackPosition(double positionSeconds)
    {
        if (Lyrics.Count == 0)
        {
            LyricPlaybackPositionSeconds = 0;
            ActiveLyricLine = null;
            return;
        }

        var normalizedPosition = Math.Clamp(
            positionSeconds,
            GetFirstTimedLyricPosition(),
            GetLastLyricEndPosition());
        LyricPlaybackPositionSeconds = normalizedPosition;
        ActiveLyricIndex = FindActiveLyricIndex(normalizedPosition);
        RefreshActiveLyric();
        UpdateLyricProgress(normalizedPosition);
        ResetLyricPlaybackClock(normalizedPosition);
    }

    private void ResetLyricPlaybackClock(double positionSeconds)
    {
        lyricPlaybackStartPositionSeconds = positionSeconds;
        lyricPlaybackStartedAt = DateTimeOffset.Now;
    }

    private int FindActiveLyricIndex(double positionSeconds)
    {
        var activeIndex = 0;

        for (var index = 0; index < Lyrics.Count; index++)
        {
            var line = Lyrics[index];

            if (line.TimeSeconds < 0)
            {
                continue;
            }

            if (line.TimeSeconds > positionSeconds)
            {
                break;
            }

            activeIndex = index;
        }

        return activeIndex;
    }

    private void UpdateLyricProgress(double positionSeconds)
    {
        for (var index = 0; index < Lyrics.Count; index++)
        {
            var line = Lyrics[index];

            if (index != ActiveLyricIndex || line.TimeSeconds < 0 || line.DurationSeconds <= 0)
            {
                line.Progress = 0;
                continue;
            }

            line.Progress = (positionSeconds - line.TimeSeconds) / line.DurationSeconds;
        }
    }

    private double GetFirstTimedLyricPosition()
    {
        return Lyrics.FirstOrDefault(line => line.TimeSeconds >= 0)?.TimeSeconds ?? 0;
    }

    private double GetLastLyricEndPosition()
    {
        var lastTimedLine = Lyrics.LastOrDefault(line => line.TimeSeconds >= 0);
        return lastTimedLine is null
            ? 0
            : lastTimedLine.TimeSeconds + Math.Max(0.1, lastTimedLine.DurationSeconds);
    }

    private double GetNextLyricPosition(LyricLineState line)
    {
        var index = Lyrics.IndexOf(line);

        for (var nextIndex = index + 1; nextIndex < Lyrics.Count; nextIndex++)
        {
            if (Lyrics[nextIndex].TimeSeconds >= 0)
            {
                return Lyrics[nextIndex].TimeSeconds;
            }
        }

        return GetLastLyricEndPosition();
    }

    private async Task<bool> SendLyricLineAsync(
        LyricLineState line,
        bool forceSend,
        CancellationToken cancellationToken = default)
    {
        if (!forceSend && PreventRepeatedLyricSend && line.IsSent)
        {
            return false;
        }

        var settings = getSettings?.Invoke();
        var account = resolveAccount?.Invoke(this);

        if (settings is null || sendService is null)
        {
            return false;
        }

        var markGroup = getMarkSymbolGroup?.Invoke() ?? MarkSymbolService.CreateDefaultGroup(settings);
        var message =
            LyricTimelineService.CreateMessage(markGroup.LyricOpenMark, markGroup.LyricCloseMark, line.Content);
        await SendMessageAsync(message, account, settings, cancellationToken);
        line.IsSent = true;
        return true;
    }

    private async Task SendActiveLyricIfNeededAsync(CancellationToken cancellationToken)
    {
        if (Lyrics.Count == 0 ||
            ActiveLyricIndex == lastAutoEnteredLyricIndex ||
            ActiveLyricIndex < 0 ||
            ActiveLyricIndex >= Lyrics.Count)
        {
            return;
        }

        var line = Lyrics[ActiveLyricIndex];

        if (line.TimeSeconds < 0)
        {
            return;
        }

        lastAutoEnteredLyricIndex = ActiveLyricIndex;
        await SendLyricLineAsync(line, false, cancellationToken);
    }

    /// <summary>
    /// 开始监听直播间。
    /// </summary>
    [RelayCommand]
    public void StartListening()
    {
        if (IsListening)
        {
            return;
        }

        var account = resolveAccount?.Invoke(this);

        if (account is null || string.IsNullOrWhiteSpace(account.Cookie) || string.IsNullOrWhiteSpace(RoomId))
        {
            ConnectionStatus = "缺少账号或房间";
            return;
        }

        listenTokenSource = new();
        if (!long.TryParse(RoomId, out var parsedRoomId))
        {
            ConnectionStatus = "房间号格式错误";
            return;
        }

        var socket = new BilibiliLiveWebSocket(parsedRoomId, account.Cookie);
        socket.DanmuReceived += OnDanmuReceived;
        socket.SuperChatReceived += OnSuperChatReceived;
        socket.Disconnected += (_, _) => Dispatcher.UIThread.Post(() => ConnectionStatus = "连接中断");
        socket.Recovered += (_, _) => Dispatcher.UIThread.Post(() => ConnectionStatus = "已恢复");
        socket.ErrorReceived += (_, exception) =>
            Dispatcher.UIThread.Post(() => ConnectionStatus = $"监听失败：{exception.Message}");
        IsListening = true;
        ConnectionStatus = "监听中";

        _ = Task.Run((Func<Task?>)(async () =>
        {
            try
            {
                await socket.StartAsync(listenTokenSource.Token);
            }
            finally
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsListening = false;
                    ConnectionStatus = "未连接";
                });
            }
        }));
    }

    /// <summary>
    /// 停止监听直播间。
    /// </summary>
    [RelayCommand]
    public void StopListening()
    {
        listenTokenSource?.Cancel();
        IsListening = false;
        ConnectionStatus = "未连接";
    }

    /// <summary>
    /// 切换直播间监听状态。
    /// </summary>
    [RelayCommand]
    public void ToggleListening()
    {
        if (IsListening)
        {
            StopListening();
            return;
        }

        StartListening();
    }

    /// <summary>
    /// 添加转发规则。
    /// </summary>
    [RelayCommand]
    public async Task AddForwardRuleAsync()
    {
        RefreshForwardSourceRooms();

        var ruleState = new DanmuForwardRuleState
        {
            MarkSymbolGroupId = getMarkSymbolGroup?.Invoke().Id ?? ""
        };
        var source = ForwardSourceRooms.FirstOrDefault();

        if (source is not null)
        {
            ruleState.SourceWorkspaceId = source.WorkspaceId;
            ruleState.SourceRoomStateId = source.RoomStateId;
        }

        State.ForwardRules.Add(ruleState);
        ForwardRules.Add(new(ruleState, OnForwardRuleChanged));
        await SaveStateAsync();
    }

    /// <summary>
    /// 删除转发规则。
    /// </summary>
    [RelayCommand]
    public async Task RemoveForwardRuleAsync(DanmuForwardRuleViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        StopForwardRule(rule);
        State.ForwardRules.Remove(rule.State);
        ForwardRules.Remove(rule);
        await SaveStateAsync();
    }

    /// <summary>
    /// 刷新转发源直播间选项。
    /// </summary>
    public void RefreshForwardSourceRooms()
    {
        if (getForwardSourceRooms is null)
        {
            ForwardSourceRooms.Clear();
            return;
        }

        var nextSources = getForwardSourceRooms(this).ToList();

        if (ForwardSourceRooms.Select(source => source.Key).SequenceEqual(nextSources.Select(source => source.Key)))
        {
            return;
        }

        ForwardSourceRooms.Clear();

        foreach (var source in nextSources)
        {
            ForwardSourceRooms.Add(source);
        }
    }

    /// <summary>
    /// 停止所有转发监听。
    /// </summary>
    public void StopForwarding()
    {
        foreach (var rule in ForwardRules)
        {
            StopForwardRule(rule);
        }
    }

    /// <summary>
    /// 切换直播播放器播放状态。
    /// </summary>
    [RelayCommand]
    public async Task ToggleLivePlayerAsync()
    {
        if (IsLivePlayerVisible)
        {
            StopLivePlayer();
            return;
        }

        await StartLivePlayerAsync(false);
    }

    /// <summary>
    /// 开始播放直播。
    /// </summary>
    [RelayCommand]
    public async Task PlayLiveAsync()
    {
        await StartLivePlayerAsync(false);
    }

    /// <summary>
    /// 刷新直播播放器。
    /// </summary>
    [RelayCommand]
    public async Task RefreshLivePlayerAsync()
    {
        await StartLivePlayerAsync(false);
    }

    /// <summary>
    /// 追到最新直播流。
    /// </summary>
    [RelayCommand]
    public async Task ChaseLiveAsync()
    {
        await StartLivePlayerAsync(true);
    }

    /// <summary>
    /// 刷新直播清晰度。
    /// </summary>
    [RelayCommand]
    public async Task RefreshLiveQualitiesAsync()
    {
        await RefreshLiveSourceAsync(false);
    }

    /// <summary>
    /// 停止直播播放。
    /// </summary>
    [RelayCommand]
    public void StopLivePlayer()
    {
        livePlayerService?.Revoke(livePlayerToken);
        livePlayerToken = null;
        LivePlayerUrl = "";
        IsLivePlayerVisible = false;
    }

    /// <summary>
    /// 切换直播播放器静音状态。
    /// </summary>
    [RelayCommand]
    public void ToggleLivePlayerMuted()
    {
        IsLivePlayerMuted = !IsLivePlayerMuted;
    }

    /// <summary>
    /// 将弹幕内容插入同传输入框。
    /// </summary>
    public int InsertDanmuContent(string? content, int? insertIndex = null)
    {
        if (content is null)
        {
            return InputDraft.Length;
        }

        var settings = getSettings?.Invoke() ?? new();
        var draft = InputDraft;
        var normalizedInsertIndex = insertIndex ?? draft.Length;

        if (normalizedInsertIndex < 0 || normalizedInsertIndex > draft.Length)
        {
            normalizedInsertIndex = draft.Length;
        }

        var insertText = $"{settings.DanmuInsertOpenMark}{content}{settings.DanmuInsertCloseMark}";
        InputDraft = draft.Insert(normalizedInsertIndex, insertText);

        return normalizedInsertIndex + insertText.Length;
    }

    /// <summary>
    /// 将 SC 内容插入同传输入框。
    /// </summary>
    [RelayCommand]
    public void CopySuperChat(SuperChatItem? superChat)
    {
        if (superChat is null)
        {
            return;
        }

        InsertDanmuContent(superChat.Content);
    }

    private async Task StartLivePlayerAsync(bool isChasing)
    {
        if (liveStreamService is null || livePlayerService is null)
        {
            return;
        }

        try
        {
            var account = resolveAccount?.Invoke(this);
            var source = await RefreshLiveSourceAsync(true);
            var oldToken = livePlayerToken;
            var session = await livePlayerService.CreatePlayerAsync(source, account?.Cookie);

            livePlayerService.Revoke(oldToken);
            livePlayerToken = session.Token;
            LivePlayerUrl = session.PlayerUrl;
            IsLivePlayerVisible = true;
            StartListeningWithLivePlayerIfNeeded();
        }
        catch
        {
            if (string.IsNullOrWhiteSpace(livePlayerToken))
            {
                IsLivePlayerVisible = false;
            }
        }
    }

    private void StartListeningWithLivePlayerIfNeeded()
    {
        if (IsListening || getSettings?.Invoke().AutoStartListeningWithLivePlayer != true)
        {
            return;
        }

        StartListening();
    }

    private static double NormalizeLivePlayerVolumePercent(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return MAX_LIVE_PLAYER_VOLUME_PERCENT;
        }

        return Math.Clamp(value, MIN_LIVE_PLAYER_VOLUME_PERCENT, MAX_LIVE_PLAYER_VOLUME_PERCENT);
    }

    private static bool ShouldNormalizeLivePlayerVolumePercent(double value, double normalizedValue)
    {
        return double.IsNaN(value) ||
               double.IsInfinity(value) ||
               Math.Abs(normalizedValue - value) > double.Epsilon;
    }

    private async Task<LiveStreamPlaySource> RefreshLiveSourceAsync(bool throwOnError)
    {
        if (liveStreamService is null)
        {
            throw new InvalidOperationException("直播流服务未初始化");
        }

        try
        {
            var account = resolveAccount?.Invoke(this);
            var settings = getSettings?.Invoke();
            var source = await liveStreamService.GetPlayableSourceAsync(
                RoomId,
                SelectedLiveQuality?.Quality ?? 0,
                account?.Cookie,
                settings is null ? null : TimeSpan.FromSeconds(settings.TimeoutSeconds));

            RefreshLiveQualityOptions(source.QualityOptions, source.CurrentQuality);

            return source;
        }
        catch when (!throwOnError)
        {
            return new();
        }
    }

    private void RefreshLiveQualityOptions(IEnumerable<LiveQualityOption> qualityOptions, int currentQuality)
    {
        suppressLiveQualityChange = true;
        LiveQualityOptions.Clear();

        foreach (var option in qualityOptions)
        {
            LiveQualityOptions.Add(option);
        }

        SelectedLiveQuality = LiveQualityOptions.FirstOrDefault(option => option.Quality == currentQuality) ??
                              LiveQualityOptions.FirstOrDefault();
        suppressLiveQualityChange = false;
    }

    /// <summary>
    /// 加载歌词库条目。
    /// </summary>
    public void LoadLyric(LyricLibraryItem item)
    {
        LyricTitle = item.Title;
        LyricInput = string.IsNullOrWhiteSpace(item.TranslatedLyricText) ? item.LyricText : item.TranslatedLyricText;
        ApplyLyric();
    }

    /// <summary>
    /// 计算歌词自动发送等待时间。
    /// </summary>
    public static TimeSpan CalculateLyricPlaybackWait(TimeSpan due, TimeSpan elapsed, double playbackRate)
    {
        var normalizedRate = NormalizeLyricPlaybackRate(playbackRate);
        var scaledDue = TimeSpan.FromTicks((long)Math.Round(due.Ticks / normalizedRate));
        var wait = scaledDue - elapsed;
        return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
    }

    private static double NormalizeLyricPlaybackRate(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            return DEFAULT_LYRIC_PLAYBACK_RATE;
        }

        return Math.Clamp(value, MIN_LYRIC_PLAYBACK_RATE, MAX_LYRIC_PLAYBACK_RATE);
    }

    private void OnForwardRuleChanged(DanmuForwardRuleViewModel rule)
    {
        _ = HandleForwardRuleChangedAsync(rule);
    }

    private async Task HandleForwardRuleChangedAsync(DanmuForwardRuleViewModel rule)
    {
        StartForwardRule(rule);
        await SaveStateAsync();
    }

    /// <summary>
    /// 重启已启用的转发规则。
    /// </summary>
    public void RestartForwarding()
    {
        foreach (var rule in ForwardRules.Where(rule => rule.IsEnabled))
        {
            StartForwardRule(rule);
        }
    }

    private void StartForwardRule(DanmuForwardRuleViewModel rule)
    {
        StopForwardRule(rule);

        if (!rule.IsEnabled)
        {
            SetForwardRuleStatus(rule, "未启用");
            return;
        }

        RefreshForwardSourceRooms();
        var source = ForwardSourceRooms.FirstOrDefault(item => item.Key == rule.SourceRoomKey);

        if (source is null)
        {
            SetForwardRuleStatus(rule, "请选择源直播间");
            return;
        }

        var error = forwardService.Validate(rule.State);

        if (!string.IsNullOrWhiteSpace(error))
        {
            SetForwardRuleStatus(rule, $"正则错误：{error}");
            return;
        }

        var sourceAccount = resolveAccountByRoom?.Invoke(source.WorkspaceId, source.RoomStateId);
        var targetAccount = resolveAccount?.Invoke(this);
        var settings = getSettings?.Invoke();

        if (sourceAccount is null || string.IsNullOrWhiteSpace(sourceAccount.Cookie))
        {
            SetForwardRuleStatus(rule, "源直播间缺少账号");
            return;
        }

        if (targetAccount is null || string.IsNullOrWhiteSpace(targetAccount.Cookie) || settings is null)
        {
            SetForwardRuleStatus(rule, "目标直播间缺少账号");
            return;
        }

        if (!long.TryParse(source.RoomId, out var sourceRoomId))
        {
            SetForwardRuleStatus(rule, "源房间号格式错误");
            return;
        }

        var tokenSource = new CancellationTokenSource();
        forwardTokenSources[rule.Id] = tokenSource;
        SetForwardRuleStatus(rule, "监听中");

        _ = Task.Run(async () =>
        {
            var socket = new BilibiliLiveWebSocket(sourceRoomId, sourceAccount.Cookie);
            socket.DanmuReceived += (_, message) =>
                _ = ForwardDanmuAsync(rule, message, targetAccount, settings, tokenSource.Token);
            socket.ErrorReceived += (_, exception) => SetForwardRuleStatus(rule, $"监听失败：{exception.Message}");
            socket.Disconnected += (_, _) => SetForwardRuleStatus(rule, "连接中断");
            socket.Recovered += (_, _) => SetForwardRuleStatus(rule, "监听中");

            try
            {
                await socket.StartAsync(tokenSource.Token);
            }
            catch (OperationCanceledException) when (tokenSource.IsCancellationRequested)
            {
            }
            finally
            {
                if (forwardTokenSources.TryGetValue(rule.Id, out var currentTokenSource) &&
                    currentTokenSource == tokenSource)
                {
                    forwardTokenSources.Remove(rule.Id);
                }

                if (!tokenSource.IsCancellationRequested && rule.IsEnabled)
                {
                    SetForwardRuleStatus(rule, "已停止");
                }

                tokenSource.Dispose();
            }
        });
    }

    private async Task ForwardDanmuAsync(
        DanmuForwardRuleViewModel rule,
        BilibiliDanmuMessage message,
        BilibiliAccount targetAccount,
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            if (!forwardService.IsMatch(message, rule.State))
            {
                return;
            }

            var markGroup = getMarkSymbolGroup?.Invoke() ?? MarkSymbolService.CreateDefaultGroup(settings);
            var messageText = forwardService.CreateForwardMessage(
                message,
                rule.State,
                markGroup,
                getSymbolGroups?.Invoke() ?? [markGroup]);
            await SendMessageAsync(messageText, targetAccount, settings, cancellationToken);
            SetForwardRuleStatus(rule, $"已转发 {DateTime.Now:HH:mm:ss}");
        }
        catch (Exception exception)
        {
            SetForwardRuleStatus(rule, $"转发失败：{exception.Message}");
        }
    }

    private void StopForwardRule(DanmuForwardRuleViewModel rule)
    {
        if (forwardTokenSources.Remove(rule.Id, out var tokenSource))
        {
            tokenSource.Cancel();
        }

        SetForwardRuleStatus(rule, rule.IsEnabled ? "已停止" : "未启用");
    }

    private void SetForwardRuleStatus(DanmuForwardRuleViewModel rule, string status)
    {
        Dispatcher.UIThread.Post(() => rule.StatusText = status);
    }

    private Task SaveStateAsync()
    {
        return saveState?.Invoke() ?? Task.CompletedTask;
    }

    private async Task SendMessageAsync(
        string message,
        BilibiliAccount? account,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (sendService is null)
        {
            return;
        }

        foreach (var part in sendService.SplitMessage(message, 30))
        {
            await sendService.SendAsync(RoomId, part, account, settings, cancellationToken);
        }
    }

    private async Task RunAutoLyricAsync(CancellationToken cancellationToken)
    {
        try
        {
            ResetLyricPlaybackClock(LyricPlaybackPositionSeconds);

            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = DateTimeOffset.Now - lyricPlaybackStartedAt;
                var nextPosition = lyricPlaybackStartPositionSeconds +
                                   (elapsed.TotalSeconds * NormalizeLyricPlaybackRate(LyricPlaybackRate));

                if (nextPosition >= GetLastLyricEndPosition())
                {
                    break;
                }

                SetLyricPlaybackPosition(nextPosition);
                await SendActiveLyricIfNeededAsync(cancellationToken);
                await Task.Delay(LYRIC_PLAYBACK_TICK_MS, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            IsLyricAutoSending = false;
        }
    }

    private void RefreshActiveLyric()
    {
        for (var index = 0; index < Lyrics.Count; index++)
        {
            Lyrics[index].IsActive = index == ActiveLyricIndex;
        }

        ActiveLyricLine = ActiveLyricIndex >= 0 && ActiveLyricIndex < Lyrics.Count
            ? Lyrics[ActiveLyricIndex]
            : null;
    }

    private void OnDanmuReceived(object? sender, BilibiliDanmuMessage message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DanmuItems.Insert(0, new()
            {
                Time = DateTimeOffset.Now,
                UserName = message.UserName,
                Content = message.Content
            });
            Trim(DanmuItems, 300);
        });
    }

    private void OnSuperChatReceived(object? sender, BilibiliSuperChatMessage message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SuperChats.Insert(0, new()
            {
                Time = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp),
                UserName = message.UserName,
                PriceText = message.PriceText,
                Content = message.Content
            });
            Trim(SuperChats, 100);
        });
    }

    private void OnSendRecordCreated(object? sender, DanmuFeedItem item)
    {
        if (item.Status == "已发送")
        {
            return;
        }

        if (item.UserName != RoomId)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            DanmuItems.Insert(0, item);
            Trim(DanmuItems, 300);
        });
    }

    private static void Trim<T>(ObservableCollection<T> collection, int maxCount)
    {
        while (collection.Count > maxCount)
        {
            collection.RemoveAt(collection.Count - 1);
        }
    }
}
