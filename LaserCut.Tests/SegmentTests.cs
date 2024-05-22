using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class SegmentTests
{
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