using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class TrayHandle : IBoxFeature
{
    private readonly double _height;
    private readonly double _width;

    public TrayHandle(double height, double width)
    {
        _height = height;
        _width = width;
    }

    public void Operate(BoxModel model)
    {
        // We're going to add the thickness of the material to the left and right faces
        AddSupportToEdge(model.Left.Left, model.Thickness);
        AddSupportToEdge(model.Right.Right, model.Thickness);

        // Check for two sides
        AddSupportToEdge(model.Left.Right, model.Thickness);
        AddSupportToEdge(model.Right.Left, model.Thickness);
    }


    private void AddSupportToEdge(BoxEdge edge, double thk)
    {
        var h = Math.Max(thk * 4, _height);

        var c = edge.EnvelopeCursor;

        // Add the thickness support
        var t0 = BoundaryLoop.Rectangle(0, 0, thk, c.Length);
        edge.EnvelopeCursor.Operate(t0);

        // Generate the cantilevered portion
        var t2 = new BoundaryLoop();
        var tc = t2.GetCursor();
        tc.SegAbs(0, 0);
        tc.SegRel(h/2, 0);
        tc.SegRel(0, thk * 2);
        var offset = h / 2 - thk;
        tc.SegRel(-offset / 2, 0);
        tc.SegRel(0, thk/2);
        tc.SegRel(-(offset/2 + 1e-4), offset / 2);

        // Generate the cutout
        var t3 = BoundaryLoop.Rectangle(h / 4, thk, h / 2, thk + 1e-4).Reversed();

        // if the end of the cursor is higher than the start, the end is up and we should flip it.
        if (c.EndWorld.Z > c.StartWorld.Z)
        {
            t2.MirrorY(c.Length / 2);
            t2.Reverse();

            t3.MirrorY(c.Length / 2);
            t3.Reverse();
        }

        edge.EnvelopeCursor.Operate(t2);
        edge.EnvelopeCursor.Operate(t3);
    }
}
