using CommunityToolkit.Mvvm.ComponentModel;
using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.ViewModels.Items;

/// <summary>
/// 弹幕转发规则视图模型。
/// </summary>
public partial class DanmuForwardRuleViewModel : ViewModelBase
{
    private readonly Action<DanmuForwardRuleViewModel> onChanged;

    /// <summary>
    /// 初始化弹幕转发规则视图模型。
    /// </summary>
    public DanmuForwardRuleViewModel(DanmuForwardRuleState state, Action<DanmuForwardRuleViewModel> onChanged)
    {
        State = state;
        this.onChanged = onChanged;
        statusText = state.IsEnabled ? "等待启动" : "未启用";
    }

    /// <summary>
    /// 转发规则状态。
    /// </summary>
    public DanmuForwardRuleState State { get; }

    /// <summary>
    /// 规则 ID。
    /// </summary>
    public string Id => State.Id;

    /// <summary>
    /// 源直播间选项键。
    /// </summary>
    public string? SourceRoomKey
    {
        get => string.IsNullOrWhiteSpace(State.SourceWorkspaceId) || string.IsNullOrWhiteSpace(State.SourceRoomStateId)
            ? ""
            : $"{State.SourceWorkspaceId}|{State.SourceRoomStateId}";
        set
        {
            var normalizedValue = value ?? "";

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                return;
            }

            if (SourceRoomKey == normalizedValue)
            {
                return;
            }

            var segments = normalizedValue.Split('|', 2);
            State.SourceWorkspaceId = segments.Length > 0 ? segments[0] : "";
            State.SourceRoomStateId = segments.Length > 1 ? segments[1] : "";
            OnPropertyChanged();
            onChanged(this);
        }
    }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled
    {
        get => State.IsEnabled;
        set
        {
            if (State.IsEnabled == value)
            {
                return;
            }

            State.IsEnabled = value;
            OnPropertyChanged();
            onChanged(this);
        }
    }

    /// <summary>
    /// 转发时使用的符号组 ID。
    /// </summary>
    public string MarkSymbolGroupId
    {
        get => State.MarkSymbolGroupId;
        set
        {
            if (State.MarkSymbolGroupId == value)
            {
                return;
            }

            State.MarkSymbolGroupId = value;
            OnPropertyChanged();
            onChanged(this);
        }
    }

    /// <summary>
    /// 转发发送时指定的账号 ID。
    /// </summary>
    public string AccountOverrideId
    {
        get => State.AccountOverrideId ?? "";
        set
        {
            var normalizedValue = value ?? "";

            if (State.AccountOverrideId == normalizedValue)
            {
                return;
            }

            State.AccountOverrideId = normalizedValue;
            OnPropertyChanged();
            onChanged(this);
        }
    }

    /// <summary>
    /// 监听发送人 UID。
    /// </summary>
    public string SenderUid
    {
        get => State.SenderUid;
        set
        {
            if (State.SenderUid == value)
            {
                return;
            }

            State.SenderUid = value;
            OnPropertyChanged();
            onChanged(this);
        }
    }

    /// <summary>
    /// 同传格式正则。
    /// </summary>
    public string ContentPattern
    {
        get => State.ContentPattern;
        set
        {
            if (State.ContentPattern == value)
            {
                return;
            }

            State.ContentPattern = value;
            OnPropertyChanged();
            onChanged(this);
        }
    }

    [ObservableProperty] private string statusText;
}
