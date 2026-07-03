using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播间信息数据。
/// </summary>
public class BilibiliRoomInfoData
{
    /// <summary>
    /// 直播间基础信息。
    /// </summary>
    [JsonProperty("room_info")]
    public BilibiliRoomInfo? RoomInfo { get; set; }

    /// <summary>
    /// 主播信息。
    /// </summary>
    [JsonProperty("anchor_info")]
    public BilibiliAnchorInfo? AnchorInfo { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
