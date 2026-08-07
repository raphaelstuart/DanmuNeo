using Yohuke.DanmuNeo.Views.Components;

namespace Yohuke.DanmuNeo.Tests;

public class HoverMarqueeControllerTests
{
    private static readonly TimeSpan START_DELAY = TimeSpan.FromMilliseconds(350);
    private const double SCROLL_SPEED = 60;

    [Fact]
    public void AdvanceDoesNotMoveWhenContentFitsViewport()
    {
        var controller = new HoverMarqueeController(START_DELAY, SCROLL_SPEED);
        controller.UpdateViewport(100, 100);

        var offset = controller.Advance(TimeSpan.FromSeconds(2));

        Assert.False(controller.CanScroll);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void AdvanceWaitsThenMovesAtConfiguredSpeedAndStopsAtEnd()
    {
        var controller = new HoverMarqueeController(START_DELAY, SCROLL_SPEED);
        controller.UpdateViewport(200, 100);

        Assert.Equal(0, controller.Advance(START_DELAY));
        Assert.Equal(60, controller.Advance(TimeSpan.FromSeconds(1)), 3);
        Assert.Equal(100, controller.Advance(TimeSpan.FromSeconds(1)), 3);
        Assert.True(controller.IsComplete);
    }

    [Fact]
    public void UpdateViewportClampsCurrentOffsetToNewEnd()
    {
        var controller = new HoverMarqueeController(START_DELAY, SCROLL_SPEED);
        controller.UpdateViewport(200, 100);
        controller.Advance(START_DELAY + TimeSpan.FromSeconds(1));

        controller.UpdateViewport(120, 100);

        Assert.Equal(20, controller.Offset, 3);
        Assert.True(controller.IsComplete);
    }

    [Fact]
    public void ResetClearsProgressAndViewport()
    {
        var controller = new HoverMarqueeController(START_DELAY, SCROLL_SPEED);
        controller.UpdateViewport(200, 100);
        controller.Advance(START_DELAY + TimeSpan.FromSeconds(1));

        controller.Reset();

        Assert.Equal(0, controller.Offset);
        Assert.Equal(0, controller.MaxOffset);
        Assert.False(controller.CanScroll);
    }
}
