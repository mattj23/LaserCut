using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class LidTabs : TabJoineryBase
{
    private readonly double _margin;
    private readonly double _chamfer;

    public LidTabs(double margin, double relief, double chamfer) : base(relief)
    {
        _margin = margin;
        _chamfer = chamfer;
    }

    public override void Operate(BoxModel model)
    {
        if (model.HasLid)
        {
            ApplyToEdge(model.Top.Top, model.Thickness);
            ApplyToEdge(model.Top.Left, model.Thickness);
            ApplyToEdge(model.Top.Right, model.Thickness);
            ApplyToEdge(model.Top.Bottom, model.Thickness);
        }
    }

    protected override void DoEdge(BoxEdge pos, BoxEdge neg, double thickness)
    {
        var length = pos.SharedCursor.Length - _margin * 2;
        if (length < 0)
        {
            return;
        }
        var gap = 0.005 * 25.4;

        PositiveTab(pos.SharedCursor, pos.SharedCursor.Length / 2, length - gap, thickness);
        NegativeTab(neg.SharedCursor, neg.SharedCursor.Length / 2, length, thickness);

    }

    protected override void AfterNegativeTab(BoxEdgeCursor cursor, double yCenter, double tabLength, double thickness,
        BoundaryLoop tool)
    {
        // If we have a chamfer, we will add it only on the negative tabs
        if (_chamfer <= GeometryConstants.DistEquals) return;

        var y0 = yCenter - tabLength / 2;
        var xc = _chamfer;
        var yc = _chamfer * 0.625;

        var t0 = new BoundaryLoop();
        var c0 = t0.GetCursor();
        c0.SegAbs(0, y0);
        c0.SegRel(0, -yc);
        c0.SegRel(-xc, yc);

        cursor.Operate(t0);

        var t1 = new BoundaryLoop();
        var c1 = t1.GetCursor();
        c1.SegAbs(0, y0 + tabLength);
        c1.SegRel(-xc, 0);
        c1.SegRel(xc, yc);

        cursor.Operate(t1);
    }
}
