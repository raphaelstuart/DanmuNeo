using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class AccountSelectionServiceTests
{
    [Fact]
    public void ResolveUsesRoomWorkspaceThenGlobalOrder()
    {
        var global = new BilibiliAccount
        {
            Id = "global",
            Name = "全局",
            IsGlobalDefault = true
        };
        var workspaceAccount = new BilibiliAccount
        {
            Id = "workspace",
            Name = "工作区"
        };
        var roomAccount = new BilibiliAccount
        {
            Id = "room",
            Name = "房间"
        };
        var state = new AppState
        {
            Accounts = [global, workspaceAccount, roomAccount]
        };
        var workspace = new WorkspaceState
        {
            AccountOverrideId = workspaceAccount.Id
        };
        var room = new LiveRoomTabState
        {
            AccountOverrideId = roomAccount.Id
        };
        var service = new AccountSelectionService();

        Assert.Equal(roomAccount, service.Resolve(state, workspace, room));

        room.AccountOverrideId = null;
        Assert.Equal(workspaceAccount, service.Resolve(state, workspace, room));

        workspace.AccountOverrideId = null;
        Assert.Equal(global, service.Resolve(state, workspace, room));
    }
}
