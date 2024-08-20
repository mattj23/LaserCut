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
}
