namespace Yohuke.DanmuNeo.Views.Components;

internal sealed class HoverMarqueeController
{
    private readonly TimeSpan startDelay;
    private readonly double scrollSpeed;
    private TimeSpan hoverDuration;

    internal HoverMarqueeController(TimeSpan startDelay, double scrollSpeed)
    {
        if (startDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(startDelay));
        }

        if (!double.IsFinite(scrollSpeed) || scrollSpeed <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scrollSpeed));
        }

        this.startDelay = startDelay;
        this.scrollSpeed = scrollSpeed;
    }

    internal double Offset { get; private set; }

    internal double MaxOffset { get; private set; }

    internal bool CanScroll => MaxOffset > 0;

    internal bool IsComplete => CanScroll && Offset >= MaxOffset;

    internal void UpdateViewport(double extentWidth, double viewportWidth)
    {
        if (!double.IsFinite(extentWidth) || !double.IsFinite(viewportWidth))
        {
            MaxOffset = 0;
            Offset = 0;
            return;
        }

        MaxOffset = Math.Max(0, extentWidth - viewportWidth);
        Offset = Math.Min(Offset, MaxOffset);
    }

    internal double Advance(TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero)
        {
            return Offset;
        }

        var previousHoverDuration = hoverDuration;
        hoverDuration += elapsed;

        if (!CanScroll || IsComplete)
        {
            return Offset;
        }

        var previousScrollDuration = Max(TimeSpan.Zero, previousHoverDuration - startDelay);
        var currentScrollDuration = Max(TimeSpan.Zero, hoverDuration - startDelay);
        var scrollDuration = currentScrollDuration - previousScrollDuration;
        Offset = Math.Min(MaxOffset, Offset + scrollDuration.TotalSeconds * scrollSpeed);
        return Offset;
    }

    internal void Reset()
    {
        hoverDuration = TimeSpan.Zero;
        Offset = 0;
        MaxOffset = 0;
    }

    private static TimeSpan Max(TimeSpan left, TimeSpan right)
    {
        return left >= right ? left : right;
    }
}
