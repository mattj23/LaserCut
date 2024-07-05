using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using LaserCut.Tests.Helpers;
using LaserCut.Tests.Plotting;

namespace LaserCut.Tests;

public class BodyOperationTests : ShapeOpTestBase
{

    [Fact]
    public void PositiveMergeSimple()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(7, 1, 1, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        
        var results = fixture.Body.Operate(tool);
        Assert.Single(results);
        AssertLoop(expected, results[0].Outer);
        AssertBodyInner(results[0], fixture.A, fixture.B, fixture.C);
    }

    [Fact]
    public void PositiveMergeRemovesInner()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(5, 1, 3, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        
        var results = fixture.Body.Operate(tool);
        Assert.Single(results);
        AssertLoop(expected, results[0].Outer);
        AssertBodyInner(results[0], fixture.A, fixture.B);
    }
    
    [Fact]
    public void PositiveMergeChangesInner()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(5.5, 1, 2.5, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        var c = BoundaryLoop.Rectangle(5, 1, 0.5, 1).Reversed();
        
        var results = fixture.Body.Operate(tool);
        Assert.Single(results);
        AssertLoop(expected, results[0].Outer);
        
        AssertBodyInner(results[0], fixture.A, fixture.B, c);
    }
    
    [Fact]
    public void PositiveMergeRemovesInnerOnly()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(2.5, 0.5, 2, 2);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        var results = fixture.Body.Operate(tool);
        Assert.Single(results);
        AssertLoop(expected, results[0].Outer);
        
