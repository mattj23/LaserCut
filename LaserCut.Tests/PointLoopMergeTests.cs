using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class PointLoopMergeTests
{
    [Fact]
    public void MergeOverlappingPositiveSimple()
    {
        var loop0 = Rect(-2, -2, 4, 4);
        var loop1 = Rect(-3, -1, 6, 2);
        var expected = ExpectedPoints((-2, -2), (2, -2), (2, -1), (3, -1), (3, 1), (2, 1), (2, 2), (-2, 2), (-2, 1),
            (-3, 1), (-3, -1), (-2, -1));

        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void MergeOverlappingNegativeSimple()
    {
        var loop0 = Rect(-2, -2, 4, 4).Reversed();
        var loop1 = Rect(-3, -1, 6, 2).Reversed();
        var expected = ExpectedPoints((-2, -2), (2, -2), (2, -1), (3, -1), (3, 1), (2, 1), (2, 2), (-2, 2), (-2, 1),
            (-3, 1), (-3, -1), (-2, -1)).Reverse().ToArray();

        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }

    [Fact]
    public void MergeCutoutPositiveSimple()
    {
        var loop0 = Rect(0, 0, 2, 3);
        var loop1 = Rect(1, 1, 2, 1).Reversed();
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3));
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void MergeCutoutNegativeSimple()
    {
        var loop0 = Rect(0, 0, 2, 3).Reversed();
        var loop1 = Rect(1, 1, 2, 1);
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3))
            .Reverse()
            .ToArray();
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }

    [Fact]
    public void MergePositiveSharedSide()
    {
        var loop0 = Rect(0, 0, 1, 3);
        var loop1 = Rect(1, 1, 1, 1);

        var expected = ExpectedPoints((0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (1, 2), (1, 3), (0, 3));
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void MergeNegativeSharedSide()
    {
        var loop0 = Rect(0, 0, 1, 3).Reversed();
        var loop1 = Rect(1, 1, 1, 1).Reversed();

        var expected = ExpectedPoints((0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (1, 2), (1, 3), (0, 3))
            .Reverse()
            .ToArray();
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CutoutPositiveSharedSide()
    {
        var loop0 = Rect(0, 0, 2, 3);
        var loop1 = Rect(1, 1, 1, 1).Reversed();
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3));
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void CutoutNegativeSharedSide()
    {
        var loop0 = Rect(0, 0, 2, 3).Reversed();
        var loop1 = Rect(1, 1, 1, 1);
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3))
            .Reverse()
            .ToArray();
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CutoutPositiveSharedCorner()
    {
        var loop0 = Rect(0, 0, 2, 2);
        var loop1 = Rect(1, 0, 1, 1).Reversed();

        var expected = ExpectedPoints((0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (0, 2));
        var result = loop0.MergedWith(loop1);
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }

    private Point2D[] ExpectedPoints(params ValueTuple<double, double> [] points)
    {
        return points.Select(p => new Point2D(p.Item1, p.Item2)).ToArray();
    }

    private Point2D[] OrientedPoints(PointLoop result, Point2D[] expected)
    {
        var closest = result.FirstId(p => p.DistanceTo(expected[0]) < 1e-10);
        return result.ToItemArray(closest);
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