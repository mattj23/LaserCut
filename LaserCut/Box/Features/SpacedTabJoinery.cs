namespace LaserCut.Box.Features;

public class SpacedTabJoinery : TabJoineryBase
{
    private readonly double _thicknessFraction;
    private readonly double _balanceFraction;

    public SpacedTabJoinery(double thicknessFraction, double balanceFraction)
    {
        _thicknessFraction = thicknessFraction;
        _balanceFraction = balanceFraction;
    }

    protected override void DoEdge(BoxEdge pos, BoxEdge neg, double thickness)
    {
        // Get the overall length of the edge
        var l = pos.SharedCursor.Length;

        // Get the tab width and the expected empty space per tab
        var w = thickness * _thicknessFraction;
        var s = w * (1 - _balanceFraction) / _balanceFraction;
        var t = w + s;

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
