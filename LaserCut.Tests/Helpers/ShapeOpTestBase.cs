using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Helpers;

public class ShapeOpTestBase
{
    protected Point2D[] ExpectedPoints(params ValueTuple<double, double> [] points)
    {
        return points.Select(p => new Point2D(p.Item1, p.Item2)).ToArray();
    }
    
    protected BoundaryLoop Loop(Point2D[] points)
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        foreach (var p in points)
        {
            cursor.SegAbs(p.X, p.Y);
        }

        return contour;
    }

    protected BoundaryLoop Loop(params ValueTuple<double, double>[] points)
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        foreach (var (x, y) in points)
        {
            cursor.SegAbs(x, y);
        }

        return contour;
    }
    
    /// <summary>
    /// Given an array of expected points, find the first match in the list of contour results where the first expected
    /// point is within 1e-10 of any point in the contour. Remove the match from the list of results and return it.
    /// </summary>
    protected BoundaryLoop TakeMatch(Point2D[] expected, List<BoundaryLoop> results)
    {
        var match = results.First(r => r.ToItemArray().Any(p => expected[0].DistanceTo(p.Point) < 1e-10));
        results.Remove(match);
        return match;
    }

    /// <summary>
    /// Given a result contour and an array of expected points, find the vertex in the contour that is closest to the
    /// first expected point and return the contour as an array of points starting at that vertex.
    /// </summary>
    protected Point2D[] OrientedPoints(BoundaryLoop result, Point2D[] expected)
    {
        var closest = result.FirstId(p => p.Point.DistanceTo(expected[0]) < 1e-10);
        return result.ToItemArray(closest).Select(x => x.Point).ToArray();
    }

    protected void AssertLoop(Point2D[] expected, BoundaryLoop loop)
    {
        var values = OrientedPoints(loop, expected);
        Assert.Equal(expected, values);
    }
    
    protected void AssertBodyInner(Body body, params BoundaryLoop[] expected)
    {
        var inners = body.Inners.ToList();
        Assert.Equal(expected.Length, inners.Count);
        foreach (var loop in expected)
        {
            var loopPnts = loop.ToItemArray().Select(x => x.Point).ToArray();
            var match = TakeMatch(loopPnts, inners);
            AssertLoop(loopPnts, match);
        }
    }
    
}