using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播播放信息。
/// </summary>
public class BilibiliLivePlayInfoData
{
    /// <summary>
    /// 直播间 ID。
    /// </summary>
    [JsonProperty("room_id")]
    public long RoomId { get; set; }

    /// <summary>
    /// 主播 UID。
    /// </summary>
    [JsonProperty("uid")]
    public long Uid { get; set; }

    /// <summary>
    /// 直播状态，1 为直播中。
    /// </summary>
    [JsonProperty("live_status")]
    public int LiveStatus { get; set; }

    /// <summary>
    /// 开播时间戳。
    /// </summary>
    [JsonProperty("live_time")]
    public long LiveTime { get; set; }

    /// <summary>
    /// 播放地址信息。
    /// </summary>
    [JsonProperty("playurl_info")]
    public BilibiliLivePlayUrlInfo? PlayUrlInfo { get; set; }
}
