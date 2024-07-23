using LaserCut.Algorithms;
using LaserCut.Geometry;
using LaserCut.Helpers;
using LaserCut.Tests.Helpers;
using LaserCut.Tests.Plotting;

namespace LaserCut.Tests.Algorithms;

public class BoundaryOpsTests : ShapeOpTestBase
{
    [Fact]
    public void MergeOverlappingPositiveSimple()
    {
        var loop0 = BoundaryLoop.Rectangle(-2, -2, 4, 4);
        var loop1 = BoundaryLoop.Rectangle(-3, -1, 6, 2);
        var expected = ExpectedPoints((-2, -2), (2, -2), (2, -1), (3, -1), (3, 1), (2, 1), (2, 2), (-2, 2), (-2, 1),
            (-3, 1), (-3, -1), (-2, -1));

        var (kind, result) = loop0.Union(loop1);
        
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void MergeOverlappingNegativeSimple()
    {
        var loop0 = BoundaryLoop.Rectangle(-2, -2, 4, 4).Reversed();
        var loop1 = BoundaryLoop.Rectangle(-3, -1, 6, 2).Reversed();
        var expected = ExpectedPoints((-2, -2), (2, -2), (2, -1), (3, -1), (3, 1), (2, 1), (2, 2), (-2, 2), (-2, 1),
            (-3, 1), (-3, -1), (-2, -1)).Reverse().ToArray();
    
        var (kind, result) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void MergeCutoutPositiveSimple()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 2, 3);
        var loop1 = BoundaryLoop.Rectangle(1, 1, 2, 1).Reversed();
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3));
        
        var (kind, result) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void MergeCutoutNegativeSimple()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 2, 3).Reversed();
        var loop1 = BoundaryLoop.Rectangle(1, 1, 2, 1);
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3))
            .Reverse()
            .ToArray();
        
        var (kind, result) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void MergePositiveSharedSide()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 3);
        var loop1 = BoundaryLoop.Rectangle(1, 1, 1, 1);
        var expected = ExpectedPoints((0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (1, 2), (1, 3), (0, 3));
        
        var (kind, result) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void MergeNegativeSharedSide()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 3).Reversed();
        var loop1 = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();
    
        var expected = ExpectedPoints((0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (1, 2), (1, 3), (0, 3))
            .Reverse()
            .ToArray();
        
        var (kind, result) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CutoutPositiveSharedSide()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 2, 3);
        var loop1 = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3));
        
        var (kind, result) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CutoutNegativeSharedSide()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 2, 3).Reversed();
        var loop1 = BoundaryLoop.Rectangle(1, 1, 1, 1);
        
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3))
            .Reverse()
            .ToArray();
        
        var (kind, result) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CutoutPositiveSharedCorner()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 2, 2);
        var loop1 = BoundaryLoop.Rectangle(1, 0, 1, 1).Reversed();
    
        var expected = ExpectedPoints((0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (0, 2));
        
        var (kind, result) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Merged, kind);
        Assert.Single(result);
        
        var values = OrientedPoints(result[0], expected);
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CutSplit()
    {
        var loop0 = BoundaryLoop.Rectangle(-2, -2, 4, 4);
        var loop1 = BoundaryLoop.Rectangle(-3, -1, 6, 2).Reversed();
        var expected = new[]
        {
            ExpectedPoints((-2, -2), (2, -2), (2, -1), (-2, -1)),
            ExpectedPoints((-2, 1), (2, 1), (2, 2), (-2, 2))
        };
    
        var (a, b) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Merged, a);
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
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var loop1 = BoundaryLoop.Rectangle(2, 2, 1, 1);
    
        var (a, b) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Destroyed, a);
        Assert.Single(b);
        Assert.Equal(loop0, b[0]);
    }
    
    [Fact]
    public void OperationSubsumedWithoutIntersections()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var loop1 = BoundaryLoop.Rectangle(-1, -1, 3, 3);
    
        var (a, b) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Replaced, a);
        Assert.Single(b);
        Assert.Equal(loop1, b[0]);
    }
    
    [Fact]
    public void OperationSubsumedWithIntersections()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var loop1 = BoundaryLoop.Rectangle(0, 0, 1, 2);
        
        var (a, b) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Replaced, a);
        Assert.Single(b);
        Assert.Equal(loop1, b[0]);
    }
    
    [Fact]
    public void OperationDestroyedNoIntersections()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var loop1 = BoundaryLoop.Rectangle(-1, -1, 3, 3).Reversed();
    
        var (a, b) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Destroyed, a);
        Assert.Empty(b);
    }
    
    [Fact]
    public void OperationDestroyedWithIntersections()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var loop1 = BoundaryLoop.Rectangle(0, 0, 1, 2).Reversed();
        
        var (a, b) = loop0.Intersection(loop1);
        Assert.Equal(BoundaryOpResult.Destroyed, a);
        Assert.Empty(b);
    }
    
    [Fact]
    public void OperationShapeEnclosesTool()
    {
        var loop0 = BoundaryLoop.Rectangle(-1, -1, 3, 3);
        var loop1 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        
        var (a, b) = loop0.Union(loop1);
        Assert.Equal(BoundaryOpResult.Unchanged, a);
        Assert.Single(b);
        Assert.Equal(loop0, b[0]);
    }
    
    [Fact]
    public void DegenerateMerge()
    {
        var loop0 = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();
        var loop1 = BoundaryLoop.Rectangle(2, 1, 1, 1.5).Reversed();
        var (_, b) = loop0.Intersection(loop1);
        Assert.Single(b);
        var result = b[0];
        
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
    
        var outer = Loop(outerPts);
        var tool = Loop((3.5, 1.75), (8.5, 1.75), (8.5, 1.25), (3.5, 1.25));
    
        var (a, _) = outer.Intersection(tool);
        Assert.Equal(BoundaryOpResult.Merged, a);
    }
    
    [Fact]
    public void CutMergeDoesntProduceTwoResults()
    {
        // This test case comes from an example while working on the body operation tests.  The tool is a 1x1 square at
        // 5,1 and should be intersecting with an outer loop that has a concave portion.  It should produce a single
        // result, but instead it produced two.
        
        var outerPts = ExpectedPoints((4, 1.75), (7, 1.75), (7, 3), (0, 3), (0, 0), (7, 0), (7, 1.25), (4, 1.25), (4, 1),
            (3, 1), (3, 2), (4, 2));
        var outer = Loop(outerPts);
        var tool = BoundaryLoop.Rectangle(5, 1, 1, 1).Reversed();
        
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1.25),
            (6, 1.25), (6, 1), (5, 1), (5, 1.25), (4, 1.25), (4, 1), (3, 1), (3, 2), (4, 2), (4, 1.75), (5, 1.75),
            (5, 2), (6, 2), (6, 1.75), (7, 1.75),
            (7, 3), (0, 3));
    
        var (a, b) = outer.Intersection(tool);
        Assert.Equal(BoundaryOpResult.Merged, a);
        Assert.Single(b);
        AssertLoop(expected, b[0]);
    }
    
    [Fact]
    public void CircleMerge()
    {
        var c0 = BoundaryLoop.Circle(0, 0, 1, false);
        var c1 = BoundaryLoop.Circle(0, 1, 1, false);
        
        var (_, b) = c0.Union(c1);
        Assert.Single(b);
    
        var result = b[0];
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public void ConcaveRejoin()
    {
        // Create a C-shaped loop
        var c0 = Loop((0, 0), (2, 0), (2, 1), (1, 1), (1, 2), (2, 2), (2, 3), (0, 3));
        
        // Create an end cap that turns the C into an O
        var tool = Loop((2, 0), (3, 0), (3, 3), (2, 3));
        
        var (a, b) = c0.Union(tool, new OpDebugWriter(@"D:\temp\test2.dbg"));
        var results = b.ToList();
        
        Assert.Equal(BoundaryOpResult.Merged, a);
        
        // We should now have two loops, one which is an outer boundary and the other which is the hole leftover
        var expectedOuter = ExpectedPoints((0, 0), (3, 0), (3, 3), (0, 3));
        var expectedHole = ExpectedPoints((1, 1), (1, 2), (2, 2), (2, 1));
        Assert.Equal(2, results.Count);
    
        var matchOuter = TakeMatch(expectedOuter, results);
        AssertLoop(expectedOuter, matchOuter);
        
        var matchHole = TakeMatch(expectedHole, results);
        AssertLoop(expectedHole, matchHole);
    }
    
    [Fact]
    public void NonTerminatingMerge()
    {
        // This test case was a real-world example where the merge operation was not terminating
        var working = Loop((2, 1.25), (2, 1), (1, 1), (1, 2), (2, 2), (2, 1.75), (5.5, 1.75), (5.5, 1.25));
        var tool = Loop((3, 1.75), (3, 2), (4, 2), (4, 1.75), (5.5, 1.75), (5.5, 1.25), (4, 1.25), (4, 1), (3, 1),
            (3, 1.25), (1.5, 1.25), (1.5, 1.75));
        
        var (a, b) = working.Intersection(tool);
        Assert.Equal(BoundaryOpResult.Merged, a);
    }
}