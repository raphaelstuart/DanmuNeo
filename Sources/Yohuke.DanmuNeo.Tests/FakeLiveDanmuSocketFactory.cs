using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

/// <summary>
/// 测试用直播弹幕连接工厂。
/// </summary>
public class FakeLiveDanmuSocketFactory : ILiveDanmuSocketFactory
{
    public FakeLiveDanmuSocket Socket { get; } = new();

    public List<(long RoomId, string Cookie)> Requests { get; } = [];

    public ILiveDanmuSocket Create(long roomId, string cookie)
    {
        Requests.Add((roomId, cookie));
        return Socket;
    }
}
