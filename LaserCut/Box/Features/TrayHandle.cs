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
        var band = Math.Max(h/5, model.Thickness * 2);

        var x0 = model.Width / 2;
        var y0 = h;
        var x1 = x0 - band;
        var y1 = y0 - band;

        var yb = 0; // model.Thickness;

        // The max radius of the handle is whichever is smaller, h/2 or the xO
        var r = Math.Min(h / 2, x0);

        var loop = new BoundaryLoop();
        var c = loop.GetCursor();

        // Start with the notch on the right side of the handle, drawing the inside L shape
        c.SegAbs(x0 - model.Thickness, yb);
        c.SegAbs(x0 - model.Thickness, h / 4);
        c.SegAbs(x0, h / 4);

        // We are now on the outside of the handle, tracing our way around the radius to the center
        c.SegAbs(x0, 0);
        c.ArcAbs(x0, y0 - r, x0 - r, y0 - r, false);

        // This should be moved inside the block?
        c.SegAbs(x0 - r, y0);

        // Now the other side
        if (x0 - r > GeometryConstants.DistEquals)
        {
            c.ArcAbs(-(x0 - r), y0, -(x0 - r), y0 - r, false);
        }

        // After the left arc, we are now heading down towards the left notch
        c.SegAbs(-x0, y0 - r);

        // The left notch
        c.SegAbs(-x0, h/4);
        c.SegAbs(-x0 + model.Thickness, h / 4);
        c.SegAbs(-x0 + model.Thickness, yb);

        // Around the corner to start the inside by heading back up
        c.SegAbs(-x1, yb);

        // If the inside of the handle is large enough for a radius, we insert them, otherwise we add two straight
        // segments to draw the interior of the handle
        if (y0 - r > y1 - GeometryConstants.DistEquals)
        {
            c.SegAbs(-x1, y1);
            c.SegAbs(x1, y1);
        }
        else
        {
            c.ArcAbs(-x1, y0 - r, -(x0 - r), y0 - r, true);
            c.SegAbs(-(x0 - r), y1);
            c.ArcAbs(x0 - r, y1, x0 - r, y0 - r, true);
        }

        // Head back down to the starting point, then head to the right to cap off the bottom face
        c.SegAbs(x1, y0 - r);
        c.SegAbs(x1, yb);

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
