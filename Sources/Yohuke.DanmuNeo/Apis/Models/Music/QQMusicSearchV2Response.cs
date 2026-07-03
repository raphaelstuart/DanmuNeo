using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐新版搜索响应。
/// </summary>
public class QQMusicSearchV2Response
{
    /// <summary>
    /// 状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 搜索请求响应。
    /// </summary>
    [JsonProperty("req_1")]
    public QQMusicSearchV2Request? Request { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
