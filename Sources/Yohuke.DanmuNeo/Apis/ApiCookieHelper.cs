using System.Net;
using System.Text.RegularExpressions;

namespace Yohuke.DanmuNeo.Apis;

/// <summary>
/// 提供平台 Cookie 的解析与写入能力。
/// </summary>
public static class ApiCookieHelper
{
    /// <summary>
    /// 从 Cookie 字符串解析键值。
    /// </summary>
    public static Dictionary<string, string> Parse(string cookie)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cleanedCookie = Regex.Replace(cookie, @"\s+", "");
        var segments = cleanedCookie.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            var index = segment.IndexOf('=');

            if (index <= 0)
            {
                continue;
            }

            var key = segment[..index];
            var value = segment[(index + 1)..];
            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// 生成 B 站直播接口所需 Cookie。
    /// </summary>
    public static string CreateBilibiliCookie(string cookie)
    {
        var values = Parse(cookie);
        values.TryGetValue("buvid3", out var buvid3);
        values.TryGetValue("SESSDATA", out var sessData);
        values.TryGetValue("bili_jct", out var biliJct);
        values.TryGetValue("DedeUserId", out var dedeUserId);
        values.TryGetValue("DedeUserID", out var dedeUserIdUpper);

        return $"buvid3={buvid3 ?? ""};SESSDATA={sessData ?? ""};bili_jct={biliJct ?? ""};DedeUserId={dedeUserId ?? dedeUserIdUpper ?? ""}";
    }

    /// <summary>
    /// 将 Cookie 字符串写入容器。
    /// </summary>
    public static void AddCookies(CookieContainer container, Uri uri, string cookie)
    {
        foreach (var pair in Parse(cookie))
        {
            container.Add(uri, new Cookie(pair.Key, pair.Value));
        }
    }

    /// <summary>
    /// 从 Cookie 字符串中读取指定值。
    /// </summary>
    public static string GetValue(string cookie, string name)
    {
        var values = Parse(cookie);
        return values.TryGetValue(name, out var value) ? value : "";
    }
}
