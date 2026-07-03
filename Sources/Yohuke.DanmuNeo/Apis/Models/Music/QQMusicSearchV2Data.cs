using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐新版搜索数据。
/// </summary>
public class QQMusicSearchV2Data
{
    /// <summary>
    /// 响应正文。
    /// </summary>
    [JsonProperty("body")]
    public QQMusicSearchV2Body? Body { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
