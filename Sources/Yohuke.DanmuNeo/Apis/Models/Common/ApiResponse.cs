using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Common;

/// <summary>
/// 表示 code/message/data 结构的平台响应。
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// 平台状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 平台消息。
    /// </summary>
    [JsonProperty("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 平台消息。
    /// </summary>
    [JsonProperty("msg")]
    public string? Msg { get; set; }

    /// <summary>
    /// 响应数据。
    /// </summary>
    [JsonProperty("data")]
    public T? Data { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
