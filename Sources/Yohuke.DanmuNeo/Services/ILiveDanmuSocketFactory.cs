namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 创建直播弹幕连接。
/// </summary>
public interface ILiveDanmuSocketFactory
{
    /// <summary>
    /// 创建直播弹幕连接。
    /// </summary>
    ILiveDanmuSocket Create(long roomId, string cookie);
}
