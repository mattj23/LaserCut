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
        for (var i = 0; i < 10000; i++)
        {
            var start0 = r.Point(100);
            var seg0 = new Segment(start0, start0 + r.UnitVector() * r.Double(0.1, 5.0), 0);
            var t0 = r.Double(0.0, seg0.Length);

            var angle = r.Double(0.1, 179.9) * r.Flip();
            var d2 = seg0.Direction.Transformed(Isometry.Rotate(angle));
            var l1 = r.Double(0.1, 5.0);
            var t1 = r.Double(0, l1);
            var pi = seg0.PointAt(t0);
            var seg1 = new Segment(pi - d2 * t1, pi + d2 * l1, 1);

            var (a0, a1) = seg0.IntersectionParams(seg1);

            Assert.Equal(a0, t0, 1e-10);
            Assert.Equal(a1, t1, 1e-10);

            var s = seg0.Intersects(seg1);
            Assert.NotNull(s);
            Assert.Equal(t1, s.Value.T, 1e-10);
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
            
            var s = seg0.Intersects(seg1);
            Assert.Null(s);
        }
    }
    
    
    
}