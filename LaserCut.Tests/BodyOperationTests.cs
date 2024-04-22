using LaserCut.Geometry;
using LaserCut.Tests.Helpers;

namespace LaserCut.Tests;

public class BodyOperationTests : PointLoopTestBase
{

    [Fact]
    private void PositiveMergeSimple()
    {
        var fixture = TestBody();
        var tool = Rect(7, 1, 1, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B, fixture.C);
    }

    [Fact]
    private void PositiveMergeRemovesInner()
    {
        var fixture = TestBody();
        var tool = Rect(5, 1, 3, 1);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (8, 1), (8, 2), (7, 2), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B);
    }
    
    [Fact]
    private void PositiveMergeChangesInner()
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
    private void PositiveMergeRemovesInnerOnly()
    {
        var fixture = TestBody();
        var tool = Rect(2.5, 0.5, 2, 2);
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.C);
    }
    
    [Fact]
    private void PositiveMergeSplitsInner()
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
    private void NegativeMergeSimple()
    {
        var fixture = TestBody();
        var tool = Rect(6.5, 1, 1, 1).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1), (6.5, 1), (6.5, 2), (7, 2), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, fixture.B, fixture.C);
    }

    [Fact]
    private void NegativeMergeSubsumesInnerSimple()
    {
        var fixture = TestBody();
        var tool = Rect(2.5, 0.5, 2, 2).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, tool, fixture.C);
    }
    
    // [Fact]
    // private void NegativeMergeMiddleJoin()
    // {
    //     var fixture = TestBody();
    //     var tool = Rect(2, 1, 1, 1.5).Reversed();
    //     var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
    //     
    //     fixture.Body.Operate(tool);
    //     AssertLoop(expected, fixture.Body.Outer);
    //     AssertBodyInner(fixture.Body, fixture.A, tool, fixture.C);
    // }

    
    [Fact]
    private void NegativeMergeSubsumesInnerWithIntersections()
    {
        var fixture = TestBody();
        var tool = Rect(3, 1, 1, 1.5).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        AssertBodyInner(fixture.Body, fixture.A, tool, fixture.C);
    }

    [Fact]
    private void NegativeMergeIntersectsBoundaryOnce()
    {
        var fixture = TestBody();
        var tool = Rect(3.25, 1.5, 0.5, 2).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 3), (3.75, 3), (3.75, 2), (4, 2), (4, 1), (3, 1), (3, 2), (3.25, 2), (3.25, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        
        AssertBodyInner(fixture.Body, fixture.A, fixture.C);
    }

    [Fact]
    private void NegativeMergeIntersectsBoundaryTwice()
    {
        var fixture = TestBody();
        var tool = Rect(3.5, 1.25, 5, 0.5).Reversed();
        var expected = ExpectedPoints((0, 0), (7, 0), (7, 1.25),
            (6, 1.25), (6, 1), (5, 1), (5, 1.25), (4, 1.25), (4, 1), (3, 1), (3, 2), (4, 2), (4, 1.75), (5, 1.75),
            (5, 2), (6, 2), (6, 1.75), (7, 1.75),
            (7, 3), (0, 3));
        
        fixture.Body.Operate(tool);
        AssertLoop(expected, fixture.Body.Outer);
        
        AssertBodyInner(fixture.Body, fixture.A, fixture.C);
    }

    [Fact]
    private void NegativeMergeJoinsTwoInners()
    {
        Assert.True(false);
    }

    [Fact]
    private void NegativeMergeJoinsThreeInners()
    {
        Assert.True(false);
    }

    [Fact]
    private void NegativeMergeJoinsInnersAndMergesBoundary()
    {
        Assert.True(false);
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