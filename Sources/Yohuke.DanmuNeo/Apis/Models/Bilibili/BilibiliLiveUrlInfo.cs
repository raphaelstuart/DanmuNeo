using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播 CDN 地址片段。
/// </summary>
public class BilibiliLiveUrlInfo
{
    /// <summary>
    /// CDN 主机。
    /// </summary>
    [JsonProperty("host")]
    public string Host { get; set; } = "";

    /// <summary>
    /// 附加查询参数。
    /// </summary>
    [JsonProperty("extra")]
    public string Extra { get; set; } = "";
}
