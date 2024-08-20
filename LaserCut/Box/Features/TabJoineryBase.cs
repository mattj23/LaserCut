using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public abstract class TabJoineryBase : IBoxFeature
{
    protected readonly double Relief;

    protected TabJoineryBase(double relief)
    {
        Relief = relief;
    }

    public virtual void Operate(BoxModel model)
    {
        ApplyToEdge(model.Bottom.Top, model.Thickness);
        ApplyToEdge(model.Bottom.Left, model.Thickness);
        ApplyToEdge(model.Bottom.Right, model.Thickness);
        ApplyToEdge(model.Bottom.Bottom, model.Thickness);

        ApplyToEdge(model.Front.Right, model.Thickness);
        ApplyToEdge(model.Front.Left, model.Thickness);
        ApplyToEdge(model.Back.Right, model.Thickness);
        ApplyToEdge(model.Back.Left, model.Thickness);
    }

    protected abstract void DoEdge(BoxEdge pos, BoxEdge neg, double thickness);

    protected void PositiveTab(BoxEdgeCursor cursor, double yCenter, double tabLength, double thickness)
    {
        var tool = BoundaryLoop.Rectangle(0, yCenter - tabLength / 2, thickness, tabLength);
        cursor.Operate(tool);

        foreach (var corner in tool.IterItems())
        {
            cursor.RelieveAt(corner.Item.Point, Relief, 0.001);
        }
    }

    protected void NegativeTab(BoxEdgeCursor cursor, double yCenter, double tabLength, double thickness)
    {
        var tool = BoundaryLoop.Rectangle(-thickness, yCenter - tabLength / 2, thickness, tabLength).Reversed();
        cursor.Operate(tool);
        foreach (var corner in tool.IterItems())
        {
            cursor.RelieveAt(corner.Item.Point, Relief, 0.001);
        }
    }

    protected void ApplyToEdge(BoxEdge edge, double thickness)
    {
        var (pos, neg) = PositiveNegative(edge);
        DoEdge(pos, neg, thickness);
    }

    protected (BoxEdge, BoxEdge) PositiveNegative(BoxEdge edge)
    {
        return edge.HasPriority ? (edge.Neighbor, edge) : (edge, edge.Neighbor);
    }
}
