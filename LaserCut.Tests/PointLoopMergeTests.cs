﻿using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class ShapeOperationTests : PointLoopTestBase
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

    [Fact]
    public void DegenerateMerge()
    {
        var loop0 = Rect(1, 1, 1, 1).Reversed();
        var loop1 = Rect(2, 1, 1, 1.5).Reversed();
        var result = loop0.MergedWith(loop1);
        result.RemoveAdjacentCollinear();
        
        var expected = ExpectedPoints((1, 1), (1, 2), (2, 2), (2, 2.5), (3, 2.5), (3, 1));
        var values = OrientedPoints(result, expected);
        
        Assert.Equal(expected, values);
    }

    [Fact]
    public void DegenerateOverlappingMerge()
    {
        // This was a test case of merge that got stuck in an infinite loop because we're effectively performing the 
        // same cut that just created the outer loop
        var outerPts = ExpectedPoints((4, 1.75), (7, 1.75), (7, 3), (0, 3), (0, 0), (7, 0), (7, 1.25), (4, 1.25), (4, 1),
            (3, 1), (3, 2), (4, 2));
        var outer = new PointLoop(outerPts);
        var tool = new PointLoop(ExpectedPoints((3.5, 1.75), (8.5, 1.75), (8.5, 1.25), (3.5, 1.25)));
        
        var (a, b) = ShapeOperation.Operate(outer, tool);
        Assert.Equal(ShapeOperation.ResultType.Merged, a);
        
    }
    

    [Fact]
    public void CutMergeDoesntProduceTwoResults()
    {
        // This test case comes from an example while working on the body operation tests.  The tool is a 1x1 square at
        // 5,1 and should be intersecting with an outer loop that has a concave portion.  It should produce a single
        // result, but instead it produced two.
        
        var outerPts = ExpectedPoints((4, 1.75), (7, 1.75), (7, 3), (0, 3), (0, 0), (7, 0), (7, 1.25), (4, 1.25), (4, 1),
            (3, 1), (3, 2), (4, 2));
        var outer = new PointLoop(outerPts);
        var tool = Rect(5, 1, 1, 1).Reversed();
        
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1.25),
            (6, 1.25), (6, 1), (5, 1), (5, 1.25), (4, 1.25), (4, 1), (3, 1), (3, 2), (4, 2), (4, 1.75), (5, 1.75),
            (5, 2), (6, 2), (6, 1.75), (7, 1.75),
            (7, 3), (0, 3));
        
        var (a, b) = ShapeOperation.Operate(outer, tool);
        Assert.Equal(ShapeOperation.ResultType.Merged, a);
        Assert.Single(b);
        AssertLoop(expected, b[0]);

    }

}