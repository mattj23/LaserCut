using System.Net.Mail;
using LaserCut.Geometry;

namespace LaserCut.Box.Features;

public class TrayHandle : IBoxFeature
{
    private readonly double _length;
    private readonly double _inset;
    private readonly bool _bothSides;
    private readonly bool _doubleHandle;
    private readonly BodyIdManager _idManager;

    public TrayHandle(double length, double inset, bool bothSides, BodyIdManager idManager, bool doubleHandle)
    {
        _length = length;
        _inset = inset;
        _bothSides = bothSides;
        _idManager = idManager;
        _doubleHandle = doubleHandle;
    }

    public void Operate(BoxModel model)
    {
        // We're going to add the thickness of the material to the left and right faces
        AddSupportToEdge(model.Left.Left, model.Thickness);
        AddSupportToEdge(model.Right.Right, model.Thickness);
        HandleBody(model);
        if (_doubleHandle) HandleBody(model);
        CutNotchesInEndFaces(model.Front, model.Thickness);

        // Check for two sides
        if (_bothSides)
        {
            AddSupportToEdge(model.Left.Right, model.Thickness);
            AddSupportToEdge(model.Right.Left, model.Thickness);
            HandleBody(model);
            if (_doubleHandle) HandleBody(model);
            CutNotchesInEndFaces(model.Back, model.Thickness);
        }
    }

    private double ScaleThk(double t)
    {
        return _doubleHandle ? t * 2 : t;
    }

    private void CutNotchesInEndFaces(BoxFace face, double thickness)
    {
        // The notches are down the inset from the top of the face by the thickness of the material and the inset
        // value.

        var c = face.Top.EnvelopeCursor;
        var inset = _inset + thickness + ScaleThk(thickness);
        var h = thickness * 2;

        var tool0 = BoundaryLoop.Rectangle(-inset, 0, ScaleThk(thickness), h);
        tool0.Reverse();
        c.Operate(tool0);

        var tool1 = BoundaryLoop.Rectangle(-inset, c.Length - h, ScaleThk(thickness), h);
        tool1.Reverse();
        c.Operate(tool1);
    }

    private double HandleLen(double thk)
    {
        return Math.Max(thk * 4, _length);
    }

    private void HandleBody(BoxModel model)
    {
        var bodyId = _idManager.GetNextId();

        // Some convenience variables
        var thk = model.Thickness;
        var thk2 = thk * 2;

        // The length (h) of the handle
        var h = HandleLen(thk);

        // The band thickness of the handle must be at least 2x the thickness of the material
        var band = Math.Max(h/5, thk2);

        // x0, y0 are the top right corner of the handle envelope, such that x0 is at the furthest right edge and
        // y0 is at the top edge.
        var x0 = model.Width / 2;
        var y0 = h;

        // x1 and y1 are the internal position of the theoretical top right corner of the handle, such that x1 is at
        // the inside right edge and y1 is at the inside top edge.
        var x1 = x0 - band;
        var y1 = y0 - band;

        // yb is the position of the bottom most edge of the handle, which should be far enough that it protrudes all
        // the way back into the adjacent space to create a mating tab.
        var yb = -model.Thickness;

        // The max radius of the handle is whichever is smaller, h/2 or the xO
        var r = Math.Min(h / 2, x0);

        var loop = new BoundaryLoop();
        var c = loop.GetCursor();

        // Start with the notch on the right side of the handle, drawing the inside L shape.  The notch ends at 2x the
        // material thickness from the face of the adjacent surface.
        c.SegAbs(x0 - thk, yb);
        c.SegAbs(x0 - thk, thk2);
        c.SegAbs(x0, thk2);

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
        c.SegAbs(-x0, thk2);
        c.SegAbs(-x0 + thk, thk2);
        c.SegAbs(-x0 + thk, yb);

        // Bottom face of the left tab
        c.SegAbs(-x1 + thk, yb);

        // The left tab geometry and chamfer back into the handle
        c.SegRel(0, thk);
        c.SegRel(-thk, thk);

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

        // End the arc
        c.SegAbs(x1, y0 - r);

        // Head back down to the starting point, interrupting at the tab chamfer
        c.SegAbs(x1, yb + thk2);
        c.SegRel(-thk, -thk);
        c.SegRel(0, -thk);

        // Bottom face of the right tab
        c.SegAbs(x1, yb);

        loop.RemoveZeroLengthElements();
        loop.RemoveAdjacentRedundancies();

        model.ExtraBodies.Add(bodyId, new Body(loop));
    }

    private void AddSupportToEdge(BoxEdge edge, double thk)
    {
        // In this case, h refers to the distance that the handle extends beyond the front face of the box. It's the
        // height of the handle silhouette.
        var h = HandleLen(thk);

        var c = edge.EnvelopeCursor;

        // Generate the cantilevered portion
        var tool = new BoundaryLoop();
        var tc = tool.GetCursor();

        // The first segment starts at the inset and goes out half the extension of the handle.
        tc.SegAbs(0, _inset);
        tc.SegAbs(h/2, _inset);

        // Now we go down by the thickness of the material to make it to the top of the notch.
        tc.SegAbs(h/2, _inset + thk);

        // Now we go in to the notch offset. This is 2x the thickness of the material.
        tc.SegAbs(2 * thk, _inset + thk);

        // Now we go down to the bottom of the notch
        tc.SegRel(0, ScaleThk(thk));

        // We come back out by one material thickness and then down by one half of the material thickness
        tc.SegRel(thk, 0);
        tc.SegRel(0, thk/2);

        // We are now 3 material thicknesses from the edge, and we want to get down to one at a 45 degree chamfer
        tc.SegRel(-2 * thk, 2 * thk);

        // Finally, we go all the way to the other end and cap off a total support which is 1 material thickness
        tc.SegAbs(thk, c.Length);
        tc.SegAbs(0, c.Length);

        // if the end of the cursor is higher than the start, the end is up and we should flip it.
        if (c.EndWorld.Z > c.StartWorld.Z)
        {
            tool.MirrorY(c.Length / 2);
            tool.Reverse();
        }

        edge.EnvelopeCursor.Operate(tool);
    }
}
