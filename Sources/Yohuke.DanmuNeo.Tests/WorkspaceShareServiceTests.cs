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
                    RoomName = "导入房间"
                }
            ]
        };

        var workspace = service.CreateWorkspace(existing, package);

        Assert.Equal("共享 (2)", workspace.Name);
        Assert.Null(workspace.AccountOverrideId);
        Assert.Single(workspace.LiveRooms);
        Assert.Null(workspace.LiveRooms[0].AccountOverrideId);
        Assert.Equal("456", workspace.LiveRooms[0].RoomId);
    }
}
