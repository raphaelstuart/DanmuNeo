using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// 网易云音乐歌词数据。
/// </summary>
public class NetEaseLyricData
{
    /// <summary>
    /// 歌词文本。
    /// </summary>
    [JsonProperty("lyric")]
    public string Lyric { get; set; } = "";

    /// <summary>
    /// 版本号。
    /// </summary>
    [JsonProperty("version")]
    public long Version { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
