using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yohuke.DanmuNeo.Models.BrowserLogin;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.ViewModels;

/// <summary>
/// 主窗口视图模型。
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AppStateService stateService;
    private readonly WorkspaceShareService workspaceShareService = new();
    private readonly AccountSelectionService accountSelectionService = new();
    private readonly DanmuSendService danmuSendService = new();
    private readonly MarkSymbolService markSymbolService = new();
    private readonly LyricLibraryService lyricLibraryService = new();
    private readonly MusicLyricImportService musicLyricImportService = new();
    private readonly AppDirectoryService appDirectoryService;
    private readonly AppBackupService appBackupService;
    private readonly AvatarCacheService avatarCacheService;
    private WorkspaceViewModel? addDialogWorkspace;
    private WorkspaceViewModel? deleteDialogWorkspace;
    private LiveRoomTabViewModel? deleteDialogRoom;
    private string addDialogKind = "";
    private string deleteDialogKind = "";

    /// <summary>
    /// 初始化主窗口视图模型。
    /// </summary>
    public MainWindowViewModel()
        : this(new AppStateService())
    {
    }

    /// <summary>
    /// 初始化主窗口视图模型。
    /// </summary>
    public MainWindowViewModel(AppStateService stateService)
    {
        this.stateService = stateService;
        appDirectoryService = new(stateService.ConfigDirectory);
        appDirectoryService.EnsureDirectories();
        appBackupService = new(appDirectoryService);
        avatarCacheService = new(appDirectoryService);
        State = stateService.Load();
        Settings = State.Settings;
        Accounts = new(State.Accounts);
        SymbolGroups = new(Settings.MarkGroups.OrderBy(group => group.SortOrder));
        LyricLibrary = new(State.LyricLibrary);
        FilteredLyricLibrary = new(lyricLibraryService.Search(State.LyricLibrary, ""));
        MusicLyricSearchResults = [];
        Workspaces = new(State.Workspaces.OrderBy(workspace => workspace.SortOrder).Select(CreateWorkspaceViewModel));
        selectedWorkspace = Workspaces.FirstOrDefault(workspace => workspace.Id == State.SelectedWorkspaceId) ??
                            Workspaces.FirstOrDefault();
        RefreshThemeModeOptions();
        RefreshSettingsSectionOptions();
        RefreshCacheSize();
        RefreshTreeSelection();
        RefreshForwardSourceRooms();
        RestartForwarding();
    }

    /// <summary>
    /// 应用状态。
    /// </summary>
    public AppState State { get; }

    /// <summary>
    /// 全局设置。
    /// </summary>
    public AppSettings Settings { get; }

    /// <summary>
    /// 工作区列表。
    /// </summary>
    public ObservableCollection<WorkspaceViewModel> Workspaces { get; }

    /// <summary>
    /// 账号列表。
    /// </summary>
    public ObservableCollection<BilibiliAccount> Accounts { get; }

    /// <summary>
    /// 符号组列表。
    /// </summary>
    public ObservableCollection<MarkSymbolGroup> SymbolGroups { get; }

    /// <summary>
    /// 歌词库。
    /// </summary>
    public ObservableCollection<LyricLibraryItem> LyricLibrary { get; }

    /// <summary>
    /// 过滤后的歌词库。
    /// </summary>
    public ObservableCollection<LyricLibraryItem> FilteredLyricLibrary { get; }

    /// <summary>
    /// 音乐 API 歌词搜索结果。
    /// </summary>
    public ObservableCollection<MusicLyricSearchResult> MusicLyricSearchResults { get; }

    /// <summary>
    /// 主题选项。
    /// </summary>
    public IReadOnlyList<ThemeModeOption> ThemeModeOptions { get; } =
    [
        new(AppThemeMode.System, "系统"),
        new(AppThemeMode.Light, "浅色"),
        new(AppThemeMode.Dark, "深色")
    ];

    /// <summary>
    /// 设置页分区选项。
    /// </summary>
    public IReadOnlyList<SettingsSectionOption> SettingsSectionOptions { get; } =
    [
        new("global", "全局设置", "外观与发送"),
        new("account", "账号登录", "B 站与音乐账号绑定"),
        new("lyricLibrary", "歌词库", "本地与音乐 API 导入"),
        new("marks", "标签组", "同传与歌词开闭标记"),
        new("workspace", "工作区设置", "名称、账号覆盖与分享"),
        new("misc", "杂项", "版本、目录与备份")
    ];

    /// <summary>
    /// 当前配色方案。
    /// </summary>
    public ThemeModeOption? SelectedThemeModeOption
    {
        get => ThemeModeOptions.FirstOrDefault(option => option.Value == Settings.ThemeMode);
        set
        {
            if (value is null || Settings.ThemeMode == value.Value)
            {
                return;
            }

            Settings.ThemeMode = value.Value;
            AppThemeService.Apply(value.Value);
            RefreshThemeModeOptions();
            OnPropertyChanged();
            _ = SaveAsync();
        }
    }

    /// <summary>
    /// 当前设置页分区。
    /// </summary>
    public SettingsSectionOption? SelectedSettingsSection
    {
        get => SettingsSectionOptions.FirstOrDefault(option => option.Key == selectedSettingsSectionKey);
        set
        {
            if (value is null || selectedSettingsSectionKey == value.Key)
            {
                return;
            }

            selectedSettingsSectionKey = value.Key;
            RefreshSettingsSectionOptions();
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGlobalSettingsSelected));
            OnPropertyChanged(nameof(IsAccountSettingsSelected));
            OnPropertyChanged(nameof(IsLyricLibrarySettingsSelected));
            OnPropertyChanged(nameof(IsMarkSettingsSelected));
            OnPropertyChanged(nameof(IsWorkspaceSettingsSelected));
            OnPropertyChanged(nameof(IsMiscSettingsSelected));
        }
    }

    /// <summary>
    /// 当前工作区符号组。
    /// </summary>
    public MarkSymbolGroup? SelectedWorkspaceMarkGroup
    {
        get
        {
            if (SelectedWorkspace is null)
            {
                return null;
            }

            return SymbolGroups.FirstOrDefault(group => group.Id == SelectedWorkspace.SelectedMarkGroupId) ??
                   SymbolGroups.FirstOrDefault();
        }
        set
        {
            if (SelectedWorkspace is null || value is null)
            {
                return;
            }

            SelectedWorkspace.SelectedMarkGroupId = value.Id;
            OnPropertyChanged();
            _ = SaveAsync();
        }
    }

    [ObservableProperty] private WorkspaceViewModel? selectedWorkspace;

    private string selectedSettingsSectionKey = "global";

    [ObservableProperty] private string lyricLibrarySearchText = "";

    [ObservableProperty] private string musicLyricSearchText = "";

    [ObservableProperty] private string selectedMusicLyricSource = "wy";

    [ObservableProperty] private bool isMusicLyricSearching;

    [ObservableProperty] private bool isSettingsOpen;

    [ObservableProperty] private bool isAddDialogOpen;

    [ObservableProperty] private bool isConfirmDeleteDialogOpen;

    [ObservableProperty] private bool isLyricLibraryPickerOpen;

    [ObservableProperty] private string addDialogTitle = "";

    [ObservableProperty] private string addDialogDescription = "";

    [ObservableProperty] private string addDialogPrimaryText = "";

    [ObservableProperty] private string deleteDialogTitle = "";

    [ObservableProperty] private string deleteDialogDescription = "";

    [ObservableProperty] private string deleteDialogPrimaryText = "删除";

    [ObservableProperty] private string newWorkspaceName = "";

    [ObservableProperty] private string newRoomId = "";

    [ObservableProperty] private string newRoomName = "";

    [ObservableProperty] private string newRoomOwnerUid = "";

    [ObservableProperty] private string statusMessage = "";

    [ObservableProperty] private string cacheSizeText = "0 B";

    [ObservableProperty] private string lastBackupPath = "";

    /// <summary>
    /// 窗口标题。
    /// </summary>
    public string WindowTitle => SelectedWorkspace is null
        ? "Yohuke.DanmuNeo"
        : $"Yohuke.DanmuNeo · {SelectedWorkspace.Name}";

    /// <summary>
    /// 是否正在显示全局设置。
    /// </summary>
    public bool IsGlobalSettingsSelected => selectedSettingsSectionKey == "global";

    /// <summary>
    /// 是否正在显示账号登录设置。
    /// </summary>
    public bool IsAccountSettingsSelected => selectedSettingsSectionKey == "account";

    /// <summary>
    /// 是否正在显示歌词库设置。
    /// </summary>
    public bool IsLyricLibrarySettingsSelected => selectedSettingsSectionKey == "lyricLibrary";

    /// <summary>
    /// 是否正在显示标签组设置。
    /// </summary>
    public bool IsMarkSettingsSelected => selectedSettingsSectionKey == "marks";

    /// <summary>
    /// 是否正在显示工作区设置。
    /// </summary>
    public bool IsWorkspaceSettingsSelected => selectedSettingsSectionKey == "workspace";

    /// <summary>
    /// 是否正在显示杂项设置。
    /// </summary>
    public bool IsMiscSettingsSelected => selectedSettingsSectionKey == "misc";

    /// <summary>
    /// 当前版本。
    /// </summary>
    public string AppVersion => typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString(3) ?? "开发版";

    /// <summary>
    /// 配置目录。
    /// </summary>
    public string ConfigDirectory => appDirectoryService.ConfigDirectory;

    /// <summary>
    /// 缓存目录。
    /// </summary>
    public string CacheDirectory => appDirectoryService.CacheDirectory;

    /// <summary>
    /// 是否存在最近导出的备份路径。
    /// </summary>
    public bool HasLastBackupPath => !string.IsNullOrWhiteSpace(LastBackupPath);

    /// <summary>
    /// QQ 音乐绑定状态。
    /// </summary>
    public string QQMusicCookieStatus => Settings.QQMusicCookieStatus;

    /// <summary>
    /// QQ 音乐 Cookie 摘要。
    /// </summary>
    public string QQMusicCookieSummary => Settings.QQMusicCookieSummary;

    /// <summary>
    /// 新增对话框是否正在创建工作区。
    /// </summary>
    public bool IsAddingWorkspace => addDialogKind == "workspace";

    /// <summary>
    /// 新增对话框是否正在创建直播间。
    /// </summary>
    public bool IsAddingRoom => addDialogKind == "room";

    /// <summary>
    /// 是否选择网易云歌词来源。
    /// </summary>
    public bool IsNetEaseMusicLyricSource => SelectedMusicLyricSource == "wy";

    /// <summary>
    /// 是否选择 QQ 音乐歌词来源。
    /// </summary>
    public bool IsQQMusicLyricSource => SelectedMusicLyricSource == "qq";

    partial void OnSelectedWorkspaceChanged(WorkspaceViewModel? value)
    {
        State.SelectedWorkspaceId = value?.Id;
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(SelectedWorkspaceMarkGroup));
        RefreshTreeSelection();
    }

    partial void OnLyricLibrarySearchTextChanged(string value)
    {
        RefreshFilteredLyricLibrary();
    }

    partial void OnSelectedMusicLyricSourceChanged(string value)
    {
        OnPropertyChanged(nameof(IsNetEaseMusicLyricSource));
        OnPropertyChanged(nameof(IsQQMusicLyricSource));
    }

    partial void OnLastBackupPathChanged(string value)
    {
        OnPropertyChanged(nameof(HasLastBackupPath));
    }

    /// <summary>
    /// 新建工作区。
    /// </summary>
    [RelayCommand]
    public async Task AddWorkspaceAsync()
    {
        var name = string.IsNullOrWhiteSpace(NewWorkspaceName)
            ? $"工作区 {Workspaces.Count + 1}"
            : NewWorkspaceName.Trim();
        var state = new WorkspaceState
        {
            Name = name
        };
        State.Workspaces.Add(state);
        var workspace = CreateWorkspaceViewModel(state);
        Workspaces.Add(workspace);
        SelectedWorkspace = workspace;
        NewWorkspaceName = "";
        RefreshWorkspaceSortOrder();
        await SaveAsync();
        RefreshTreeSelection();
    }

    /// <summary>
    /// 选择工作区。
    /// </summary>
    [RelayCommand]
    public void SelectWorkspace(WorkspaceViewModel? workspace)
    {
        if (workspace is null)
        {
            return;
        }

        SelectedWorkspace = workspace;
        workspace.RefreshRoomSelection();
        RefreshTreeSelection();
    }

    /// <summary>
    /// 选择直播间。
    /// </summary>
    [RelayCommand]
    public void SelectRoom(LiveRoomTabViewModel? room)
    {
        if (room is null)
        {
            return;
        }

        var workspace = Workspaces.FirstOrDefault(item => item.Rooms.Contains(room));

        if (workspace is null)
        {
            return;
        }

        SelectedWorkspace = workspace;
        workspace.SelectedRoom = room;
        workspace.IsExpanded = true;
        RefreshTreeSelection();
    }

    /// <summary>
    /// 切换工作区展开状态。
    /// </summary>
    [RelayCommand]
    public void ToggleWorkspaceExpanded(WorkspaceViewModel? workspace)
    {
        if (workspace is null)
        {
            return;
        }

        workspace.IsExpanded = !workspace.IsExpanded;
    }

    /// <summary>
    /// 打开新增工作区对话框。
    /// </summary>
    [RelayCommand]
    public void OpenAddWorkspaceDialog()
    {
        addDialogKind = "workspace";
        addDialogWorkspace = null;
        NewWorkspaceName = "";
        NewRoomName = "";
        NewRoomId = "";
        NewRoomOwnerUid = "";
        AddDialogTitle = "新建工作区";
        AddDialogDescription = "给新的同传工作空间起一个容易识别的名称。";
        AddDialogPrimaryText = "确定";
        OnPropertyChanged(nameof(IsAddingWorkspace));
        OnPropertyChanged(nameof(IsAddingRoom));
        IsAddDialogOpen = true;
    }

    /// <summary>
    /// 打开新增直播间对话框。
    /// </summary>
    [RelayCommand]
    public void OpenAddRoomDialog(WorkspaceViewModel? workspace)
    {
        var targetWorkspace = workspace ?? SelectedWorkspace;

        if (targetWorkspace is null)
        {
            return;
        }

        SelectedWorkspace = targetWorkspace;
        addDialogKind = "room";
        addDialogWorkspace = targetWorkspace;
        NewWorkspaceName = "";
        NewRoomName = "";
        NewRoomId = "";
        NewRoomOwnerUid = "";
        AddDialogTitle = $"添加直播间到 {targetWorkspace.Name}";
        AddDialogDescription = "输入直播间名称和 B 站直播间 ID，创建后会自动切换到该直播间。";
        AddDialogPrimaryText = "确定";
        OnPropertyChanged(nameof(IsAddingWorkspace));
        OnPropertyChanged(nameof(IsAddingRoom));
        IsAddDialogOpen = true;
    }

    /// <summary>
    /// 关闭新增对话框。
    /// </summary>
    [RelayCommand]
    public void CloseAddDialog()
    {
        IsAddDialogOpen = false;
    }

    /// <summary>
    /// 确认新增工作区或直播间。
    /// </summary>
    [RelayCommand]
    public async Task ConfirmAddDialogAsync()
    {
        if (addDialogKind == "workspace")
        {
            await AddWorkspaceAsync();
            IsAddDialogOpen = false;
            return;
        }

        if (addDialogKind != "room" || string.IsNullOrWhiteSpace(NewRoomId))
        {
            return;
        }

        var workspace = addDialogWorkspace ?? SelectedWorkspace;

        if (workspace is null)
        {
            return;
        }

        SelectedWorkspace = workspace;
        var room = workspace.AddRoom(NewRoomId.Trim(), NewRoomName.Trim(), NewRoomOwnerUid.Trim());
        ConfigureRoom(room);
        workspace.IsExpanded = true;
        NewRoomId = "";
        NewRoomName = "";
        NewRoomOwnerUid = "";
        IsAddDialogOpen = false;
        await SaveAsync();
        RefreshTreeSelection();
    }

    /// <summary>
    /// 打开删除工作区确认框。
    /// </summary>
    [RelayCommand]
    public void OpenDeleteWorkspaceDialog(WorkspaceViewModel? workspace)
    {
        if (workspace is null || Workspaces.Count <= 1)
        {
            return;
        }

        deleteDialogKind = "workspace";
        deleteDialogWorkspace = workspace;
        deleteDialogRoom = null;
        DeleteDialogTitle = "删除工作区";
        DeleteDialogDescription = $"确定删除“{workspace.Name}”吗？该工作区下的直播间配置会一并移除。";
        DeleteDialogPrimaryText = "删除工作区";
        IsConfirmDeleteDialogOpen = true;
    }

    /// <summary>
    /// 打开删除直播间确认框。
    /// </summary>
    [RelayCommand]
    public void OpenDeleteRoomDialog(LiveRoomTabViewModel? room)
    {
        if (room is null)
        {
            return;
        }

        deleteDialogKind = "room";
        deleteDialogWorkspace = Workspaces.FirstOrDefault(item => item.Rooms.Contains(room));
        deleteDialogRoom = room;
        DeleteDialogTitle = "删除直播间";
        DeleteDialogDescription = $"确定删除直播间“{room.Header}”吗？运行期间的弹幕和 SC 历史会从当前工作区移除。";
        DeleteDialogPrimaryText = "删除直播间";
        IsConfirmDeleteDialogOpen = true;
    }

    /// <summary>
    /// 关闭删除确认框。
    /// </summary>
    [RelayCommand]
    public void CloseDeleteDialog()
    {
        IsConfirmDeleteDialogOpen = false;
        deleteDialogKind = "";
        deleteDialogWorkspace = null;
        deleteDialogRoom = null;
    }

    /// <summary>
    /// 确认删除工作区或直播间。
    /// </summary>
    [RelayCommand]
    public async Task ConfirmDeleteDialogAsync()
    {
        var workspace = deleteDialogWorkspace;
        var room = deleteDialogRoom;
        var kind = deleteDialogKind;
        CloseDeleteDialog();

        if (kind == "workspace")
        {
            await DeleteWorkspaceAsync(workspace);
            return;
        }

        if (kind == "room")
        {
            await RemoveRoomAsync(room);
        }
    }

    /// <summary>
    /// 删除当前工作区。
    /// </summary>
    [RelayCommand]
    public async Task DeleteSelectedWorkspaceAsync()
    {
        await DeleteWorkspaceAsync(SelectedWorkspace);
    }

    /// <summary>
    /// 删除指定工作区。
    /// </summary>
    [RelayCommand]
    public async Task DeleteWorkspaceAsync(WorkspaceViewModel? workspace)
    {
        if (workspace is null || Workspaces.Count <= 1)
        {
            return;
        }

        foreach (var room in workspace.Rooms)
        {
            room.StopListening();
            room.StopForwarding();
        }

        State.Workspaces.Remove(workspace.State);
        Workspaces.Remove(workspace);
        SelectedWorkspace = Workspaces.FirstOrDefault();
        RefreshWorkspaceSortOrder();
        await SaveAsync();
        RefreshTreeSelection();
        RefreshForwardSourceRooms();
    }

    /// <summary>
    /// 添加直播间 tab。
    /// </summary>
    [RelayCommand]
    public async Task AddRoomAsync()
    {
        if (SelectedWorkspace is null || string.IsNullOrWhiteSpace(NewRoomId))
        {
            return;
        }

        var room = SelectedWorkspace.AddRoom(NewRoomId.Trim(), NewRoomName.Trim(), NewRoomOwnerUid.Trim());
        ConfigureRoom(room);
        SelectedWorkspace.IsExpanded = true;
        NewRoomId = "";
        NewRoomName = "";
        NewRoomOwnerUid = "";
        await SaveAsync();
        RefreshTreeSelection();
        RefreshForwardSourceRooms();
    }

    /// <summary>
    /// 删除直播间 tab。
    /// </summary>
    [RelayCommand]
    public async Task RemoveRoomAsync(LiveRoomTabViewModel? room)
    {
        if (room is null)
        {
            return;
        }

        var workspace = Workspaces.FirstOrDefault(item => item.Rooms.Contains(room));

        if (workspace is null)
        {
            return;
        }

        room.StopListening();
        room.StopForwarding();
        workspace.RemoveRoom(room);

        if (SelectedWorkspace == workspace)
        {
            SelectedWorkspace = workspace;
        }

        await SaveAsync();
        RefreshTreeSelection();
        RefreshForwardSourceRooms();
    }

    /// <summary>
    /// 保存直播间信息改动。
    /// </summary>
    [RelayCommand]
    public async Task SaveRoomInfoAsync(LiveRoomTabViewModel? room)
    {
        if (room is null)
        {
            return;
        }

        RefreshTreeSelection();
        RefreshForwardSourceRooms();
        await SaveAsync();

        if (!await room.LoadAvatarAsync(true))
        {
            StatusMessage = "主播头像获取失败，请检查直播间 ID 或主播 UID";
        }
    }

    /// <summary>
    /// 打开设置页。
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        SelectedSettingsSection = SettingsSectionOptions.FirstOrDefault(option => option.Key == "global");
        IsSettingsOpen = true;
    }

    /// <summary>
    /// 打开同传页面内嵌歌词库选择器。
    /// </summary>
    [RelayCommand]
    public void OpenLyricLibraryPicker()
    {
        IsLyricLibraryPickerOpen = true;
    }

    /// <summary>
    /// 关闭同传页面内嵌歌词库选择器。
    /// </summary>
    [RelayCommand]
    public void CloseLyricLibraryPicker()
    {
        IsLyricLibraryPickerOpen = false;
    }

    /// <summary>
    /// 打开指定工作区设置。
    /// </summary>
    [RelayCommand]
    public void OpenWorkspaceSettings(WorkspaceViewModel? workspace)
    {
        if (workspace is not null)
        {
            SelectedWorkspace = workspace;
        }

        SelectedSettingsSection = SettingsSectionOptions.FirstOrDefault(option => option.Key == "workspace");
        IsSettingsOpen = true;
    }

    /// <summary>
    /// 选择设置页分区。
    /// </summary>
    [RelayCommand]
    public void SelectSettingsSection(SettingsSectionOption? option)
    {
        if (option is null)
        {
            return;
        }

        SelectedSettingsSection = option;
    }

    /// <summary>
    /// 选择网易云歌词来源。
    /// </summary>
    [RelayCommand]
    public void SelectNetEaseMusicLyricSource()
    {
        SelectedMusicLyricSource = "wy";
    }

    /// <summary>
    /// 选择 QQ 音乐歌词来源。
    /// </summary>
    [RelayCommand]
    public void SelectQQMusicLyricSource()
    {
        SelectedMusicLyricSource = "qq";
    }

    /// <summary>
    /// 搜索音乐 API 歌词。
    /// </summary>
    [RelayCommand]
    public async Task SearchMusicLyricsAsync()
    {
        MusicLyricSearchResults.Clear();

        if (string.IsNullOrWhiteSpace(MusicLyricSearchText))
        {
            return;
        }

        IsMusicLyricSearching = true;

        try
        {
            var results = await musicLyricImportService.SearchAsync(
                MusicLyricSearchText,
                SelectedMusicLyricSource,
                Settings.QQMusicCookie);

            foreach (var result in results)
            {
                MusicLyricSearchResults.Add(result);
            }

            StatusMessage = $"找到 {MusicLyricSearchResults.Count} 条歌词候选";
        }
        catch (Exception exception)
        {
            StatusMessage = $"音乐 API 搜索失败：{exception.Message}";
        }
        finally
        {
            IsMusicLyricSearching = false;
        }
    }

    /// <summary>
    /// 选择配色方案。
    /// </summary>
    [RelayCommand]
    public async Task SelectThemeModeAsync(ThemeModeOption? option)
    {
        if (option is null || Settings.ThemeMode == option.Value)
        {
            return;
        }

        Settings.ThemeMode = option.Value;
        AppThemeService.Apply(option.Value);
        RefreshThemeModeOptions();
        OnPropertyChanged(nameof(SelectedThemeModeOption));
        await SaveAsync();
    }

    /// <summary>
    /// 关闭设置页并保存。
    /// </summary>
    [RelayCommand]
    public async Task CloseSettingsAsync()
    {
        IsSettingsOpen = false;
        await SaveAsync();
    }

    /// <summary>
    /// 新增账号。
    /// </summary>
    [RelayCommand]
    public async Task AddAccountAsync()
    {
        var account = new BilibiliAccount
        {
            Name = $"账号 {Accounts.Count + 1}",
            IsGlobalDefault = Accounts.Count == 0
        };
        Accounts.Add(account);
        State.Accounts.Add(account);
        RefreshAccountLabels();
        await SaveAsync();
    }

    /// <summary>
    /// 应用 B 站浏览器登录结果。
    /// </summary>
    public async Task ApplyBilibiliBrowserLoginAsync(BrowserCookieLoginResult result, BilibiliAccount? account)
    {
        if (result.Platform != BrowserLoginPlatform.Bilibili || string.IsNullOrWhiteSpace(result.Cookie))
        {
            return;
        }

        var targetAccount = account;

        if (targetAccount is null)
        {
            targetAccount = new()
            {
                Name = $"B站账号 {Accounts.Count + 1}",
                IsGlobalDefault = Accounts.Count == 0
            };
            Accounts.Add(targetAccount);
            State.Accounts.Add(targetAccount);
        }

        targetAccount.Cookie = result.Cookie;
        RefreshAccountLabels();
        await SaveAsync();
        StatusMessage = result.Message;
    }

    /// <summary>
    /// 应用 QQ 音乐浏览器登录结果。
    /// </summary>
    public async Task ApplyQQMusicBrowserLoginAsync(BrowserCookieLoginResult result)
    {
        if (result.Platform != BrowserLoginPlatform.QQMusic || string.IsNullOrWhiteSpace(result.Cookie))
        {
            return;
        }

        Settings.QQMusicCookie = result.Cookie;
        OnPropertyChanged(nameof(QQMusicCookieStatus));
        OnPropertyChanged(nameof(QQMusicCookieSummary));
        await SaveAsync();
        StatusMessage = result.Message;
    }

    /// <summary>
    /// 删除账号。
    /// </summary>
    [RelayCommand]
    public async Task DeleteAccountAsync(BilibiliAccount? account)
    {
        if (account is null)
        {
            return;
        }

        Accounts.Remove(account);
        State.Accounts.Remove(account);

        foreach (var workspace in State.Workspaces)
        {
            if (workspace.AccountOverrideId == account.Id)
            {
                workspace.AccountOverrideId = null;
            }

            foreach (var room in workspace.LiveRooms)
            {
                if (room.AccountOverrideId == account.Id)
                {
                    room.AccountOverrideId = null;
                }
            }
        }

        if (Accounts.Count > 0 && Accounts.All(item => !item.IsGlobalDefault))
        {
            Accounts[0].IsGlobalDefault = true;
        }

        RefreshAccountLabels();
        await SaveAsync();
    }

    /// <summary>
    /// 设置默认账号。
    /// </summary>
    [RelayCommand]
    public async Task SetDefaultAccountAsync(BilibiliAccount? account)
    {
        if (account is null)
        {
            return;
        }

        foreach (var item in Accounts)
        {
            item.IsGlobalDefault = item == account;
        }

        RefreshAccountLabels();
        await SaveAsync();
    }

    /// <summary>
    /// 当前工作区使用全局账号。
    /// </summary>
    [RelayCommand]
    public async Task ClearWorkspaceAccountAsync()
    {
        if (SelectedWorkspace is null)
        {
            return;
        }

        SelectedWorkspace.AccountOverrideId = null;
        RefreshAccountLabels();
        await SaveAsync();
    }

    /// <summary>
    /// 当前工作区使用指定账号。
    /// </summary>
    [RelayCommand]
    public async Task SetWorkspaceAccountAsync(BilibiliAccount? account)
    {
        if (SelectedWorkspace is null || account is null)
        {
            return;
        }

        SelectedWorkspace.AccountOverrideId = account.Id;
        RefreshAccountLabels();
        await SaveAsync();
    }

    /// <summary>
    /// 上移工作区。
    /// </summary>
    [RelayCommand]
    public async Task MoveWorkspaceUpAsync(WorkspaceViewModel? workspace)
    {
        await MoveWorkspaceAsync(workspace, -1);
    }

    /// <summary>
    /// 下移工作区。
    /// </summary>
    [RelayCommand]
    public async Task MoveWorkspaceDownAsync(WorkspaceViewModel? workspace)
    {
        await MoveWorkspaceAsync(workspace, 1);
    }

    /// <summary>
    /// 上移直播间。
    /// </summary>
    [RelayCommand]
    public async Task MoveRoomUpAsync(LiveRoomTabViewModel? room)
    {
        await MoveRoomAsync(room, -1);
    }

    /// <summary>
    /// 下移直播间。
    /// </summary>
    [RelayCommand]
    public async Task MoveRoomDownAsync(LiveRoomTabViewModel? room)
    {
        await MoveRoomAsync(room, 1);
    }

    /// <summary>
    /// 新增符号组。
    /// </summary>
    [RelayCommand]
    public async Task AddSymbolGroupAsync()
    {
        var baseGroup = SelectedWorkspaceMarkGroup ??
                        SymbolGroups.FirstOrDefault() ?? MarkSymbolService.CreateDefaultGroup(Settings);
        var group = new MarkSymbolGroup
        {
            Name = $"符号组 {SymbolGroups.Count + 1}",
            TranslateOpenMark = baseGroup.TranslateOpenMark,
            TranslateCloseMark = baseGroup.TranslateCloseMark,
            LyricOpenMark = baseGroup.LyricOpenMark,
            LyricCloseMark = baseGroup.LyricCloseMark
        };
        SymbolGroups.Add(group);
        Settings.MarkGroups.Add(group);
        RefreshSymbolGroupSortOrder();

        if (SelectedWorkspace is not null)
        {
            SelectedWorkspaceMarkGroup = group;
        }

        await SaveAsync();
    }

    /// <summary>
    /// 删除符号组。
    /// </summary>
    [RelayCommand]
    public async Task DeleteSymbolGroupAsync(MarkSymbolGroup? group)
    {
        if (group is null || SymbolGroups.Count <= 1)
        {
            return;
        }

        SymbolGroups.Remove(group);
        Settings.MarkGroups.Remove(group);
        var fallback = SymbolGroups[0];
        RefreshSymbolGroupSortOrder();

        foreach (var workspace in Workspaces.Where(workspace => workspace.SelectedMarkGroupId == group.Id))
        {
            workspace.SelectedMarkGroupId = fallback.Id;
        }

        OnPropertyChanged(nameof(SelectedWorkspaceMarkGroup));
        await SaveAsync();
    }

    /// <summary>
    /// 切换符号组展开状态。
    /// </summary>
    [RelayCommand]
    public async Task ToggleSymbolGroupExpandedAsync(MarkSymbolGroup? group)
    {
        if (group is null)
        {
            return;
        }

        group.IsExpanded = !group.IsExpanded;
        await SaveAsync();
    }

    /// <summary>
    /// 上移符号组。
    /// </summary>
    [RelayCommand]
    public async Task MoveSymbolGroupUpAsync(MarkSymbolGroup? group)
    {
        await MoveSymbolGroupAsync(group, -1);
    }

    /// <summary>
    /// 下移符号组。
    /// </summary>
    [RelayCommand]
    public async Task MoveSymbolGroupDownAsync(MarkSymbolGroup? group)
    {
        await MoveSymbolGroupAsync(group, 1);
    }

    /// <summary>
    /// 将当前直播间歌词保存到歌词库。
    /// </summary>
    [RelayCommand]
    public async Task SaveCurrentLyricToLibraryAsync()
    {
        var room = SelectedWorkspace?.SelectedRoom;

        if (room is null || string.IsNullOrWhiteSpace(room.LyricInput))
        {
            return;
        }

        var title = string.IsNullOrWhiteSpace(room.LyricTitle) ? room.Header : room.LyricTitle.Trim();
        try
        {
            UpsertLyricLibraryItem(new()
            {
                Title = title,
                Source = "local",
                Tags = title,
                LyricText = room.LyricInput
            });
            await SaveAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = $"歌词保存失败：{exception.Message}";
        }
    }

    /// <summary>
    /// 从本地文件导入歌词到当前直播间和歌词库。
    /// </summary>
    public async Task<bool> ImportLyricFileAsync(string filePath)
    {
        try
        {
            var item = await lyricLibraryService.CreateFromLocalFileAsync(filePath);
            var savedItem = UpsertLyricLibraryItem(item);
            UseLyricLibraryItem(savedItem);
            await SaveAsync();
            return true;
        }
        catch (Exception exception)
        {
            StatusMessage = $"歌词导入失败：{exception.Message}";
            return false;
        }
    }

    /// <summary>
    /// 从本地文件导入歌词到歌词库。
    /// </summary>
    public async Task<bool> ImportLyricFileToLibraryAsync(string filePath)
    {
        try
        {
            var item = await lyricLibraryService.CreateFromLocalFileAsync(filePath);
            UpsertLyricLibraryItem(item);
            await SaveAsync();
            return true;
        }
        catch (Exception exception)
        {
            StatusMessage = $"歌词导入失败：{exception.Message}";
            return false;
        }
    }

    /// <summary>
    /// 从音乐 API 导入歌词。
    /// </summary>
    [RelayCommand]
    public async Task ImportMusicLyricAsync(MusicLyricSearchResult? result)
    {
        if (result is null)
        {
            return;
        }

        try
        {
            var item = await musicLyricImportService.ImportAsync(result, Settings.QQMusicCookie);
            var savedItem = UpsertLyricLibraryItem(item);
            UseLyricLibraryItem(savedItem);
            await SaveAsync();

            if (IsLyricLibraryPickerOpen)
            {
                IsLyricLibraryPickerOpen = false;
            }
        }
        catch (Exception exception)
        {
            StatusMessage = $"歌词导入失败：{exception.Message}";
        }
    }

    /// <summary>
    /// 使用歌词库条目。
    /// </summary>
    [RelayCommand]
    public async Task UseLyricLibraryItemAsync(LyricLibraryItem? item)
    {
        if (item is null)
        {
            return;
        }

        UseLyricLibraryItem(item);
        await SaveAsync();
    }

    /// <summary>
    /// 在内嵌选择器中选择歌词库条目。
    /// </summary>
    [RelayCommand]
    public async Task PickLyricLibraryItemAsync(LyricLibraryItem? item)
    {
        if (item is null)
        {
            return;
        }

        UseLyricLibraryItem(item);
        IsLyricLibraryPickerOpen = false;
        await SaveAsync();
    }

    /// <summary>
    /// 删除歌词库条目。
    /// </summary>
    [RelayCommand]
    public async Task DeleteLyricLibraryItemAsync(LyricLibraryItem? item)
    {
        if (item is null)
        {
            return;
        }

        LyricLibrary.Remove(item);
        State.LyricLibrary.Remove(item);
        RefreshFilteredLyricLibrary();
        await SaveAsync();
    }

    /// <summary>
    /// 歌词库条目编辑后刷新并保存。
    /// </summary>
    public async Task SaveLyricLibraryEditAsync()
    {
        RefreshFilteredLyricLibrary();
        await SaveAsync();
    }

    /// <summary>
    /// 打开配置目录。
    /// </summary>
    [RelayCommand]
    public void OpenConfigDirectory()
    {
        OpenDirectory(ConfigDirectory, "配置目录打开失败");
    }

    /// <summary>
    /// 打开缓存目录。
    /// </summary>
    [RelayCommand]
    public void OpenCacheDirectory()
    {
        OpenDirectory(CacheDirectory, "缓存目录打开失败");
    }

    /// <summary>
    /// 刷新缓存体积。
    /// </summary>
    [RelayCommand]
    public void RefreshCacheSize()
    {
        CacheSizeText = AppBackupService.FormatSize(appBackupService.GetDirectorySize(CacheDirectory));
    }

    /// <summary>
    /// 清理缓存。
    /// </summary>
    [RelayCommand]
    public async Task ClearCacheAsync()
    {
        appBackupService.ClearCache();

        foreach (var room in Workspaces.SelectMany(workspace => workspace.Rooms))
        {
            room.AvatarPath = "";
        }

        RefreshCacheSize();
        await SaveAsync();
        StatusMessage = "缓存已清理";
    }

    /// <summary>
    /// 导出全部配置备份。
    /// </summary>
    [RelayCommand]
    public async Task ExportAllBackupAsync()
    {
        try
        {
            var outputDirectory = Path.Combine(ConfigDirectory, "Exports");
            LastBackupPath = await appBackupService.ExportBackupZipAsync(outputDirectory);
            StatusMessage = $"已导出备份：{LastBackupPath}";
            OpenDirectory(outputDirectory, "备份目录打开失败");
        }
        catch (Exception exception)
        {
            StatusMessage = $"导出备份失败：{exception.Message}";
        }
    }

    /// <summary>
    /// 保存应用状态。
    /// </summary>
    public async Task SaveAsync()
    {
        Settings.SidebarWidth = AppStateService.ClampSidebarWidth(Settings.SidebarWidth);
        await stateService.SaveAsync(State);
        StatusMessage = $"已保存 {DateTime.Now:HH:mm:ss}";
    }

    /// <summary>
    /// 导出当前工作区。
    /// </summary>
    public async Task ExportSelectedWorkspaceAsync(string filePath)
    {
        await ExportWorkspaceAsync(SelectedWorkspace, filePath);
    }

    /// <summary>
    /// 导出指定工作区。
    /// </summary>
    public async Task ExportWorkspaceAsync(WorkspaceViewModel? workspace, string filePath)
    {
        if (workspace is null)
        {
            return;
        }

        await workspaceShareService.ExportAsync(workspace.State, Settings, filePath);
        StatusMessage = "工作区已导出";
    }

    /// <summary>
    /// 导入工作区。
    /// </summary>
    public async Task ImportWorkspaceAsync(string filePath)
    {
        var workspaceState = await workspaceShareService.ImportAsync(State.Workspaces, filePath);
        State.Workspaces.Add(workspaceState);
        var workspace = CreateWorkspaceViewModel(workspaceState);
        Workspaces.Add(workspace);
        SelectedWorkspace = workspace;
        await SaveAsync();
        StatusMessage = "工作区已导入";
    }

    /// <summary>
    /// 获取导出文件名。
    /// </summary>
    public string GetExportFileName()
    {
        return GetExportFileName(SelectedWorkspace);
    }

    /// <summary>
    /// 获取指定工作区的导出文件名。
    /// </summary>
    public string GetExportFileName(WorkspaceViewModel? workspace)
    {
        var name = workspace?.Name ?? "workspace";
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName =
            new string(name.Select(character => invalidChars.Contains(character) ? '_' : character).ToArray());
        return $"{safeName}.dnworkspace.json";
    }

    private WorkspaceViewModel CreateWorkspaceViewModel(WorkspaceState state)
    {
        var workspace = new WorkspaceViewModel(state, GetAccountName);

        foreach (var room in workspace.Rooms)
        {
            ConfigureRoom(room);
        }

        workspace.RefreshRoomSelection();
        return workspace;
    }

    private void ConfigureRoom(LiveRoomTabViewModel room)
    {
        room.Configure(
            ResolveAccount,
            ResolveAccount,
            () => Settings,
            () => ResolveMarkGroup(room),
            () => SymbolGroups,
            CreateForwardSourceRoomOptions,
            danmuSendService,
            avatarCacheService,
            SaveAsync);
        _ = room.LoadAvatarAsync();
    }

    private BilibiliAccount? ResolveAccount(LiveRoomTabViewModel room)
    {
        var workspace = Workspaces.FirstOrDefault(item => item.Rooms.Contains(room));

        if (workspace is null)
        {
            return null;
        }

        return accountSelectionService.Resolve(State, workspace.State, room.State);
    }

    private BilibiliAccount? ResolveAccount(string workspaceId, string roomStateId)
    {
        var workspace = Workspaces.FirstOrDefault(item => item.Id == workspaceId);
        var room = workspace?.Rooms.FirstOrDefault(item => item.Id == roomStateId);

        if (workspace is null)
        {
            return null;
        }

        return accountSelectionService.Resolve(State, workspace.State, room?.State);
    }

    private IEnumerable<ForwardSourceRoomOption> CreateForwardSourceRoomOptions(LiveRoomTabViewModel targetRoom)
    {
        foreach (var workspace in Workspaces)
        {
            foreach (var room in workspace.Rooms)
            {
                if (room.Id == targetRoom.Id)
                {
                    continue;
                }

                yield return new()
                {
                    WorkspaceId = workspace.Id,
                    WorkspaceName = workspace.Name,
                    RoomStateId = room.Id,
                    RoomId = room.RoomId,
                    RoomName = room.RoomName
                };
            }
        }
    }

    private void RefreshForwardSourceRooms()
    {
        foreach (var room in Workspaces.SelectMany(workspace => workspace.Rooms))
        {
            room.RefreshForwardSourceRooms();
        }
    }

    private void RestartForwarding()
    {
        foreach (var room in Workspaces.SelectMany(workspace => workspace.Rooms))
        {
            room.RestartForwarding();
        }
    }

    private MarkSymbolGroup ResolveMarkGroup(LiveRoomTabViewModel room)
    {
        var workspace = Workspaces.FirstOrDefault(item => item.Rooms.Contains(room)) ?? SelectedWorkspace;
        return markSymbolService.Resolve(Settings, workspace?.State);
    }

    private string GetAccountName(string? accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return "使用全局账号";
        }

        return Accounts.FirstOrDefault(account => account.Id == accountId)?.Name ?? "账号不存在";
    }

    private async Task MoveWorkspaceAsync(WorkspaceViewModel? workspace, int offset)
    {
        if (workspace is null)
        {
            return;
        }

        var oldIndex = Workspaces.IndexOf(workspace);
        var newIndex = oldIndex + offset;

        if (oldIndex < 0 || newIndex < 0 || newIndex >= Workspaces.Count)
        {
            return;
        }

        Workspaces.Move(oldIndex, newIndex);
        RefreshWorkspaceSortOrder();
        await SaveAsync();
    }

    private async Task MoveRoomAsync(LiveRoomTabViewModel? room, int offset)
    {
        if (room is null)
        {
            return;
        }

        var workspace = Workspaces.FirstOrDefault(item => item.Rooms.Contains(room));

        if (workspace is null || !workspace.MoveRoom(room, offset))
        {
            return;
        }

        await SaveAsync();
    }

    private async Task MoveSymbolGroupAsync(MarkSymbolGroup? group, int offset)
    {
        if (group is null)
        {
            return;
        }

        var oldIndex = SymbolGroups.IndexOf(group);
        var newIndex = oldIndex + offset;

        if (oldIndex < 0 || newIndex < 0 || newIndex >= SymbolGroups.Count)
        {
            return;
        }

        SymbolGroups.Move(oldIndex, newIndex);
        RefreshSymbolGroupSortOrder();
        await SaveAsync();
    }

    private void RefreshWorkspaceSortOrder()
    {
        State.Workspaces.Clear();

        for (var index = 0; index < Workspaces.Count; index++)
        {
            Workspaces[index].State.SortOrder = index;
            State.Workspaces.Add(Workspaces[index].State);
        }
    }

    private void RefreshSymbolGroupSortOrder()
    {
        Settings.MarkGroups.Clear();

        for (var index = 0; index < SymbolGroups.Count; index++)
        {
            SymbolGroups[index].SortOrder = index;
            Settings.MarkGroups.Add(SymbolGroups[index]);
        }
    }

    private void RefreshAccountLabels()
    {
        foreach (var workspace in Workspaces)
        {
            workspace.RefreshAccountLabel();
        }
    }

    private void RefreshThemeModeOptions()
    {
        foreach (var option in ThemeModeOptions)
        {
            option.IsSelected = option.Value == Settings.ThemeMode;
        }
    }

    private void RefreshSettingsSectionOptions()
    {
        foreach (var option in SettingsSectionOptions)
        {
            option.IsSelected = option.Key == selectedSettingsSectionKey;
        }
    }

    private LyricLibraryItem UpsertLyricLibraryItem(LyricLibraryItem item)
    {
        var savedItem = lyricLibraryService.Upsert(State.LyricLibrary, item);

        if (!LyricLibrary.Contains(savedItem))
        {
            LyricLibrary.Add(savedItem);
        }

        RefreshFilteredLyricLibrary();
        StatusMessage = "歌词已写入本地歌词库";
        return savedItem;
    }

    private void UseLyricLibraryItem(LyricLibraryItem item)
    {
        var room = SelectedWorkspace?.SelectedRoom;

        if (room is null)
        {
            return;
        }

        lyricLibraryService.Touch(item);
        room.LoadLyric(item);
        RefreshFilteredLyricLibrary();
        StatusMessage = "歌词已加载到当前直播间";
    }

    private void RefreshFilteredLyricLibrary()
    {
        FilteredLyricLibrary.Clear();

        foreach (var item in lyricLibraryService.Search(State.LyricLibrary, LyricLibrarySearchText))
        {
            FilteredLyricLibrary.Add(item);
        }
    }

    private void OpenDirectory(string directory, string errorPrefix)
    {
        try
        {
            appBackupService.OpenDirectory(directory);
        }
        catch (Exception exception)
        {
            StatusMessage = $"{errorPrefix}：{exception.Message}";
        }
    }

    private void RefreshTreeSelection()
    {
        foreach (var workspace in Workspaces)
        {
            workspace.IsSelected = workspace == SelectedWorkspace;
            workspace.RefreshRoomSelection();
        }
    }
}