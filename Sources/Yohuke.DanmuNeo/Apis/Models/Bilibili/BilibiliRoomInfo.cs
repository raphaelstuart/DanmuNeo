using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播间基础信息。
/// </summary>
public class BilibiliRoomInfo
{
    /// <summary>
    /// 主播 UID。
    /// </summary>
    [JsonProperty("uid")]
    public long Uid { get; set; }

    /// <summary>
    /// 直播间 ID。
    /// </summary>
    [JsonProperty("room_id")]
    public long RoomId { get; set; }

    /// <summary>
    /// 短房间号。
    /// </summary>
    [JsonProperty("short_id")]
    public long ShortId { get; set; }

    /// <summary>
    /// 标题。
    /// </summary>
    [JsonProperty("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 简介。
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 直播状态。
    /// </summary>
    [JsonProperty("live_status")]
    public int LiveStatus { get; set; }

    /// <summary>
    /// 分区 ID。
    /// </summary>
    [JsonProperty("area_id")]
    public long AreaId { get; set; }

    /// <summary>
    /// 父分区 ID。
    /// </summary>
    [JsonProperty("parent_area_id")]
    public long ParentAreaId { get; set; }

    /// <summary>
    /// 人气值。
    /// </summary>
    [JsonProperty("online")]
    public long Online { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
