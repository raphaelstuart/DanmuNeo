using System.Collections.ObjectModel;
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
    private CancellationTokenSource? listenTokenSource;
    private CancellationTokenSource? lyricTokenSource;
    private readonly Dictionary<string, CancellationTokenSource> forwardTokenSources = [];
    private Func<LiveRoomTabViewModel, BilibiliAccount?>? resolveAccount;
    private Func<string, string, BilibiliAccount?>? resolveAccountByRoom;
    private Func<AppSettings>? getSettings;
    private Func<MarkSymbolGroup>? getMarkSymbolGroup;
    private Func<LiveRoomTabViewModel, IEnumerable<ForwardSourceRoomOption>>? getForwardSourceRooms;
    private Func<Task>? saveState;
    private DanmuSendService? sendService;
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
        ForwardRules = new(state.ForwardRules.Select(rule => new DanmuForwardRuleViewModel(rule, OnForwardRuleChanged)));
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
            OnPropertyChanged();
            OnPropertyChanged(nameof(Header));
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

    [ObservableProperty]
    private string inputDraft = "";

    [ObservableProperty]
    private string lyricInput = "";

    [ObservableProperty]
    private string lyricTitle = "";

    [ObservableProperty]
    private string connectionStatus = "未连接";

    [ObservableProperty]
    private bool isListening;

    [ObservableProperty]
    private bool isLyricAutoSending;

    /// <summary>
    /// 是否未监听。
    /// </summary>
    public bool IsNotListening => !IsListening;

    /// <summary>
    /// 监听切换按钮文本。
    /// </summary>
    public string ListenToggleText => IsListening ? "停止" : "监听";

    partial void OnIsListeningChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotListening));
        OnPropertyChanged(nameof(ListenToggleText));
    }

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private int activeLyricIndex;

    partial void OnInputDraftChanged(string value)
    {
        State.InputDraft = value;
        OnPropertyChanged(nameof(IsInputDraftEmpty));
    }

    /// <summary>
    /// 同传输入是否为空。
    /// </summary>
    public bool IsInputDraftEmpty => string.IsNullOrWhiteSpace(InputDraft);

    partial void OnLyricInputChanged(string value)
    {
        State.LyricText = value;
    }

    partial void OnLyricTitleChanged(string value)
    {
        State.LyricTitle = value;
    }

    /// <summary>
    /// 注入运行时服务。
    /// </summary>
    public void Configure(
        Func<LiveRoomTabViewModel, BilibiliAccount?> resolveAccount,
        Func<string, string, BilibiliAccount?> resolveAccountByRoom,
        Func<AppSettings> getSettings,
        Func<MarkSymbolGroup> getMarkSymbolGroup,
        Func<LiveRoomTabViewModel, IEnumerable<ForwardSourceRoomOption>> getForwardSourceRooms,
        DanmuSendService sendService,
        Func<Task> saveState)
    {
        this.resolveAccount = resolveAccount;
        this.resolveAccountByRoom = resolveAccountByRoom;
        this.getSettings = getSettings;
        this.getMarkSymbolGroup = getMarkSymbolGroup;
        this.getForwardSourceRooms = getForwardSourceRooms;
        this.sendService = sendService;
        this.saveState = saveState;
        sendService.RecordCreated += OnSendRecordCreated;
        ApplyLyric();
    }

    /// <summary>
    /// 解析歌词输入。
    /// </summary>
    [RelayCommand]
    public void ApplyLyric()
    {
        Lyrics.Clear();

        foreach (var line in lyricTimelineService.Parse(LyricInput))
        {
            Lyrics.Add(line);
        }

        ActiveLyricIndex = 0;
        RefreshActiveLyric();
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
    /// 手动发送当前歌词。
    /// </summary>
    [RelayCommand]
    public async Task SendCurrentLyricAsync()
    {
        if (Lyrics.Count == 0)
        {
            return;
        }

        var settings = getSettings?.Invoke();
        var account = resolveAccount?.Invoke(this);

        if (settings is null || sendService is null)
        {
            return;
        }

        var line = Lyrics[Math.Clamp((int)ActiveLyricIndex, 0, Lyrics.Count - 1)];
        var markGroup = getMarkSymbolGroup?.Invoke() ?? MarkSymbolService.CreateDefaultGroup(settings);
        var message = LyricTimelineService.CreateMessage(markGroup.LyricOpenMark, markGroup.LyricCloseMark, line.Content);
        await SendMessageAsync(message, account, settings);
        ActiveLyricIndex = Math.Min(ActiveLyricIndex + 1, Math.Max(0, Lyrics.Count - 1));
        RefreshActiveLyric();
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

        ApplyLyric();

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
        lyricTokenSource?.Cancel();
        IsLyricAutoSending = false;
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
        socket.ErrorReceived += (_, exception) => Dispatcher.UIThread.Post(() => ConnectionStatus = $"监听失败：{exception.Message}");
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

        var ruleState = new DanmuForwardRuleState();
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
        ForwardSourceRooms.Clear();

        if (getForwardSourceRooms is null)
        {
            return;
        }

        foreach (var source in getForwardSourceRooms(this))
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
    /// 将 SC 内容复制到同传输入框。
    /// </summary>
    [RelayCommand]
    public void CopySuperChat(SuperChatItem? superChat)
    {
        if (superChat is null)
        {
            return;
        }

        InputDraft = superChat.Content;
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
            socket.DanmuReceived += (_, message) => _ = ForwardDanmuAsync(rule, message, targetAccount, settings, tokenSource.Token);
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
                if (forwardTokenSources.TryGetValue(rule.Id, out var currentTokenSource) && currentTokenSource == tokenSource)
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

            await SendMessageAsync(message.Content, targetAccount, settings, cancellationToken);
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
            var start = DateTimeOffset.Now;
            var firstTime = Lyrics.FirstOrDefault(line => line.TimeSeconds >= 0)?.TimeSeconds ?? 0;

            for (var index = 0; index < Lyrics.Count; index++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var line = Lyrics[index];

                if (line.TimeSeconds < 0)
                {
                    continue;
                }

                var due = TimeSpan.FromSeconds(Math.Max(0, line.TimeSeconds - firstTime));
                var wait = due - (DateTimeOffset.Now - start);

                if (wait > TimeSpan.Zero)
                {
                    await Task.Delay(wait, cancellationToken);
                }

                ActiveLyricIndex = index;
                RefreshActiveLyric();
                await SendCurrentLyricAsync();
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
