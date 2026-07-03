using System.Text.RegularExpressions;
using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 解析歌词时间轴并提供发送文本。
/// </summary>
public class LyricTimelineService
{
    private static readonly Regex TIMELINE_PATTERN = new(@"\[(?<minute>\d+):(?<second>\d+)(?<decimal>\.\d+)?\]", RegexOptions.Compiled);

    /// <summary>
    /// 解析 LRC 歌词文本。
    /// </summary>
    public List<LyricLineState> Parse(string lyricText)
    {
        var lines = new List<LyricLineState>();

        foreach (var rawLine in lyricText.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();

            if (line.Length == 0)
            {
                continue;
            }

            var matches = TIMELINE_PATTERN.Matches(line);
            var content = TIMELINE_PATTERN.Replace(line, "").Trim();

            if (matches.Count == 0)
            {
                lines.Add(new()
                {
                    TimeSeconds = -1,
                    Timeline = "",
                    Content = content
                });
                continue;
            }

            foreach (Match match in matches)
            {
                var seconds = ParseSeconds(match);
                lines.Add(new()
                {
                    TimeSeconds = seconds,
                    Timeline = FormatTimeline(seconds),
                    Content = content
                });
            }
        }

        return lines.OrderBy(line => line.TimeSeconds < 0 ? double.MaxValue : line.TimeSeconds).ToList();
    }

    /// <summary>
    /// 生成要发送的歌词弹幕。
    /// </summary>
    public static string CreateMessage(string openMark, string closeMark, string content)
    {
        return $"{openMark}{content}{closeMark}";
    }

    /// <summary>
    /// 判断歌词是否包含时间轴。
    /// </summary>
    public static bool HasTimeline(IEnumerable<LyricLineState> lines)
    {
        return lines.Any(line => line.TimeSeconds >= 0);
    }

    private static double ParseSeconds(Match match)
    {
        var minute = int.Parse(match.Groups["minute"].Value);
        var second = int.Parse(match.Groups["second"].Value);
        var decimalText = match.Groups["decimal"].Success ? match.Groups["decimal"].Value : "";
        var decimalValue = string.IsNullOrWhiteSpace(decimalText) ? 0 : double.Parse("0" + decimalText);
        return minute * 60 + second + decimalValue;
    }

    private static string FormatTimeline(double seconds)
    {
        if (seconds < 0)
        {
            return "";
        }

        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
    }
}
