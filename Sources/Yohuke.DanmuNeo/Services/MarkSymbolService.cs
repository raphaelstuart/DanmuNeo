using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 管理同传与歌词符号组。
/// </summary>
public class MarkSymbolService
{
    /// <summary>
    /// 创建默认符号组。
    /// </summary>
    public static MarkSymbolGroup CreateDefaultGroup(AppSettings settings)
    {
        return new()
        {
            Name = "默认符号",
            TranslateOpenMark = string.IsNullOrEmpty(settings.TranslateOpenMark) ? "【" : settings.TranslateOpenMark,
            TranslateCloseMark = string.IsNullOrEmpty(settings.TranslateCloseMark) ? "】" : settings.TranslateCloseMark,
            LyricOpenMark = string.IsNullOrEmpty(settings.LyricOpenMark) ? "【♪" : settings.LyricOpenMark,
            LyricCloseMark = string.IsNullOrEmpty(settings.LyricCloseMark) ? "】" : settings.LyricCloseMark
        };
    }

    /// <summary>
    /// 归一化符号组配置。
    /// </summary>
    public static void Normalize(AppSettings settings)
    {
        settings.MarkGroups ??= [];

        if (settings.MarkGroups.Count == 0)
        {
            settings.MarkGroups.Add(CreateDefaultGroup(settings));
        }

        foreach (var group in settings.MarkGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Id))
            {
                group.Id = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(group.Name))
            {
                group.Name = "未命名符号";
            }
        }

        settings.MarkGroups = settings.MarkGroups
            .OrderBy(group => group.SortOrder)
            .ThenBy(group => group.Name)
            .ToList();

        for (var index = 0; index < settings.MarkGroups.Count; index++)
        {
            settings.MarkGroups[index].SortOrder = index;
        }

        var defaultGroup = settings.MarkGroups[0];
        settings.TranslateOpenMark = defaultGroup.TranslateOpenMark;
        settings.TranslateCloseMark = defaultGroup.TranslateCloseMark;
        settings.LyricOpenMark = defaultGroup.LyricOpenMark;
        settings.LyricCloseMark = defaultGroup.LyricCloseMark;
    }

    /// <summary>
    /// 解析工作区正在使用的符号组。
    /// </summary>
    public MarkSymbolGroup Resolve(AppSettings settings, WorkspaceState? workspace)
    {
        Normalize(settings);
        var groupId = workspace?.SelectedMarkGroupId;

        if (!string.IsNullOrWhiteSpace(groupId))
        {
            var group = settings.MarkGroups.FirstOrDefault(item => item.Id == groupId);

            if (group is not null)
            {
                return group;
            }
        }

        return settings.MarkGroups[0];
    }
}
