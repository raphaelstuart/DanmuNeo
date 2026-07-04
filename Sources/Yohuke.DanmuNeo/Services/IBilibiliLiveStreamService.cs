using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 提供 B 站直播可播放流。
/// </summary>
public interface IBilibiliLiveStreamService
{
    /// <summary>
    /// 获取直播间可播放流。
    /// </summary>
    Task<LiveStreamPlaySource> GetPlayableSourceAsync(
        string roomId,
        int quality,
        string? cookie,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}
