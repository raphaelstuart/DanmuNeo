using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 提供本地直播播放器页面与代理流。
/// </summary>
public interface ILivePlayerService
{
    /// <summary>
    /// 创建播放器会话。
    /// </summary>
    Task<LivePlayerSession> CreatePlayerAsync(
        LiveStreamPlaySource source,
        string? cookie,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 撤销播放器会话。
    /// </summary>
    void Revoke(string? token);
}
