using System.Text.Json;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class WorkspaceShareServiceTests
{
    [Fact]
    public void CreatePackageDoesNotContainSensitiveFields()
    {
        var service = new WorkspaceShareService();
        var workspace = new WorkspaceState
        {
            Name = "测试工作区",
            AccountOverrideId = "secret-account",
            LiveRooms =
            [
                new()
                {
                    RoomId = "123",
                    RoomName = "直播间",
                    OwnerUid = "10001",
                    AccountOverrideId = "room-account",
                    InputDraft = "draft",
                    LyricTitle = "song",
                    LyricText = "[00:01]line",
                    ForwardRules =
                    [
                        new()
                        {
                            SourceWorkspaceId = "source-workspace",
                            SourceRoomStateId = "source-room",
                            SenderUid = "123456",
                            ContentPattern = "secret-pattern"
                        }
                    ]
                }
            ]
        };
        var settings = new AppSettings
        {
            QQMusicCookie = "uin=secret;qm_keyst=secret"
        };

        var package = service.CreatePackage(workspace, settings);
        var json = JsonSerializer.Serialize(package);

        Assert.DoesNotContain("secret-account", json);
        Assert.DoesNotContain("room-account", json);
        Assert.DoesNotContain("qm_keyst", json);
        Assert.DoesNotContain("draft", json);
        Assert.DoesNotContain("secret-pattern", json);
        Assert.DoesNotContain("source-workspace", json);
        Assert.Equal("直播间", package.Rooms[0].RoomName);
        Assert.Equal("10001", package.Rooms[0].OwnerUid);
    }

    [Fact]
    public void CreateWorkspaceGeneratesNewIdAndUniqueName()
    {
        var service = new WorkspaceShareService();
        var existing = new[]
        {
            new WorkspaceState
            {
                Name = "共享"
            }
        };
        var package = new WorkspaceSharePackage
        {
            WorkspaceName = "共享",
            Rooms =
            [
                new()
                {
                    RoomId = "456",
                    RoomName = "导入房间",
                    OwnerUid = "20002"
                }
            ]
        };

        var workspace = service.CreateWorkspace(existing, package);

        Assert.Equal("共享 (2)", workspace.Name);
        Assert.Null(workspace.AccountOverrideId);
        Assert.Single(workspace.LiveRooms);
        Assert.Null(workspace.LiveRooms[0].AccountOverrideId);
        Assert.Equal("456", workspace.LiveRooms[0].RoomId);
        Assert.Equal("20002", workspace.LiveRooms[0].OwnerUid);
    }

    [Fact]
    public void CreatePackageAndWorkspacePreserveMultilingualLyrics()
    {
        var service = new WorkspaceShareService();
        var sourceWorkspace = new WorkspaceState
        {
            Name = "双语歌词工作区",
            LiveRooms =
            [
                new()
                {
                    RoomId = "789",
                    RoomName = "双语歌词直播间",
                    LyricTitle = "双语歌曲",
                    LyricText = "[00:01]原文",
                    TranslatedLyricText = "[00:01]翻译",
                    LyricSendMode = LyricSendMode.TranslationOnly
                }
            ]
        };

        var package = service.CreatePackage(sourceWorkspace, new());
        var packageRoom = Assert.Single(package.Rooms);
        var importedWorkspace = service.CreateWorkspace([], package);
        var importedRoom = Assert.Single(importedWorkspace.LiveRooms);

        Assert.Equal("[00:01]原文", packageRoom.LyricText);
        Assert.Equal("[00:01]翻译", packageRoom.TranslatedLyricText);
        Assert.Equal(LyricSendMode.TranslationOnly, packageRoom.LyricSendMode);
        Assert.Equal("[00:01]原文", importedRoom.LyricText);
        Assert.Equal("[00:01]翻译", importedRoom.TranslatedLyricText);
        Assert.Equal(LyricSendMode.TranslationOnly, importedRoom.LyricSendMode);
    }

    [Fact]
    public void CreateWorkspaceNormalizesUnknownLyricSendMode()
    {
        var service = new WorkspaceShareService();
        var package = new WorkspaceSharePackage
        {
            Rooms =
            [
                new()
                {
                    RoomId = "789",
                    LyricSendMode = (LyricSendMode)999
                }
            ]
        };

        var workspace = service.CreateWorkspace([], package);

        Assert.Equal(LyricSendMode.Bilingual, workspace.LiveRooms[0].LyricSendMode);
    }
}
