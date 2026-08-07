using Avalonia.Media;
using Yohuke.DanmuNeo.ViewModels.Items;

namespace Yohuke.DanmuNeo.Tests;

public class DanmuStylePreviewViewModelTests
{
    [Fact]
    public void CompactDanmuDisplayRaisesNotification()
    {
        var viewModel = new DanmuStylePreviewViewModel();
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        viewModel.IsCompactDanmuDisplay = true;

        Assert.True(viewModel.IsCompactDanmuDisplay);
        Assert.Contains(nameof(DanmuStylePreviewViewModel.IsCompactDanmuDisplay), changedProperties);
    }

    [Fact]
    public void ConstructorCreatesAllSuperChatTiersAndDanmuVariants()
    {
        var viewModel = new DanmuStylePreviewViewModel();

        Assert.Equal(6, viewModel.SuperChatPresets.Count);
        Assert.Equal(6, viewModel.SuperChats.Count);
        Assert.Equal(4, viewModel.DanmuItems.Count);
        Assert.Equal("300 电池", viewModel.SuperChats[0].PriceText);
        Assert.Equal(Color.Parse("#3171D2"), viewModel.SuperChats[0].BorderColor);
        Assert.Equal("20000 电池", viewModel.SuperChats[^1].PriceText);
    }

    [Fact]
    public void AddDanmuUsesEditableFieldsAndFallbacks()
    {
        var viewModel = new DanmuStylePreviewViewModel
        {
            UserName = "  ",
            Content = "  ",
            DanmuStatus = " 已转发 "
        };

        viewModel.AddDanmu();

        var item = viewModel.DanmuItems[0];
        Assert.Equal("测试用户", item.UserName);
        Assert.Equal("空内容样式测试", item.Content);
        Assert.Equal("已转发", item.Status);
    }

    [Fact]
    public void AddSuperChatUsesBatteryAmountAndCustomColor()
    {
        var viewModel = new DanmuStylePreviewViewModel
        {
            SelectedSuperChatPreset = null
        };
        viewModel.SelectedSuperChatPreset = viewModel.SuperChatPresets[2];
        viewModel.SuperChatColorText = "#123456";

        viewModel.AddSuperChat();

        var item = viewModel.SuperChats[0];
        Assert.Equal("1000 电池", item.PriceText);
        Assert.Equal(Color.Parse("#123456"), item.BorderColor);
    }

    [Fact]
    public void ClearSamplesClearsBothFeeds()
    {
        var viewModel = new DanmuStylePreviewViewModel();

        viewModel.ClearSamples();

        Assert.Empty(viewModel.SuperChats);
        Assert.Empty(viewModel.DanmuItems);
    }
}
