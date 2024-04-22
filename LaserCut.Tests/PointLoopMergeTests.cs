using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class ShapeOperationTests
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

    [Fact]
    public void CutSplit()
    {
        var loop0 = Rect(-2, -2, 4, 4);
        var loop1 = Rect(-3, -1, 6, 2).Reversed();
        var expected = new[]
        {
            ExpectedPoints((-2, -2), (2, -2), (2, -1), (-2, -1)),
            ExpectedPoints((-2, 1), (2, 1), (2, 2), (-2, 2))
        };

        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.Merged, a);
        var results = b.ToList();
        
        var match0 = TakeMatch(expected[0], results);
        var values0 = OrientedPoints(match0, expected[0]);
        Assert.Equal(expected[0], values0);
        
        var match1 = TakeMatch(expected[1], results);
        var values1 = OrientedPoints(match1, expected[1]);
        Assert.Equal(expected[1], values1);
        
        Assert.Empty(results);
    }

    [Fact]
    public void OperationDisjoint()
    {
        var loop0 = Rect(0, 0, 1, 1);
        var loop1 = Rect(2, 2, 1, 1);
        
        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.Disjoint, a);
        Assert.Empty(b);
    }
    
    [Fact]
    public void OperationSubsumedNoIntersections()
    {
        var loop0 = Rect(0, 0, 1, 1);
        var loop1 = Rect(-1, -1, 3, 3);
        
        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.Subsumed, a);
        Assert.Empty(b);
    }
    
    [Fact]
    public void OperationSubsumedWithIntersections()
    {
        var loop0 = Rect(0, 0, 1, 1);
        var loop1 = Rect(0, 0, 1, 2);
        
        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.Subsumed, a);
        Assert.Empty(b);
    }
    
    [Fact]
    public void OperationDestroyedNoIntersections()
    {
        var loop0 = Rect(0, 0, 1, 1);
        var loop1 = Rect(-1, -1, 3, 3).Reversed();
        
        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.Destroyed, a);
        Assert.Empty(b);
    }
    
    [Fact]
    public void OperationDestroyedWithIntersections()
    {
        var loop0 = Rect(0, 0, 1, 1);
        var loop1 = Rect(0, 0, 1, 2).Reversed();
        
        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.Destroyed, a);
        Assert.Empty(b);
    }

    [Fact]
    public void OperationShapeEnclosesTool()
    {
        var loop0 = Rect(-1, -1, 3, 3);
        var loop1 = Rect(0, 0, 1, 1);
        
        var (a, b) = ShapeOperation.Operate(loop0, loop1);
        Assert.Equal(ShapeOperation.ResultType.ShapeEnclosesTool, a);
        Assert.Empty(b);
    }

    private Point2D[] ExpectedPoints(params ValueTuple<double, double> [] points)
    {
        return points.Select(p => new Point2D(p.Item1, p.Item2)).ToArray();
    }

    private PointLoop TakeMatch(Point2D[] expected, List<PointLoop> results)
    {
        var match = results.First(r => r.ToItemArray().Any(p => expected[0].DistanceTo(p) < 1e-10));
        results.Remove(match);
        return match;
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