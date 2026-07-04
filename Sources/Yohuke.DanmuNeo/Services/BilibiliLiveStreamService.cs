using Yohuke.DanmuNeo.Apis;
using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 获取并解析 B 站直播播放流。
/// </summary>
public class BilibiliLiveStreamService : IBilibiliLiveStreamService
{
    /// <inheritdoc/>
    public async Task<LiveStreamPlaySource> GetPlayableSourceAsync(
        string roomId,
        int quality,
        string? cookie,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (!long.TryParse(roomId, out var parsedRoomId))
        {
            throw new InvalidOperationException("房间号格式错误");
        }

        using var api = new BilibiliApi(cookie ?? "", timeout);
        var response = await api.GetRoomPlayInfoAsync(parsedRoomId, quality, cookie, timeout, cancellationToken);

        if (response.Code != 0)
        {
            throw new InvalidOperationException(response.Message ?? response.Msg ?? "直播流获取失败");
        }

        return BilibiliLivePlayInfoSelector.Select(response.Data, roomId);
    }
}
