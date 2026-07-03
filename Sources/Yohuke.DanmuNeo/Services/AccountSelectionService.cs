using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 解析直播间实际使用的账号。
/// </summary>
public class AccountSelectionService
{
    /// <summary>
    /// 按标签页、工作区、全局默认的顺序选择账号。
    /// </summary>
    public BilibiliAccount? Resolve(AppState state, WorkspaceState workspace, LiveRoomTabState? room)
    {
        var accountId = room?.AccountOverrideId;

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            var roomAccount = state.Accounts.FirstOrDefault(account => account.Id == accountId);

            if (roomAccount is not null)
            {
                return roomAccount;
            }
        }

        accountId = workspace.AccountOverrideId;

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            var workspaceAccount = state.Accounts.FirstOrDefault(account => account.Id == accountId);

            if (workspaceAccount is not null)
            {
                return workspaceAccount;
            }
        }

        return state.Accounts.FirstOrDefault(account => account.IsGlobalDefault) ?? state.Accounts.FirstOrDefault();
    }
}
