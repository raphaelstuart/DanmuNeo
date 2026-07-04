using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 屏蔽词替换服务。
/// </summary>
public static class ShieldReplacementService
{
    /// <summary>
    /// 归一化屏蔽词替换规则。
    /// </summary>
    public static void Normalize(AppSettings settings)
    {
        settings.ShieldReplacementRules ??= [];
        settings.ShieldReplacementRules = settings.ShieldReplacementRules
            .OrderBy(rule => rule.SortOrder)
            .ThenBy(rule => rule.SourceText)
            .ToList();

        for (var index = 0; index < settings.ShieldReplacementRules.Count; index++)
        {
            var rule = settings.ShieldReplacementRules[index];

            if (string.IsNullOrWhiteSpace(rule.Id))
            {
                rule.Id = Guid.NewGuid().ToString("N");
            }

            rule.SortOrder = index;
        }
    }

    /// <summary>
    /// 应用屏蔽词替换规则。
    /// </summary>
    public static string Apply(string text, AppSettings settings, bool isLyric)
    {
        if (!settings.EnableShieldProcessing ||
            isLyric && !settings.ApplyShieldReplacementToLyrics ||
            string.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = text;

        foreach (var rule in settings.ShieldReplacementRules.OrderBy(rule => rule.SortOrder))
        {
            if (string.IsNullOrEmpty(rule.SourceText))
            {
                continue;
            }

            result = result.Replace(rule.SourceText, rule.ReplacementText ?? "", StringComparison.Ordinal);
        }

        return result;
    }
}
