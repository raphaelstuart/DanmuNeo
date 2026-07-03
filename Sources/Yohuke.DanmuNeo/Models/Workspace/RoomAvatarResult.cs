namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 直播间头像拉取结果。
/// </summary>
public class RoomAvatarResult
{
    /// <summary>
    /// 主播 UID。
    /// </summary>
    public string OwnerUid { get; set; } = "";

    /// <summary>
    /// 主播名称。
    /// </summary>
    public string OwnerName { get; set; } = "";

    /// <summary>
    /// 头像缓存路径。
    /// </summary>
    public string AvatarPath { get; set; } = "";
}