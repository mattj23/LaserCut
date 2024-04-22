using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Helpers;

public class PointLoopTestBase
{
    protected Point2D[] ExpectedPoints(params ValueTuple<double, double> [] points)
    {
        return points.Select(p => new Point2D(p.Item1, p.Item2)).ToArray();
    }

    protected PointLoop TakeMatch(Point2D[] expected, List<PointLoop> results)
    {
        var match = results.First(r => r.ToItemArray().Any(p => expected[0].DistanceTo(p) < 1e-10));
        results.Remove(match);
        return match;
    }

    protected Point2D[] OrientedPoints(PointLoop result, Point2D[] expected)
    {
        var closest = result.FirstId(p => p.DistanceTo(expected[0]) < 1e-10);
        return result.ToItemArray(closest);
    }

    protected void AssertLoop(Point2D[] expected, PointLoop loop)
    {
        var values = OrientedPoints(loop, expected);
        Assert.Equal(expected, values);
    }
    
    protected void AssertBodyInner(Body body, params PointLoop[] expected)
    {
        var inners = body.Inners.ToList();
        Assert.Equal(expected.Length, inners.Count);
        foreach (var loop in expected)
        {
            var match = TakeMatch(loop.ToItemArray(), inners);
            AssertLoop(loop.ToItemArray(), match);
        }
    }
    
    protected PointLoop Rect(double x0, double y0, double width, double height)
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(x0, y0);
        cursor.InsertRelX(width);
        cursor.InsertRelY(height);
        cursor.InsertRelX(-width);
        return loop;
    }
    
}