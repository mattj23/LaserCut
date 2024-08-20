namespace LaserCut.Box.Features;

public class SpacedTabJoinery : TabJoineryBase
{
    private readonly double _width;
    private readonly double _gap;

    public SpacedTabJoinery(double width, double gap, double relief)
        : base(relief)
    {
        _width = width;
        _gap = gap;
    }

    protected override void DoEdge(BoxEdge pos, BoxEdge neg, double thickness)
    {
        // Get the overall length of the edge
        var l = pos.SharedCursor.Length;

        // Get the tab width and the expected empty space per tab
        var w = _width;
        var t = w + _gap;

        var count = (int)(l / t);
        var step = l / count;

        for (var i = 0; i < count; i++)
        {
            var yc = i * step + step / 2;
            PositiveTab(pos.SharedCursor, yc, w, thickness);
            NegativeTab(neg.SharedCursor, neg.SharedY(yc), w, thickness);
        }
    }
}
