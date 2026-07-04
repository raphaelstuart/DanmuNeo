using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播清晰度描述。
/// </summary>
public class BilibiliLiveQualityDescription
{
    /// <summary>
    /// 清晰度编号。
    /// </summary>
    [JsonProperty("qn")]
    public int Quality { get; set; }

    /// <summary>
    /// 清晰度文本。
    /// </summary>
    [JsonProperty("desc")]
    public string Description { get; set; } = "";
}
