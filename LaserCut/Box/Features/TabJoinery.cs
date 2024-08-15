using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class TabJoinery
{
    private readonly double _tabCount;

    public TabJoinery(double tabCount)
    {
        _tabCount = tabCount;
    }

    public void Operate(BoxModel model)
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

    private void ApplyToEdge(BoxEdge edge, double thickness)
    {
        var (pos, neg) = PositiveNegative(edge);

        var step = pos.SharedCursor.Length / _tabCount;

        for (int i = 0; i < _tabCount; i++)
        {
            var yCPos = i * step + step / 2;
            var y0Pos = yCPos - step / 4;

            // Positive
            var tool = BoundaryLoop.Rectangle(0, y0Pos, thickness, step / 2);
            pos.SharedCursor.Operate(tool);

            // Negative
            var yCNeg = neg.SharedY(yCPos);
            var y0Neg = yCNeg - step / 4;
            var toolNeg = BoundaryLoop.Rectangle(-thickness, y0Neg, thickness, step / 2).Reversed();
            neg.SharedCursor.Operate(toolNeg);
        }
    }

    private (BoxEdge, BoxEdge) PositiveNegative(BoxEdge edge)
    {
        if (edge.HasPriority)
        {
            return (edge.Neighbor, edge);
        }
        else
        {
            return (edge, edge.Neighbor);
        }
    }
}
