using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class PointLoopMergeTests
{
    [Fact]
    public void MergeTwoRectangles()
    {
        var loop0 = Rect(-2, -2, 4, 4);
        var loop1 = Rect(-3, -1, 6, 2);

        var result = loop0.MergedWith(loop1);
        
        var expected = new[]
        {
            new Point2D(-2, -2),
            new Point2D(2, -2),
            new Point2D(2, -1),
            new Point2D(3, -1),
            new Point2D(3, 1),
            new Point2D(2, 1),
            new Point2D(2, 2),
            new Point2D(-2, 2),
            new Point2D(-2, 1),
            new Point2D(-3, 1),
            new Point2D(-3, -1),
            new Point2D(-2, -1)
        };
        
        var closest = result.FirstId(p => p.DistanceTo(expected[0]) < 1e-10);
        var values = result.ToItemArray(closest);
        
        Assert.Equal(expected, values);
    }


    private PointLoop Rect(double x0, double y0, double width, double height)
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