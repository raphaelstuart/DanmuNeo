using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 同传与歌词发送符号组。
/// </summary>
public partial class MarkSymbolGroup : ObservableObject
{
    private string id = Guid.NewGuid().ToString("N");
    private string name = "默认符号";
    private int sortOrder;
    private bool isExpanded = true;
    private string translateOpenMark = "【";
    private string translateCloseMark = "】";
    private string lyricOpenMark = "【♪";
    private string lyricCloseMark = "】";

    /// <summary>
    /// 符号组 ID。
    /// </summary>
    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    /// <summary>
    /// 符号组名称。
    /// </summary>
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    /// <summary>
    /// 排序序号。
    /// </summary>
    public int SortOrder
    {
        get => sortOrder;
        set => SetProperty(ref sortOrder, value);
    }

    /// <summary>
    /// 是否展开编辑区域。
    /// </summary>
    public bool IsExpanded
    {
        get => isExpanded;
        set => SetProperty(ref isExpanded, value);
    }

    /// <summary>
    /// 同传开标记。
    /// </summary>
    public string TranslateOpenMark
    {
        get => translateOpenMark;
        set => SetProperty(ref translateOpenMark, value);
    }

    /// <summary>
    /// 同传闭标记。
    /// </summary>
    public string TranslateCloseMark
    {
        get => translateCloseMark;
        set => SetProperty(ref translateCloseMark, value);
    }

    /// <summary>
    /// 歌词开标记。
    /// </summary>
    public string LyricOpenMark
    {
        get => lyricOpenMark;
        set => SetProperty(ref lyricOpenMark, value);
    }

    /// <summary>
    /// 歌词闭标记。
    /// </summary>
    public string LyricCloseMark
    {
        get => lyricCloseMark;
        set => SetProperty(ref lyricCloseMark, value);
    }
}
