using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐旧版搜索数据。
/// </summary>
public class QQMusicSearchV1Data
{
    /// <summary>
    /// 歌曲容器。
    /// </summary>
    [JsonProperty("song")]
    public QQMusicSearchV1SongContainer? Song { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
