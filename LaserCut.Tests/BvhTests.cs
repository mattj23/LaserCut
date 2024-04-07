using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Tests;

public class BvhTests
{
    [Fact]
    public void StressTestKnownIntersections()
    {
        var b = new Aabb2(-10, -10, 10, 10);
        var r = new RandomValues();
        for (int i = 0; i < 1000; i++)
        {
            var seg = r.Segment(b);
            var (seg1, t0, t1) = r.IntersectingSegment(seg, 10);

            var segments = new[]
            {
                r.NonIntersectingSegment(seg, 1),
                r.NonIntersectingSegment(seg, 2),
                r.NonIntersectingSegment(seg, 3),
                seg1,
                r.NonIntersectingSegment(seg, 4),
                r.NonIntersectingSegment(seg, 5),
                r.NonIntersectingSegment(seg, 6),
            };
            
            var bvh = new BvhNode(segments);
            var results = bvh.Intersections(seg);
            
            Assert.Single(results);
            Assert.Equal(seg1, results[0].Segment);
            Assert.Equal(t1, results[0].T, 1e-10);
        }
    }
    
    [Fact]
    public void StressTestSegmentIntersections()
    {
        var b0 = new Aabb2(-10, 0, 5, 10);
        var b1 = new Aabb2(-5, 0, 10, 10);
        var r = new RandomValues();
        
        for (int i = 0; i < 1; i++)
        {
            var test = r.Segment(b0);
            var segments1 = r.Segments(b1, 20);
            
            // Brute force the expected results
            var expected = new List<SegIntersection>();
            foreach (var seg in segments1)
            {
                if (test.Intersects(seg) is { } s)
                {
                    expected.Add(s);
                }
            }
            
            // Test the BVH
            var bvh = new BvhNode(segments1);
            var results = bvh.Intersections(test);

            var exp = expected
                .OrderBy(x => x.Segment.Index)
                .Select(x => (x.Segment.Index, x.T))
                .ToArray();
            
            var tst = results
                .OrderBy(x => x.Segment.Index)
                .Select(x => (x.Segment.Index, x.T))
                .ToArray();
            
            Assert.Equal(exp, tst);
        }
        
    }
    
}