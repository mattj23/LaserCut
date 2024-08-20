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
        // Check for tangency between the two arcs.  Tangency is both easier to solve and also will be a degenerate
        // case under the general solution.
        if (arc0.AtEnd.Normal.IsParallelTo(arc1.AtStart.Normal, GeometryConstants.DistEquals))
        {
            var a0 = arc0.OffsetBy(distance).End.ToVector();
            var a1 = arc1.OffsetBy(distance).Start.ToVector();
            return Point2D.OfVector((a0 + a1) / 2);
        }

        /*
         I think this is overkill, but I'm going to leave it in here for now.  I believe that I can just take the
         intersection of the two offset arcs and find the one which is closest in angle to the original intersection.

        // We have two arcs which are not tangent where they meet.  We will now move to the modified coordinate system,
        // which consists of three parameters: the radius of the first arc (r0), the radius of the second arc (r1), and
        // the distance between the centers of the two arcs (h).
        //
        // When working, we will begin by finding the formula of the polar parabola that defines the medial axis between
        // the two arcs by working will the full circles.  To visualize the problem, imagine a circle of radius r0
        // sitting at the origin, and a circle of radius r1 sitting at x = h.  A polar coordinate system superimposed
        // on top of this will have Θ=0 pointing in the positive x direction.
        //
        // In the polar coordinate system, the medial axis will lie at a distance d(Θ) = (a cosΘ + b)/(c cosΘ + d) from
        // the origin.  The values of a, b, c, and d are determined by the configuration of outside to inside arcs.
        var x0 = (arc1.Center - arc0.Center).Normalize();

        var r0 = arc0.Radius;
        var r1 = arc1.Radius;
        var h = arc0.Center.DistanceTo(arc1.Center);

        var c = -2 * h;
        var a = 2 * r0 * h * (arc0.IsCcW ? 1 : -1);
        var d = 2 * r0 + 2 * r1 * (arc1.IsCcW == arc0.IsCcW ? -1 : 1);
        var b = arc0.IsCcW
            ? -(r0 * r0) + r1 * r1 - h * h
            : r0 * r0 - r1 * r1 + h * h;

        // To solve this, we will first find a new offset radius and solve for it directly.
        var y = r0 + (arc0.IsCcW ? distance : -distance);
        var m = (d * y - b) / (a - c * y);

        // The possible solutions are the various valid arccos values of m.  We must find the ones
        */

        var offset0 = (Arc)arc0.OffsetBy(distance);
        var offset1 = (Arc)arc1.OffsetBy(distance);

        var intersections = offset0.Circle.Intersections(offset1.Circle);
        if (intersections.Length == 0)
        {
            throw new InvalidOperationException("The two arcs do not intersect.");
        }

        var v0 = (arc0.End - arc0.Center).Normalize();

        return intersections.MinBy(pi => Math.Abs((pi - arc0.Center).Normalize().SignedAngle(v0)));
    }


}
