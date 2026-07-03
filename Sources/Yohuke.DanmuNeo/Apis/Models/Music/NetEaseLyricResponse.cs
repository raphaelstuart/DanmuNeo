using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// 网易云音乐歌词响应。
/// </summary>
public class NetEaseLyricResponse
{
    /// <summary>
    /// 状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 原文歌词。
    /// </summary>
    [JsonProperty("lrc")]
    public NetEaseLyricData? Lyric { get; set; }

    /// <summary>
    /// 翻译歌词。
    /// </summary>
    [JsonProperty("tlyric")]
    public NetEaseLyricData? TranslatedLyric { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
