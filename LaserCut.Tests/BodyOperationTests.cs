using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using LaserCut.Tests.Helpers;
using LaserCut.Tests.Plotting;

namespace LaserCut.Tests;

public class BodyOperationTests : PointLoopTestBase
{

    [Fact]
    public void PositiveMergeSimple()
    {
        var fixture = TestBody();
        var tool = Rect(7, 1, 1, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B, fixture.C);
    }

    [Fact]
    public void PositiveMergeRemovesInner()
    {
        var fixture = TestBody();
        var tool = Rect(5, 1, 3, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B);
    }
    
    [Fact]
    public void PositiveMergeChangesInner()
    {
        var fixture = TestBody();
        var tool = Rect(5.5, 1, 2.5, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        var c = Rect(5, 1, 0.5, 1).Reversed();
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B, c);
    }
    
    [Fact]
    public void PositiveMergeRemovesInnerOnly()
    {
        var fixture = TestBody();
        var tool = Rect(2.5, 0.5, 2, 2);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.C);
    }
    
    [Fact]
    public void PositiveMergeSplitsInner()
    {
        var fixture = TestBody();
        var tool = Rect(2.5, 1.25, 2, 0.5);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        var b1 = Rect(3, 1, 1, 0.25).Reversed();
        var b2 = Rect(3, 1.75, 1, 0.25).Reversed();
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, b1, b2, fixture.C);
    }

    [Fact]
    public void NegativeMergeSimple()
    {
        var fixture = TestBody();
        var tool = Rect(6.5, 1, 1, 1).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (6.5, 1), (6.5, 2), (7, 2), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B, fixture.C);
    }

    [Fact]
    public void NegativeMergeAddsBoundary()
    {
        var outer = Rect(0, 0, 3, 3);
        var body = new Body(outer);
        var tool = Rect(1, 1, 1, 1).Reversed();
        
        body.Operate(tool);

        var expectedOuter = outer.ToItemArray();
        
        AssertLoop(expectedOuter, body.Outer);
        AssertBodyInner(body, tool);
    }
    
    [Fact]
    public void NegativeMergeAddsSecondBoundary()
    {
        var outer = Rect(0, 0, 5, 3);
        var body = new Body(outer);
        var tool0 = Rect(1, 1, 1, 1).Reversed();
        var tool1 = Rect(3, 1, 1, 1).Reversed();
        
        body.Operate(tool0);
        body.Operate(tool1);

        var expectedOuter = outer.ToItemArray();
        
        AssertLoop(expectedOuter, body.Outer);
        AssertBodyInner(body, tool0, tool1);
    }

    [Fact]
    public void NegativeMergeSubsumesInnerSimple()
    {
        var fixture = TestBody();
        var tool = Rect(2.5, 0.5, 2, 2).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, tool, fixture.C);
    }
    
    [Fact]
    public void NegativeMergeSubsumesInnerWithIntersections()
    {
        var fixture = TestBody();
        var tool = Rect(3, 1, 1, 1.5).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, tool, fixture.C);
    }

    [Fact]
    public void NegativeMergeIntersectsBoundaryOnce()
    {
        var fixture = TestBody();
        var tool = Rect(3.25, 1.5, 0.5, 2).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (3.75, 3), (3.75, 2), (4, 2), (4, 1), (3, 1), (3, 2), (3.25, 2), (3.25, 3), (0, 3));
        
        // var plot = new DebugPlot("NegativeMergeIntersectsBoundaryOnce");
        // plot.Add(fixture.Body, "Fixture");
        // plot.Add(tool, "Tool");
        // plot.Plot();
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        
        AssertBodyInner(fixture.Body, fixture.A, fixture.C);
    }

    [Fact]
    public void NegativeMergeIntersectsBoundaryTwice()
    {
        var fixture = TestBody();
        var tool = Rect(3.5, 1.25, 5, 0.5).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1.25),
            (6, 1.25), (6, 1), (5, 1), (5, 1.25), (4, 1.25), (4, 1), (3, 1), (3, 2), (4, 2), (4, 1.75), (5, 1.75),
            (5, 2), (6, 2), (6, 1.75), (7, 1.75),
            (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        
        AssertBodyInner(fixture.Body, fixture.A);
    }

    [Fact]
    public void NegativeMergeJoinsTwoInners()
    {
        var fixture = TestBody();
        var tool = Rect(3.5, 1.25, 2, 0.5).Reversed();
        var outside = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        var inside = Loop((3, 1), (4, 1), (4, 1.25), (5, 1.25), (5, 1), (6, 1), (6, 2), (5, 2), (5, 1.75),
            (4, 1.75), (4, 2), (3, 2)).Reversed();
        
        fixture.Body.Operate(tool);
        AssertLoop(outside, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, inside);
    }

    [Fact]
    public void NegativeMergeJoinsThreeInners()
    {
        var fixture = TestBody();
        var tool = Rect(1.5, 1.25, 4, 0.5).Reversed();
        var outside = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        var inside = Loop((3, 1), (4, 1), (4, 1.25), (5, 1.25), (5, 1), (6, 1), (6, 2), (5, 2), (5, 1.75),
            (4, 1.75), (4, 2), (3, 2), (3, 1.75), (2, 1.75), (2, 2), (1, 2), (1, 1),
            (2, 1), (2, 1.25), (3, 1.25)).Reversed();
        
        fixture.Body.Operate(tool);
        AssertLoop(outside, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, inside);
    }

    [Fact]
    public void BodyDoubleCutout()
    {
        // This test case comes from an actual application which was misbehaving
        var width = 1;
        var height = 2.7;

        // Use a temporary body to cut the main pocket
        var working = new Body(PointLoop.Rectangle(3.5, 5.0));

        var c0 = PointLoop.RoundedRectangle(height, width, 0.15, 8).Reversed();
        var c1 = PointLoop.Rectangle(0.62, 1.7).Reversed();
        var c3 = PointLoop.Rectangle(0.25, 0.25).Reversed();
        c1.Translate(c0.Bounds.Center.X - c1.Bounds.MinX - (1.25 / 2), c0.Bounds.MinY - c1.Bounds.MinY + .3);
        c3.Translate(c0.Bounds.MaxX - c3.Bounds.MinX - 0.01, c0.Bounds.MaxY - c3.Bounds.MaxY - 0.73);

        // Cut the main pocket
        working.Operate(c0);
        working.Operate(c1);
        working.Operate(c3);

        // Create the actual body
        var cut0 = working.Inners.First().Copy();
        var cut1 = cut0.Copy();
        var body = new Body(PointLoop.Rectangle(4, 5.0));

        var padding = (body.Bounds.Width - 2 * cut0.Bounds.Width) / 3;
        cut0.Translate(body.Bounds.MinX - cut0.Bounds.MinX + padding, body.Bounds.MinY - cut0.Bounds.MinY + 0.3);
        cut1.Translate(cut0.Bounds.MaxX - cut1.Bounds.MinX + padding, body.Bounds.MinY - cut1.Bounds.MinY + 0.3);

        body.Operate(cut0);
        body.Operate(cut1);

        var outer = ExpectedPoints((-2.5, -2), (2.5, -2), (2.5, 2), (-2.5, 2));
        AssertLoop(outer, body.Outer);
        AssertBodyInner(body, cut0, cut1);
    }

    [Fact]
    public void AvoidErroneousSubsumed()
    {
        // This test case comes from an actual application which was misbehaving
        var width = 1;
        var height = 2.7;

        // Use a temporary body to cut the main pocket
        var working = new Body(PointLoop.Rectangle(3.5, 5.0));

        var c0 = PointLoop.RoundedRectangle(height, width, 0.15, 8).Reversed();
        var c1 = PointLoop.Rectangle(0.62, 1.7).Reversed();
        var c3 = PointLoop.Rectangle(0.25, 0.25).Reversed();
        c1.Translate(c0.Bounds.Center.X - c1.Bounds.MinX - (1.25 / 2), c0.Bounds.MinY - c1.Bounds.MinY + .3);
        c3.Translate(c0.Bounds.MaxX - c3.Bounds.MinX - 0.01, c0.Bounds.MaxY - c3.Bounds.MaxY - 0.73);

        // Cut the main pocket
        working.Operate(c0);
        working.Operate(c1);
        working.Operate(c3);

        // Create the actual body
        var cut0 = working.Inners.First().Copy();
        var cut1 = cut0.Copy();
        var body = new Body(PointLoop.Rectangle(4, 5.0));

        var padding = (body.Bounds.Width - 2 * cut0.Bounds.Width) / 3;
        cut0.Translate(body.Bounds.MinX - cut0.Bounds.MinX + padding, body.Bounds.MinY - cut0.Bounds.MinY + 0.3);
        cut1.Translate(cut0.Bounds.MaxX - cut1.Bounds.MinX + padding, body.Bounds.MinY - cut1.Bounds.MinY + 0.3);

        var (result, _) = ShapeOperation.Operate(cut0, cut1);
        
        Assert.Equal(ShapeOperation.ResultType.Disjoint, result);
    }

    private TestFixture TestBody()
    {
        var a = Rect(1, 1, 1, 1).Reversed();
        var b = Rect(3, 1, 1, 1).Reversed();
        var c = Rect(5, 1, 1, 1).Reversed();
        
        var outer = Rect(0, 0, 7, 3);
        var inners = new List<PointLoop> { a, b, c };

        return new TestFixture(new Body(outer, inners), a, b, c);
    }

    private record TestFixture(Body Body, PointLoop A, PointLoop B, PointLoop C);

}