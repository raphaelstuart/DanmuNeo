using Yohuke.DanmuNeo.Apis;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 创建 B 站直播弹幕连接。
/// </summary>
public class BilibiliLiveDanmuSocketFactory : ILiveDanmuSocketFactory
{
    /// <inheritdoc/>
    public ILiveDanmuSocket Create(long roomId, string cookie)
    {
        return new BilibiliLiveWebSocket(roomId, cookie);
    }
}
