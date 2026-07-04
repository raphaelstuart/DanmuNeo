using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播播放地址列表。
/// </summary>
public class BilibiliLivePlayUrl
{
    /// <summary>
    /// 清晰度描述。
    /// </summary>
    [JsonProperty("g_qn_desc")]
    public List<BilibiliLiveQualityDescription> QualityDescriptions { get; set; } = [];

    /// <summary>
    /// 流列表。
    /// </summary>
    [JsonProperty("stream")]
    public List<BilibiliLiveStream> Streams { get; set; } = [];
}