        AssertBodyInner(results[0], fixture.A, fixture.C);
    }
    
    [Fact]
    public void PositiveMergeSplitsInner()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(2.5, 1.25, 2, 0.5);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        var b1 = BoundaryLoop.Rectangle(3, 1, 1, 0.25).Reversed();
        var b2 = BoundaryLoop.Rectangle(3, 1.75, 1, 0.25).Reversed();
        
        var results = fixture.Body.Operate(tool);
        Assert.Single(results);
        AssertLoop(expected, results[0].Outer);
        
        AssertBodyInner(results[0], fixture.A, b1, b2, fixture.C);
    }

    [Fact]
    public void NegativeMergeSimple()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(6.5, 1, 1, 1).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (6.5, 1), (6.5, 2), (7, 2), (7, 3), (0, 3));
        
        var results = fixture.Body.Operate(tool);
        Assert.Single(results);
        AssertLoop(expected, results[0].Outer);
        
        AssertBodyInner(results[0], fixture.A, fixture.B, fixture.C);
    }

    [Fact]
    public void NegativeMergeAddsBoundary()
    {
        var outer = BoundaryLoop.Rectangle(0, 0, 3, 3);
        var body = new Body(outer);
        var tool = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();
        
        var results = body.Operate(tool);
        Assert.Single(results);

        var expectedOuter = results[0].Outer.ToItemArray().Select(x => x.Point).ToArray();
        
        AssertLoop(expectedOuter, results[0].Outer);
        AssertBodyInner(results[0], tool);
    }
    
    [Fact]
    public void NegativeMergeAddsSecondBoundary()
    {
        var outer = BoundaryLoop.Rectangle(0, 0, 5, 3);
        var body = new Body(outer);
        var tool0 = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();
        var tool1 = BoundaryLoop.Rectangle(3, 1, 1, 1).Reversed();
        
        var result0 = body.Operate(tool0);
        Assert.Single(result0);
        
        var result1 = result0[0].Operate(tool1);
        Assert.Single(result1);

        var expectedOuter = outer.ToItemArray().Select(x => x.Point).ToArray();
        
        AssertLoop(expectedOuter, result1[0].Outer);
        AssertBodyInner(result1[0], tool0, tool1);
    }

    [Fact]
    public void NegativeMergeSubsumesInnerSimple()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(2.5, 0.5, 2, 2).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        var result = fixture.Body.Operate(tool);
        Assert.Single(result);
        
        AssertLoop(expected, result[0].Outer);
        AssertBodyInner(result[0], fixture.A, tool, fixture.C);
    }
    
    [Fact]
    public void NegativeMergeSubsumesInnerWithIntersections()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(3, 1, 1, 1.5).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        var result = fixture.Body.Operate(tool);
        AssertLoop(expected, result[0].Outer);
        AssertBodyInner(result[0], fixture.A, tool, fixture.C);
    }

    [Fact]
    public void NegativeMergeIntersectsBoundaryOnce()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(3.25, 1.5, 0.5, 2).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (3.75, 3), (3.75, 2), (4, 2), (4, 1), (3, 1), (3, 2), (3.25, 2), (3.25, 3), (0, 3));
        
        // var plot = new DebugPlot("NegativeMergeIntersectsBoundaryOnce");
        // plot.Add(fixture.Body, "Fixture");
        // plot.Add(tool, "Tool");
        // plot.Plot();
        
        var result = fixture.Body.Operate(tool);
        AssertLoop(expected, result[0].Outer);
        
        AssertBodyInner(result[0], fixture.A, fixture.C);
    }

    [Fact]
    public void NegativeMergeIntersectsBoundaryTwice()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(3.5, 1.25, 5, 0.5).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1.25),
            (6, 1.25), (6, 1), (5, 1), (5, 1.25), (4, 1.25), (4, 1), (3, 1), (3, 2), (4, 2), (4, 1.75), (5, 1.75),
            (5, 2), (6, 2), (6, 1.75), (7, 1.75),
            (7, 3), (0, 3));
        
        var result = fixture.Body.Operate(tool);
        AssertLoop(expected, result[0].Outer);
        
        AssertBodyInner(result[0], fixture.A);
    }

    [Fact]
    public void NegativeMergeJoinsTwoInners()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(3.5, 1.25, 2, 0.5).Reversed();
        var outside = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        var inside = Loop((3, 1), (4, 1), (4, 1.25), (5, 1.25), (5, 1), (6, 1), (6, 2), (5, 2), (5, 1.75),
            (4, 1.75), (4, 2), (3, 2)).Reversed();
        
        var result = fixture.Body.Operate(tool);
        AssertLoop(outside, result[0].Outer);
        AssertBodyInner(result[0], fixture.A, inside);
    }

    [Fact]
    public void NegativeMergeJoinsThreeInners()
    {
        var fixture = TestBody();
        var tool = BoundaryLoop.Rectangle(1.5, 1.25, 4, 0.5).Reversed();
        var outside = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        var inside = Loop((3, 1), (4, 1), (4, 1.25), (5, 1.25), (5, 1), (6, 1), (6, 2), (5, 2), (5, 1.75),
            (4, 1.75), (4, 2), (3, 2), (3, 1.75), (2, 1.75), (2, 2), (1, 2), (1, 1),
            (2, 1), (2, 1.25), (3, 1.25)).Reversed();
        
        var result = fixture.Body.Operate(tool);
        AssertLoop(outside, result[0].Outer);
        AssertBodyInner(result[0], inside);
    }

    // [Fact]
    // public void BodyDoubleCutout()
    // {
    //     // This test case comes from an actual application which was misbehaving
    //     var width = 1;
    //     var height = 2.7;
    //
    //     // Use a temporary body to cut the main pocket
    //     var working = new Body(PointLoop.Rectangle(3.5, 5.0));
    //
    //     var c0 = PointLoop.RoundedRectangle(height, width, 0.15, 8).Reversed();
    //     var c1 = PointLoop.Rectangle(0.62, 1.7).Reversed();
    //     var c3 = PointLoop.Rectangle(0.25, 0.25).Reversed();
    //     c1.Translate(c0.Bounds.Center.X - c1.Bounds.MinX - (1.25 / 2), c0.Bounds.MinY - c1.Bounds.MinY + .3);
    //     c3.Translate(c0.Bounds.MaxX - c3.Bounds.MinX - 0.01, c0.Bounds.MaxY - c3.Bounds.MaxY - 0.73);
    //
    //     // Cut the main pocket
    //     working.Operate(c0);
    //     working.Operate(c1);
    //     working.Operate(c3);
    //
    //     // Create the actual body
    //     var cut0 = working.Inners.First().Copy();
    //     var cut1 = cut0.Copy();
    //     var body = new Body(PointLoop.Rectangle(4, 5.0));
    //
    //     var padding = (body.Bounds.Width - 2 * cut0.Bounds.Width) / 3;
    //     cut0.Translate(body.Bounds.MinX - cut0.Bounds.MinX + padding, body.Bounds.MinY - cut0.Bounds.MinY + 0.3);
    //     cut1.Translate(cut0.Bounds.MaxX - cut1.Bounds.MinX + padding, body.Bounds.MinY - cut1.Bounds.MinY + 0.3);
    //
    //     body.Operate(cut0);
    //     body.Operate(cut1);
    //
    //     var outer = ExpectedPoints((-2.5, -2), (2.5, -2), (2.5, 2), (-2.5, 2));
    //     AssertLoop(outer, body.Outer);
    //     AssertBodyInner(body, cut0, cut1);
    // }
    //
    // [Fact]
    // public void AvoidErroneousSubsumed()
    // {
    //     // This test case comes from an actual application which was misbehaving
    //     var width = 1;
    //     var height = 2.7;
    //
    //     // Use a temporary body to cut the main pocket
    //     var working = new Body(Contour.Rectangle(0, 0, 5, 3.5));
    //
    //     var c0 = PointLoop.RoundedRectangle(height, width, 0.15, 8).Reversed();
    //     var c1 = PointLoop.Rectangle(0.62, 1.7).Reversed();
    //     var c3 = PointLoop.Rectangle(0.25, 0.25).Reversed();
    //     c1.Translate(c0.Bounds.Center.X - c1.Bounds.MinX - (1.25 / 2), c0.Bounds.MinY - c1.Bounds.MinY + .3);
    //     c3.Translate(c0.Bounds.MaxX - c3.Bounds.MinX - 0.01, c0.Bounds.MaxY - c3.Bounds.MaxY - 0.73);
    //
    //     // Cut the main pocket
    //     working.Operate(c0);
    //     working.Operate(c1);
    //     working.Operate(c3);
    //
    //     // Create the actual body
    //     var cut0 = working.Inners.First().Copy();
    //     var cut1 = cut0.Copy();
    //     var body = new Body(Contour.Rectangle(0, 0, 5, 4));
    //
    //     var padding = (body.Bounds.Width - 2 * cut0.Bounds.Width) / 3;
    //     cut0.Translate(body.Bounds.MinX - cut0.Bounds.MinX + padding, body.Bounds.MinY - cut0.Bounds.MinY + 0.3);
    //     cut1.Translate(cut0.Bounds.MaxX - cut1.Bounds.MinX + padding, body.Bounds.MinY - cut1.Bounds.MinY + 0.3);
    //
    //     var (result, _) = ShapeOperation.Operate(cut0, cut1);
    //     
    //     Assert.Equal(ShapeOperation.ResultType.Disjoint, result);
    // }

    private TestFixture TestBody()
    {
        var a = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();
        var b = BoundaryLoop.Rectangle(3, 1, 1, 1).Reversed();
        var c = BoundaryLoop.Rectangle(5, 1, 1, 1).Reversed();
        
        var outer = BoundaryLoop.Rectangle(0, 0, 7, 3);
        var inners = new List<BoundaryLoop> { a, b, c };

        return new TestFixture(new Body(outer, inners), a, b, c);
    }

    private record TestFixture(Body Body, BoundaryLoop A, BoundaryLoop B, BoundaryLoop C);

}