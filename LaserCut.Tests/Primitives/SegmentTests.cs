using LaserCut.Geometry.Primitives;

namespace LaserCut.Tests.Primitives;

public class SegmentTests
{
    [Fact]
    public void SplitAfterNull()
    {
        var seg = new Segment(10, 10, 11, 10, 0);
        var after = seg.SplitAfter(1.0);
        Assert.Null(after);
    }
    
    [Fact]
    public void SplitBeforeNull()
    {
        var seg = new Segment(10, 10, 11, 10, 0);
        var before = seg.SplitBefore(0.0);
        Assert.Null(before);
    }

    [Fact]
    public void SplitAfter()
    {
        var seg = new Segment(10, 10, 11, 10, 0);
        var after = seg.SplitAfter(0.75);
        Assert.NotNull(after);
        Assert.Equal(10.75, after.Start.X, 1e-10);
        Assert.Equal(10, after.Start.Y, 1e-10);
        Assert.Equal(11, after.End.X, 1e-10);
        Assert.Equal(10, after.End.Y, 1e-10);
    }
    
    [Fact]
    public void SplitBefore()
    {
        var seg = new Segment(10, 10, 11, 10, 0);
        var before = seg.SplitBefore(0.25);
        Assert.NotNull(before);
        Assert.Equal(10, before.Start.X, 1e-10);
        Assert.Equal(10, before.Start.Y, 1e-10);
        Assert.Equal(10.25, before.End.X, 1e-10);
        Assert.Equal(10, before.End.Y, 1e-10);
    }
    
    [Fact]
    public void StressTestIntersections()
    {
        var r = new RandomValues();
        var b = new Aabb2(-10, -10, 10, 10);
        for (var i = 0; i < 10000; i++)
        {
            var seg0 = r.Segment(b);
            var (seg1, t0, t1) = r.IntersectingSegment(seg0);

            var (a0, a1) = seg0.IntersectionParams(seg1);

            Assert.Equal(a0, t0, 1e-10);
            Assert.Equal(a1, t1, 1e-10);

            var s = seg0.Intersections(seg1);
            Assert.Single(s);
            Assert.Equal(t1, s.First().L, 1e-10);
        }
    }


    [Fact]
    public void StressTestNonIntersections()
    {
        var b = new Aabb2(-10, -10, 10, 10);
        var r = new RandomValues();
        for (var i = 0; i < 10000; i++)
        {
            var seg0 = r.Segment(b);
            var seg1 = r.NonIntersectingSegment(seg0);
            
            var s = seg1.Intersections(seg0);
            Assert.Empty(s);
        }
    }
    
    
    
}