using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class StackableFeet : IBoxFeature
{
    private readonly double _height;
    private readonly double _width;

    public StackableFeet(double height, double width)
    {
        _height = height;
        _width = width;
    }

    public void Operate(BoxModel model)
    {
        DoFeetEdge(model.Left.Bottom, model.Thickness);
        DoFeetEdge(model.Right.Bottom, model.Thickness);
        DoFeetEdge(model.Front.Bottom, model.Thickness);
        DoFeetEdge(model.Back.Bottom, model.Thickness);

        DoNotchEdge(model.Left.Top, model.Thickness);
        DoNotchEdge(model.Right.Top, model.Thickness);
        DoNotchEdge(model.Front.Top, model.Thickness);
        DoNotchEdge(model.Back.Top, model.Thickness);
    }

    private void DoFeetEdge(BoxEdge edge, double thk)
    {
        var t0 = LegTool(Width(edge.Previous, thk));
        edge.EnvelopeCursor.Operate(t0);

        var t1 = LegTool(Width(edge.Next, thk));
        edge.EnvelopeCursor.Operate(Flip(t1, edge.EnvelopeCursor.Length));
    }

    private double Width(BoxEdge edge, double thk)
    {
        return edge.HasPriority ? _width + thk : _width;
    }

    private BoundaryLoop Flip(BoundaryLoop loop, double length)
    {
        var copy = loop.Copy();
        copy.MirrorY();
        copy.Translate(0, length);
        copy.Reverse();

        return copy;
    }

    private BoundaryLoop LegTool(double w)
    {
        var tool = new BoundaryLoop();
        var cursor = tool.GetCursor();
        cursor.SegAbs(0, 0);
        cursor.SegRel(_height, 0);
        cursor.SegRel(0, w - _height);
        cursor.SegRel(-_height, _height);
        return tool;
    }

    private BoundaryLoop NotchTool(double w)
    {
        var t = LegTool(w);
        t.MirrorX();
        return t;
    }

    private void DoNotchEdge(BoxEdge edge, double thk)
    {
        var t0 = NotchTool(Width(edge.Previous, thk));
        edge.EnvelopeCursor.Operate(t0);

        var t1 = NotchTool(Width(edge.Next, thk));
        edge.EnvelopeCursor.Operate(Flip(t1, edge.EnvelopeCursor.Length));

    }
}
