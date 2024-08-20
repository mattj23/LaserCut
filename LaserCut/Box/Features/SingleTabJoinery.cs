using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class SingleTabJoinery : TabJoineryBase
{
    private readonly double _tabWidth;

    public SingleTabJoinery(double tabWidth, double relief) : base(relief)
    {
        _tabWidth = tabWidth;
    }

    protected override void DoEdge(BoxEdge pos, BoxEdge neg, double thickness)
    {
        if (pos.SharedCursor.Length < _tabWidth * 1.1)
        {
            return;
        }

        var center = pos.SharedCursor.Length / 2;
        PositiveTab(pos.SharedCursor, center, _tabWidth, thickness);
        NegativeTab(neg.SharedCursor, neg.SharedY(center), _tabWidth, thickness);
    }
}
