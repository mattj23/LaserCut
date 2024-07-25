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
public static class ReplaceWithArcs
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

    private static BoundaryPoint[] PointsAt(BoundaryLoop loop, int start, int n)
    {
        var cursor = loop.GetCursor(start);
        var results = new BoundaryPoint[n];
        for (int i = 0; i < n; i++)
        {
            results[i] = cursor.Current;
            cursor.MoveForward();
        }

        return results;
    }
    
    private static Circle2? CircleFromPoints(BoundaryPoint[] points)
    {
        var p0 = points[0].Point;
        var p1 = points[points.Length / 2].Point;
        var p2 = points[^1].Point;
        try
        {
            return new Circle2(p0, p1, p2);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static bool ArcMatches(BoundaryLoop loop, int start, int n, double tol)
    {
        var points = PointsAt(loop, start, n);
        if (points.SkipLast(1).Any(p => p is not BoundaryLine)) return false;
        var circle = CircleFromPoints(points);
        if (circle is null) return false;
        
        var maxError = double.MinValue;
        for (int i = 0; i < points.Length - 1; i++)
        {
            var error = Farthest(circle, points[i].Point, points[i + 1].Point);
            maxError = Math.Max(maxError, error);
        }

        return maxError <= tol;
    }
    
    public static void ReplaceLinesWithArcs(this BoundaryLoop loop, int minPoints, double tol)
    {
        if (minPoints < 4) throw new ArgumentException("Minimum number of points must be at least 4");

        var visited = new HashSet<int>();
        var walk = loop.GetCursor();

        while (loop.Count > 3 && !visited.Contains(walk.CurrentId))
        {
            var n = minPoints;
            if (!ArcMatches(loop, walk.CurrentId, n, tol))
            {
                visited.Add(walk.CurrentId);
                walk.MoveForward();
                continue;
            }
            
            while (ArcMatches(loop, walk.PreviousId, n + 1, tol) && n < loop.Count)
            {
                walk.MoveBackward();
                n++;
            }
            
            while (ArcMatches(loop, walk.CurrentId, n + 1, tol) && n < loop.Count)
            {
                if (n == 157)
                {
                    Console.WriteLine("here");
                }
                n++;
            }
            
            var points = PointsAt(loop, walk.CurrentId, n);
            var circle = CircleFromPoints(points);
            if (circle is null) throw new InvalidOperationException("Invalid circle");

            var original = walk.Current.Point;
            
            for (int k = 0; k < n - 1; k++)
            {
                walk.Remove();
            }
            
            // Are the points going clockwise or counter-clockwise?
            var v0 = points[0].Point - circle.Center;
            var v1 = points[1].Point - circle.Center;
            bool clockwise = v0.CrossProduct(v1) < 0;
            walk.ArcAbs(original.X, original.Y, circle.Center.X, circle.Center.Y, clockwise);
        }
    }
    
}