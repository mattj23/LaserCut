using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public static class OffsetIntersections
{
    public static Point2D Make(Segment seg, Arc arc, double distance)
    {
        // We move to the modified coordinate system, where the line is the x-axis with the normal pointing in the
        // positive y direction, the arc is centered at the origin and the distance between the center and the line at
        // y=0 is the height h.
        var t0 = seg.ProjectionParam(arc.Center);
        var h = seg.SignedDistanceTo(arc.Center);

        if (Math.Abs(arc.Radius - Math.Abs(h)) < GeometryConstants.DistEquals)
        {
            // The line is tangent to the arc, we can simply offset the arc by the distance and return the start point.
            return arc.OffsetBy(distance).Start;
        }

        // There is a parabola that defines the medial axis between the line and the arc.  We can find its form by
        // defining an a and b value.  A clockwise arc means we are intersecting on the inside.
        var a = h * h - arc.Radius * arc.Radius;
        var b = arc.IsCcW ? 2 * (h + arc.Radius) : 2 * (h - arc.Radius);

        // The parabola is defined by the equation y = (a + t^2) / b.  We can find the intersection at any given y by
        // solving for t.  The two intersections are at t = sqrt(b * y - a) and t = -sqrt(b * y - a).
        var t = Math.Sqrt(b * distance - a);

        // Now we need to figure out where the original end was in the modified coordinate system.  This will be the
        // end of the segment, offset by t0.
        var tP = seg.Length - t0;

        // Because the medial axis parabola is continuous over the t domain, we know that the possible intersection
        // which is close to t prime (tP) is the correct one.
        var tN = Math.Abs(t - tP) < Math.Abs(-t - tP) ? t : -t;

        var line = seg.Offset(distance);
        return line.PointAt(tN + t0);
    }

    public static Point2D Make(Arc arc, Segment seg, double distance)
    {
        // We move to the modified coordinate system, where the line is the x-axis with the normal pointing in the
        // positive y direction, the arc is centered at the origin and the distance between the center and the line at
        // y=0 is the height h.
        var t0 = seg.ProjectionParam(arc.Center);
        var h = seg.SignedDistanceTo(arc.Center);

        if (Math.Abs(arc.Radius - Math.Abs(h)) < GeometryConstants.DistEquals)
        {
            // The line is tangent to the arc, we can simply offset the arc by the distance and return the start point.
            return arc.OffsetBy(distance).End;
        }

        // There is a parabola that defines the medial axis between the line and the arc.  We can find its form by
        // defining an a and b value.  A clockwise arc means we are intersecting on the inside.
        var a = h * h - arc.Radius * arc.Radius;
        var b = arc.IsCcW ? 2 * (h + arc.Radius) : 2 * (h - arc.Radius);

        // The parabola is defined by the equation y = (a + t^2) / b.  We can find the intersection at any given y by
        // solving for t.  The two intersections are at t = sqrt(b * y - a) and t = -sqrt(b * y - a).
        var t = Math.Sqrt(b * distance - a);

        // Now we need to figure out where the original end was in the modified coordinate system.  This will be the
        // start of the segment (0), offset by t0.
        var tP = - t0;

        // Because the medial axis parabola is continuous over the t domain, we know that the possible intersection
        // which is close to t prime (tP) is the correct one.
        var tN = Math.Abs(t - tP) < Math.Abs(-t - tP) ? t : -t;

        var line = seg.Offset(distance);
        return line.PointAt(tN + t0);
    }

    public static Point2D Make(Arc arc0, Arc arc1, double distance)
    {
        // Check for tangency
        if (arc0.AtEnd.Normal.IsParallelTo(arc1.AtStart.Normal, GeometryConstants.DistEquals))
        {
            var a0 = arc0.OffsetBy(distance).End.ToVector();
            var a1 = arc1.OffsetBy(distance).Start.ToVector();
            return Point2D.OfVector((a0 + a1) / 2);
        }

        throw new NotImplementedException();
    }

}
