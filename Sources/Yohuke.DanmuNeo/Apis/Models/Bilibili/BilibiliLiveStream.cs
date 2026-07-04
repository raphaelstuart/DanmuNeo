using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播协议流。
/// </summary>
public class BilibiliLiveStream
{
    /// <summary>
    /// 协议名称。
    /// </summary>
    [JsonProperty("protocol_name")]
    public string ProtocolName { get; set; } = "";

    /// <summary>
    /// 格式列表。
    /// </summary>
    [JsonProperty("format")]
    public List<BilibiliLiveFormat> Formats { get; set; } = [];
}
