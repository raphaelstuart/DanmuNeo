using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐旧版搜索响应。
/// </summary>
public class QQMusicSearchV1Response
{
    /// <summary>
    /// 状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 搜索数据。
    /// </summary>
    [JsonProperty("data")]
    public QQMusicSearchV1Data? Data { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
