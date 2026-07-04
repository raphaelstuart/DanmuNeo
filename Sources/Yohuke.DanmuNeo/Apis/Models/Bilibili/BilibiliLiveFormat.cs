using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播封装格式。
/// </summary>
public class BilibiliLiveFormat
{
    /// <summary>
    /// 格式名称。
    /// </summary>
    [JsonProperty("format_name")]
    public string FormatName { get; set; } = "";

    /// <summary>
    /// 编码列表。
    /// </summary>
    [JsonProperty("codec")]
    public List<BilibiliLiveCodec> Codecs { get; set; } = [];
}
