using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 屏蔽词替换规则。
/// </summary>
public class ShieldReplacementRule : ObservableObject
{
    private string id = Guid.NewGuid().ToString("N");
    private string sourceText = "";
    private string replacementText = "";
    private int sortOrder;

    /// <summary>
    /// 规则 ID。
    /// </summary>
    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    /// <summary>
    /// 匹配文本。
    /// </summary>
    public string SourceText
    {
        get => sourceText;
        set => SetProperty(ref sourceText, value);
    }

    /// <summary>
    /// 替换文本。
    /// </summary>
    public string ReplacementText
    {
        get => replacementText;
        set => SetProperty(ref replacementText, value);
    }

    /// <summary>
    /// 排序序号。
    /// </summary>
    public int SortOrder
    {
        get => sortOrder;
        set => SetProperty(ref sortOrder, value);
    }
}
