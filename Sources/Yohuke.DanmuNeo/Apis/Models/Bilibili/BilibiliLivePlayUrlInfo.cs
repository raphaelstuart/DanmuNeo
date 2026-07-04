using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播播放地址容器。
/// </summary>
public class BilibiliLivePlayUrlInfo
{
    /// <summary>
    /// 播放地址。
    /// </summary>
    [JsonProperty("playurl")]
    public BilibiliLivePlayUrl? PlayUrl { get; set; }
}
