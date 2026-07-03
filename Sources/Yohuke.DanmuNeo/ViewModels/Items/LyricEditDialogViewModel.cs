using CommunityToolkit.Mvvm.ComponentModel;
using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.ViewModels.Items;

/// <summary>
/// 歌词编辑弹窗视图模型。
/// </summary>
public class LyricEditDialogViewModel : ObservableObject
{
    private string errorText = "";

    /// <summary>
    /// 初始化歌词编辑弹窗视图模型。
    /// </summary>
    public LyricEditDialogViewModel(LyricLibraryItem item)
    {
        Title = item.Title;
        Artist = item.Artist;
        Album = item.Album;
        Tags = item.Tags;
        LyricText = item.LyricText;
        TranslatedLyricText = item.TranslatedLyricText;
    }

    /// <summary>
    /// 歌曲名。
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// 歌手。
    /// </summary>
    public string Artist { get; set; } = "";

    /// <summary>
    /// 专辑。
    /// </summary>
    public string Album { get; set; } = "";

    /// <summary>
    /// 标签。
    /// </summary>
    public string Tags { get; set; } = "";

    /// <summary>
    /// 原文歌词。
    /// </summary>
    public string LyricText { get; set; } = "";

    /// <summary>
    /// 翻译歌词。
    /// </summary>
    public string TranslatedLyricText { get; set; } = "";

    /// <summary>
    /// 错误提示。
    /// </summary>
    public string ErrorText
    {
        get => errorText;
        set
        {
            if (SetProperty(ref errorText, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    /// <summary>
    /// 是否存在错误。
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

    /// <summary>
    /// 写回歌词库条目。
    /// </summary>
    public void ApplyTo(LyricLibraryItem item)
    {
        item.Title = Title.Trim();
        item.Artist = Artist.Trim();
        item.Album = Album.Trim();
        item.Tags = Tags.Trim();
        item.LyricText = LyricText;
        item.TranslatedLyricText = TranslatedLyricText;
        item.HasTranslation = !string.IsNullOrWhiteSpace(TranslatedLyricText);
        item.UpdatedAt = DateTimeOffset.Now;
    }
}
