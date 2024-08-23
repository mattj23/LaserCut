using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class TrayHandle : IBoxFeature
{
    private readonly double _length;
    private readonly double _inset;
    private readonly bool _bothSides;
    private readonly BodyIdManager _idManager;

    public TrayHandle(double length, double inset, bool bothSides, BodyIdManager idManager)
    {
        _length = length;
        _inset = inset;
        _bothSides = bothSides;
        _idManager = idManager;
    }

    public void Operate(BoxModel model)
    {
        // We're going to add the thickness of the material to the left and right faces
        AddSupportToEdge(model.Left.Left, model.Thickness);
        AddSupportToEdge(model.Right.Right, model.Thickness);
        HandleBody(model);

        // Check for two sides
        if (_bothSides)
        {
            AddSupportToEdge(model.Left.Right, model.Thickness);
            AddSupportToEdge(model.Right.Left, model.Thickness);
            HandleBody(model);
        }
    }


    private void HandleBody(BoxModel model)
    {
        var bodyId = _idManager.GetNextId();

        // The length (h) of the handle
        var h = Math.Max(model.Thickness * 4, _length);

        // The band thickness of the handle must be at least 2x the thickness of the material
        var band = model.Thickness * 2;

        var x0 = model.Width / 2;
        var y0 = h;
        var x1 = x0 - band;
        var y1 = y0 - band;

        // The max radius of the handle is whichever is smaller, h/2 or the xO
        var r = Math.Min(h / 2, x0);

        var loop = new BoundaryLoop();
        var c = loop.GetCursor();

        c.SegAbs(x0, 0);
        c.ArcAbs(x0, y0 - r, x0 - r, y0 - r, false);
        c.SegAbs(x0 - r, y0);
        if (x0 - r > GeometryConstants.DistEquals)
        {
            c.ArcAbs(-(x0 - r), y0, -(x0 - r), y0 - r, false);
        }

        c.SegAbs(-x0, y0 - r);
        c.SegAbs(-x0, 0);

        loop.RemoveZeroLengthElements();
        loop.RemoveAdjacentRedundancies();

        model.ExtraBodies.Add(bodyId, new Body(loop));
    }

    private void AddSupportToEdge(BoxEdge edge, double thk)
    {
        var h = Math.Max(thk * 4, _length);

        var c = edge.EnvelopeCursor;

        // Add the thickness support
        var t0 = BoundaryLoop.Rectangle(0, 0, thk, c.Length);
        edge.EnvelopeCursor.Operate(t0);

        // Generate the cantilevered portion
        var t2 = new BoundaryLoop();
        var tc = t2.GetCursor();
        tc.SegAbs(0, _inset);
        tc.SegRel(h/2, 0);
        tc.SegRel(0, thk * 2);
        var offset = h / 2 - thk;
        tc.SegRel(-offset / 2, 0);
        tc.SegRel(0, thk/2);
        tc.SegRel(-(offset/2 + 1e-4), offset / 2);

        // Generate the cutout
        var t3 = BoundaryLoop.Rectangle(h / 4, thk + _inset, h / 2, thk + 1e-4).Reversed();

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
