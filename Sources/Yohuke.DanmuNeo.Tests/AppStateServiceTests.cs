using Yohuke.DanmuNeo.Services;
using Yohuke.DanmuNeo.Models.State;
using System.Text.Json;

namespace Yohuke.DanmuNeo.Tests;

public class AppStateServiceTests
{
    [Fact]
    public void DefaultConfigDirectoryUsesApplicationData()
    {
        var service = new AppStateService();
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yohuke.DanmuNeo");

        Assert.Equal(expected, service.ConfigDirectory);
    }

    [Fact]
    public async Task LoadCreatesDefaultStateWhenFileMissing()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppStateService(directory);

        var state = await service.LoadAsync();

        Assert.Single(state.Workspaces);
        Assert.Equal("默认工作区", state.Workspaces[0].Name);
        Assert.Equal(Yohuke.DanmuNeo.Models.State.AppThemeMode.System, state.Settings.ThemeMode);
        Assert.True(File.Exists(service.SettingsFilePath));
        Assert.True(File.Exists(service.AccountsFilePath));
        Assert.True(File.Exists(service.LyricLibraryFilePath));
        Assert.True(File.Exists(service.WorkspacesFilePath));
    }

    [Fact]
    public async Task LoadFallsBackToDefaultStateWhenJsonIsBroken()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var service = new AppStateService(directory);
        await File.WriteAllTextAsync(service.WorkspacesFilePath, "{ broken json");

        var state = await service.LoadAsync();

        Assert.Single(state.Workspaces);
        Assert.Equal("默认工作区", state.Workspaces[0].Name);
        Assert.True(Directory.EnumerateFiles(service.CorruptDirectory).Any());
    }

    [Theory]
    [InlineData(120, 200)]
    [InlineData(240, 240)]
    [InlineData(600, 360)]
    public void ClampSidebarWidthKeepsConfiguredRange(double input, double expected)
    {
        Assert.Equal(expected, AppStateService.ClampSidebarWidth(input));
    }

    [Fact]
    public void SaveAndLoadKeepsWorkspaceAndRoomSortOrder()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppStateService(directory);
        var state = new AppState
        {
            Workspaces =
            [
                new()
                {
                    Name = "B",
                    SortOrder = 1,
                    LiveRooms =
                    [
                        new()
                        {
                            RoomName = "B2",
                            SortOrder = 1
                        },
                        new()
                        {
                            RoomName = "B1",
                            SortOrder = 0
                        }
                    ]
                },
                new()
                {
                    Name = "A",
                    SortOrder = 0
                }
            ]
        };

        service.Save(state);
        var loaded = service.Load();

        Assert.Equal("A", loaded.Workspaces[0].Name);
        Assert.Equal("B", loaded.Workspaces[1].Name);
        Assert.Equal("B1", loaded.Workspaces[1].LiveRooms[0].RoomName);
        Assert.Equal("B2", loaded.Workspaces[1].LiveRooms[1].RoomName);
    }

    [Fact]
    public void LoadMigratesLegacyStateFileToSplitStorage()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var service = new AppStateService(directory);
        var legacyState = new AppState
        {
            Workspaces =
            [
                new()
                {
                    Name = "旧工作区"
                }
            ],
            LyricLibrary =
            [
                new()
                {
                    Title = "旧歌词",
                    LyricText = "[00:01.00]test"
                }
            ]
        };
        File.WriteAllText(service.StateFilePath, JsonSerializer.Serialize(legacyState));

        var state = service.Load();

        Assert.Equal("旧工作区", state.Workspaces[0].Name);
        Assert.Equal("旧歌词", state.LyricLibrary[0].Title);
        Assert.True(File.Exists(service.SettingsFilePath));
        Assert.True(File.Exists(service.LyricLibraryFilePath));
        Assert.True(File.Exists(service.WorkspacesFilePath));
    }

    [Fact]
    public void SaveCreatesBackupBeforeReplacingSplitFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppStateService(directory);
        var firstState = new AppState
        {
            Workspaces =
            [
                new()
                {
                    Name = "第一次保存"
                }
            ]
        };
        var secondState = new AppState
        {
            Workspaces =
            [
                new()
                {
                    Name = "第二次保存"
                }
            ]
        };

        service.Save(firstState);
        service.Save(secondState);

        var backupFile = Directory
            .EnumerateFiles(service.BackupDirectory, "workspaces.json.*.bak")
            .Single();
        var backupState = JsonSerializer.Deserialize<WorkspaceStorageState>(File.ReadAllText(backupFile));

        Assert.Equal("第一次保存", backupState?.Workspaces[0].Name);
        Assert.Equal("第二次保存", service.Load().Workspaces[0].Name);
    }

    [Fact]
    public void LoadRestoresWorkspaceFileFromLatestValidBackup()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppStateService(directory);
        var state = new AppState
        {
            Workspaces =
            [
                new()
                {
                    Name = "可恢复工作区"
                }
            ]
        };

        service.Save(state);
        service.Save(state);
        File.WriteAllText(service.WorkspacesFilePath, "{ broken json");

        var loaded = service.Load();

        Assert.Equal("可恢复工作区", loaded.Workspaces[0].Name);
        var restoredState =
            JsonSerializer.Deserialize<WorkspaceStorageState>(File.ReadAllText(service.WorkspacesFilePath));
        Assert.Equal("可恢复工作区", restoredState?.Workspaces[0].Name);
    }

    [Fact]
    public void BrokenLyricLibraryDoesNotDropWorkspaces()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppStateService(directory);
        var state = new AppState
        {
            Workspaces =
            [
                new()
                {
                    Name = "工作区仍在"
                }
            ],
            LyricLibrary =
            [
                new()
                {
                    Title = "歌词",
                    LyricText = "[00:01.00]test"
                }
            ]
        };

        service.Save(state);
        File.WriteAllText(service.LyricLibraryFilePath, "{ broken json");

        var loaded = service.Load();

        Assert.Equal("工作区仍在", loaded.Workspaces[0].Name);
        Assert.Empty(loaded.LyricLibrary);
    }

    [Fact]
    public void SaveAndLoadKeepsForwardRules()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppStateService(directory);
        var state = new AppState
        {
            Settings = new()
            {
                MarkGroups =
                [
                    new()
                    {
                        Id = "mark-group",
                        Name = "转发符号组"
                    }
                ]
            },
            Workspaces =
            [
                new()
                {
                    Name = "转发工作区",
                    LiveRooms =
                    [
                        new()
                        {
                            RoomId = "100",
                            RoomName = "目标",
                            OwnerUid = "10001",
                            ForwardRules =
                            [
                                new()
                                {
                                    SourceWorkspaceId = "source-workspace",
                                    SourceRoomStateId = "source-room",
                                    MarkSymbolGroupId = "mark-group",
                                    SenderUid = "42",
                                    ContentPattern = "^【.+】$",
                                    IsEnabled = true
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        service.Save(state);
        var loaded = service.Load();
        var rule = loaded.Workspaces[0].LiveRooms[0].ForwardRules[0];

        Assert.Equal("10001", loaded.Workspaces[0].LiveRooms[0].OwnerUid);
        Assert.True(rule.IsEnabled);
        Assert.Equal("source-workspace", rule.SourceWorkspaceId);
        Assert.Equal("source-room", rule.SourceRoomStateId);
        Assert.Equal("mark-group", rule.MarkSymbolGroupId);
        Assert.Equal("42", rule.SenderUid);
        Assert.Equal("^【.+】$", rule.ContentPattern);
    }
}