using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 歌词时间轴行。
/// </summary>
public class LyricLineState : ObservableObject
{
    private double timeSeconds;
    private string timeline = "";
    private string content = "";
    private string translatedContent = "";
    private double durationSeconds;
    private double progress;
    private bool isActive;
    private bool isSent;

    /// <summary>
    /// 时间轴秒数。
    /// </summary>
    public double TimeSeconds
    {
        get => timeSeconds;
        set => SetProperty(ref timeSeconds, value);
    }

    /// <summary>
    /// 时间轴文本。
    /// </summary>
    public string Timeline
    {
        get => timeline;
        set => SetProperty(ref timeline, value);
    }

    /// <summary>
    /// 歌词内容。
    /// </summary>
    public string Content
    {
        get => content;
        set => SetProperty(ref content, value);
    }

    /// <summary>
    /// 当前行翻译内容。
    /// </summary>
    public string TranslatedContent
    {
        get => translatedContent;
        set
        {
            if (SetProperty(ref translatedContent, value))
            {
                OnPropertyChanged(nameof(HasTranslatedContent));
            }
        }
    }

    /// <summary>
    /// 当前行是否包含翻译内容。
    /// </summary>
    public bool HasTranslatedContent => !string.IsNullOrWhiteSpace(TranslatedContent);

    /// <summary>
    /// 当前行播放时长。
    /// </summary>
    public double DurationSeconds
    {
        get => durationSeconds;
        set => SetProperty(ref durationSeconds, value);
    }

    /// <summary>
    /// 当前行播放进度。
    /// </summary>
    public double Progress
    {
        get => progress;
        set => SetProperty(ref progress, Math.Clamp(value, 0, 1));
    }

    /// <summary>
    /// 当前行播放进度百分比。
    /// </summary>
    public double ProgressPercent => Progress * 100;

    /// <summary>
    /// 是否为当前播放行。
    /// </summary>
    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }

    /// <summary>
    /// 当前播放会话内是否已经发送。
    /// </summary>
    public bool IsSent
    {
        get => isSent;
        set => SetProperty(ref isSent, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Progress))
        {
            OnPropertyChanged(nameof(ProgressPercent));
        }
    }
}
