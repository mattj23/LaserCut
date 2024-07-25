using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.Distributions;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// This class holds the algorithms for scanning through a `BoundaryLoop` object and finding places where a sequence
/// of n or more points can be replaced with an arc. This is useful for reducing the number of points in a path
/// and getting true arcs in the boundary.
/// </summary>
internal static class ReplaceWithArcs
{

    /// <summary>
    /// Get the farthest distance between a circle and a line segment defined by two points.
    /// </summary>
    /// <param name="circle"></param>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <returns></returns>
    private static double Farthest(Circle2 circle, Point2D p0, Point2D p1)
    {
        var d0 = Math.Max(circle.DistanceFromEdge(p0), circle.DistanceFromEdge(p1));
        
        // The projection parameter of the closest point on the segment to the center of the circle. 
        var seg = new Segment(p0, p1, -1);
        var closest = seg.ProjectionParam(circle.Center);

        // If the closest point is outside the segment, return the farthest distance between the two points.
        if (closest < 0 || closest > seg.Length) return d0;
        
        // if not, we need to at least check the distance from the closest point on the segment to the circle.
        var dc = seg.PointAt(closest);
        return Math.Max(d0, circle.DistanceFromEdge(dc));
    }
    
    private static int[] NodeIds(BoundaryLoop loop, IBoundaryLoopCursor cursor, Interval interval)
    {
        var start = cursor.GetIdBack(interval.Back);
        var end = cursor.GetIdFwd(interval.Forward);
        var results = new List<int>();
        var working = loop.GetCursor(start);
        while (working.CurrentId != loop.NextId(end))
        {
            results.Add(working.CurrentId);
            working.MoveForward();
        }

        return results.ToArray();
    }
    
    private static (List<Point2D>, Circle2) ArcFromPoints(BoundaryLoop loop, IBoundaryLoopCursor c, Interval interval)
    {
        var points = new List<Point2D>();
        foreach (var nodeId in NodeIds(loop, c, interval))
        {
            // TODO: This check shouldn't be applied to the last point in the list
            if (loop.ItemAt(nodeId) is BoundaryLine line)
            {
                points.Add(line.Point);
            }
            else
            {
                throw new ArgumentException("Invalid node type");
            }
        }
        
        // Create a 3-point circle
        int middle = points.Count / 2;
        var circle = new Circle2(points[0], points[middle], points[^1]);
        return (points, circle);
    }

    private static bool ArcCheck(BoundaryLoop loop, IBoundaryLoopCursor c, Interval interval, double tolerance)
    {
        var (points, circle) = ArcFromPoints(loop, c, interval);
        var maxError = double.MinValue;
        for (int i = 0; i < points.Count - 1; i++)
        {
            var error = Farthest(circle, points[i], points[i + 1]);
            maxError = Math.Max(maxError, error);
        }
        Console.WriteLine(maxError);

        return maxError <= tolerance;
    }

    private record Interval(int Back, int Forward)
    {
        public Interval ExpandBack() => this with { Back = Back - 1 };
        public Interval ExpandForward() => this with { Forward = Forward + 1 };
        
        public int Length => Forward - Back;
    };

    private static Interval? LargestInterval(BoundaryLoop loop, IBoundaryLoopCursor cursor, int minPoints, double tolerance)
    {
        var interval = new Interval(0, minPoints);
        if (ArcCheck(loop, cursor, interval, tolerance))
        {
            while (ArcCheck(loop, cursor, interval.ExpandForward(), tolerance) && interval.Length < loop.Count) 
                interval = interval.ExpandForward();

            while (ArcCheck(loop, cursor, interval.ExpandBack(), tolerance) && interval.Length < loop.Count) 
                interval = interval.ExpandBack();
            
            return interval;
        }

        return null;
    }
    
    public static void ReplaceLinesWithArcs(this BoundaryLoop loop, int minPoints, double tolerance)
    {
        if (minPoints < 4) throw new ArgumentException("Minimum number of points must be at least 4");
        
        // To make this work, we'll start at a point and look for a set of points that are within the tolerance for
        // replacement with an arc.  If we find one, we'll check if expanding forward and backwards from the current
        // set will allow us to replace more.  Once we have a set of points which can't be further expanded, we'll 
        // perform the replacement.  We will continue this process until we've moved through the entire loop.

        var visited = new HashSet<int>();
        var cursor = loop.GetCursor();

        while (loop.Count > 3 && !visited.Contains(cursor.CurrentId))
        {
            if (LargestInterval(loop, cursor, minPoints, tolerance) is { } interval)
            {
                visited.Clear();
                var (points, circle) = ArcFromPoints(loop, cursor, interval);
                // Are the points going clockwise or counter-clockwise?
                var v0 = points[0] - circle.Center;
                var v1 = points[1] - circle.Center;
                bool clockwise = v0.CrossProduct(v1) < 0;
                
                // Get the ids of the nodes we're going to remove, skipping the last once
                var ids = NodeIds(loop, cursor, interval).SkipLast(1).ToArray();
                
                cursor.MoveTo(ids[^1]);
                while (ids.Contains(cursor.CurrentId)) cursor.Remove();
                cursor.ArcAbs(points[0].X, points[0].Y, circle.Center.X, circle.Center.Y, clockwise);
            }
            else
            {
                visited.Add(cursor.CurrentId);
                cursor.MoveForward();
            }
        }

    }
    
}