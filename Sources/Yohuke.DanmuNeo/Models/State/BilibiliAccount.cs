using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// B 站账号配置。
/// </summary>
public partial class BilibiliAccount : ObservableObject
{
    private string id = Guid.NewGuid().ToString("N");
    private string name = "账号";
    private string cookie = "";
    private bool isGlobalDefault;

    /// <summary>
    /// 账号 ID。
    /// </summary>
    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    /// <summary>
    /// 账号名称。
    /// </summary>
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    /// <summary>
    /// Cookie 文本。
    /// </summary>
    public string Cookie
    {
        get => cookie;
        set
        {
            if (SetProperty(ref cookie, value))
            {
                OnPropertyChanged(nameof(CookieStatus));
                OnPropertyChanged(nameof(CookieSummary));
            }
        }
    }

    /// <summary>
    /// Cookie 绑定状态。
    /// </summary>
    public string CookieStatus => string.IsNullOrWhiteSpace(Cookie) ? "未绑定" : "已绑定";

    /// <summary>
    /// Cookie 摘要。
    /// </summary>
    public string CookieSummary => string.IsNullOrWhiteSpace(Cookie)
        ? "尚未配置 Cookie"
        : $"Cookie 尾段 {Cookie[^Math.Min(8, Cookie.Length)..]}";

    /// <summary>
    /// 是否为全局默认账号。
    /// </summary>
    public bool IsGlobalDefault
    {
        get => isGlobalDefault;
        set => SetProperty(ref isGlobalDefault, value);
    }
}
