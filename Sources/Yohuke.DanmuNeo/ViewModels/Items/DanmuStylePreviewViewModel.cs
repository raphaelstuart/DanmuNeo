using System.Collections.ObjectModel;
using Avalonia.Media;
using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.ViewModels.Items;

internal sealed class DanmuStylePreviewViewModel : ViewModelBase
{
    private string userName = "样式测试用户";
    private string content = "这是一条用于检查字号、间距和自动换行的测试消息。";
    private string danmuStatus = "";
    private string superChatColorText = "";
    private bool isCompactDanmuDisplay;
    private SuperChatStylePreset? selectedSuperChatPreset;

    public DanmuStylePreviewViewModel()
    {
        SuperChatPresets =
        [
            CreatePreset(30, 300, "蓝色", "#3171D2"),
            CreatePreset(50, 500, "青蓝", "#427D9E"),
            CreatePreset(100, 1000, "金色", "#E2B52B"),
            CreatePreset(500, 5000, "橙色", "#E09443"),
            CreatePreset(1000, 10000, "红色", "#E54D4D"),
            CreatePreset(2000, 20000, "深红", "#AB1A32")
        ];
        SelectedSuperChatPreset = SuperChatPresets[0];
        ResetSamples();
    }

    public ObservableCollection<SuperChatItem> SuperChats { get; } = [];

    public ObservableCollection<DanmuFeedItem> DanmuItems { get; } = [];

    public IReadOnlyList<SuperChatStylePreset> SuperChatPresets { get; }

    public bool IsCompactDanmuDisplay
    {
        get => isCompactDanmuDisplay;
        set => SetProperty(ref isCompactDanmuDisplay, value);
    }

    public string UserName
    {
        get => userName;
        set => SetProperty(ref userName, value);
    }

    public string Content
    {
        get => content;
        set => SetProperty(ref content, value);
    }

    public string DanmuStatus
    {
        get => danmuStatus;
        set => SetProperty(ref danmuStatus, value);
    }

    public string SuperChatColorText
    {
        get => superChatColorText;
        set => SetProperty(ref superChatColorText, value);
    }

    public SuperChatStylePreset? SelectedSuperChatPreset
    {
        get => selectedSuperChatPreset;
        set
        {
            if (!SetProperty(ref selectedSuperChatPreset, value) || value is null)
            {
                return;
            }

            SuperChatColorText = value.BorderColor.ToString();
        }
    }

    internal void AddDanmu()
    {
        DanmuItems.Insert(0, new()
        {
            Time = DateTimeOffset.Now,
            UserName = NormalizeUserName(UserName),
            UserUid = "debug-user",
            Content = NormalizeContent(Content),
            Status = DanmuStatus.Trim()
        });
    }

    internal void AddSuperChat()
    {
        if (SelectedSuperChatPreset is not { } preset)
        {
            return;
        }

        var borderColor = Color.TryParse(SuperChatColorText, out var customColor)
            ? customColor
            : preset.BorderColor;
        SuperChats.Insert(0, CreateSuperChatItem(
            preset,
            NormalizeUserName(UserName),
            NormalizeContent(Content),
            DateTimeOffset.Now,
            borderColor));
    }

    internal void ClearSamples()
    {
        SuperChats.Clear();
        DanmuItems.Clear();
    }

    internal void ResetSamples()
    {
        ClearSamples();
        var now = DateTimeOffset.Now;

        for (var index = 0; index < SuperChatPresets.Count; index++)
        {
            var preset = SuperChatPresets[index];
            SuperChats.Add(CreateSuperChatItem(
                preset,
                $"{preset.ColorName}档用户",
                $"这是 {preset.PriceText} 档醒目留言，用于检查边框、金额和长文本换行效果。",
                now.AddMinutes(-index),
                preset.BorderColor));
        }

        DanmuItems.Add(new()
        {
            Time = now,
            UserName = "短名",
            UserUid = "10001",
            Content = "普通短弹幕"
        });
        DanmuItems.Add(new()
        {
            Time = now.AddSeconds(-8),
            UserName = "这是一个很长很长的用户名",
            UserUid = "10002",
            Content = "这是一条很长的弹幕，用于检查预览区域较窄时用户名裁切以及正文自动换行是否清晰自然。"
        });
        DanmuItems.Add(new()
        {
            Time = now.AddSeconds(-16),
            UserName = "Emoji 用户 🎉",
            UserUid = "10003",
            Content = "中日文、English、数字 12345 与 Emoji ✨🚀 混排测试。"
        });
        DanmuItems.Add(new()
        {
            Time = now.AddSeconds(-24),
            UserName = "状态测试",
            UserUid = "10004",
            Content = "带有状态文本的弹幕。",
            Status = "已转发"
        });
    }

    private static SuperChatStylePreset CreatePreset(
        decimal cnyPrice,
        int batteryAmount,
        string colorName,
        string colorText)
    {
        return new()
        {
            CnyPrice = cnyPrice,
            BatteryAmount = batteryAmount,
            ColorName = colorName,
            BorderColor = Color.Parse(colorText)
        };
    }

    private static SuperChatItem CreateSuperChatItem(
        SuperChatStylePreset preset,
        string userName,
        string content,
        DateTimeOffset time,
        Color borderColor)
    {
        return new()
        {
            MessageId = $"debug-{preset.BatteryAmount}-{time.ToUnixTimeMilliseconds()}",
            Time = time,
            UserName = userName,
            PriceText = preset.PriceText,
            BorderColor = borderColor,
            Content = content
        };
    }

    private static string NormalizeUserName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "测试用户" : value.Trim();
    }

    private static string NormalizeContent(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "空内容样式测试" : value.Trim();
    }
}
