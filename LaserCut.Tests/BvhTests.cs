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
            
            var bvh = new Bvh(segments);
            var results = bvh.Intersections(seg);
            
            Assert.Single(results);
            Assert.Equal(seg1, results[0].Element);
            Assert.Equal(t1, results[0].LengthAlong, 1e-10);
        }
    }
    
    [Fact]
    public void StressTestSegmentIntersections()
    {
        var b0 = new Aabb2(-10, 0, 5, 10);
        var b1 = new Aabb2(-5, 0, 10, 10);
        var r = new RandomValues();
        
        for (int i = 0; i < 10000; i++)
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
            var bvh = new Bvh(segments1);
            var results = bvh.Intersections(test);

            var exp = expected
                .OrderBy(x => x.Segment.Index)
                .Select(x => (x.Segment.Index, x.T))
                .ToArray();
            
            var tst = results
                .OrderBy(x => x.Element.Index)
                .Select(x => (x.Element.Index, x.LengthAlong))
                .ToArray();
            
            Assert.Equal(exp, tst);
        }
        
    }

    [Fact]
    public void StressTestOtherBvh()
    {
        var b0 = new Aabb2(-10, 0, 5, 10);
        var b1 = new Aabb2(-5, 0, 10, 10);
        var r = new RandomValues();
        
        for (int i = 0; i < 10000; i++)
        {
            var segments0 = r.Segments(b0, r.Int(5, 30));
            var segments1 = r.Segments(b1, r.Int(5, 30));
            
            // Brute force the expected results
            var expected = new List<SegPairIntersection>();
            foreach (var test in segments0)
            {
                foreach (var seg in segments1)
                {
                    if (test.IntersectsAsPair(seg) is { } s)
                    {
                        expected.Add(s);
                    }
                }
            }
            
            // Test the BVH
            var bvh0 = new Bvh(segments0);
            var bvh1 = new Bvh(segments1);
            var results = bvh0.Intersections(bvh1);

            var exp = expected
                .OrderBy(x => x.Segment0.Index)
                .ThenBy(x => x.Segment1.Index)
                .Select(x => (x.Segment0.Index, x.Segment1.Index, Math.Round(x.T0, 6), Math.Round(x.T1, 6)))
                .ToArray();
            
            var tst = results
                .OrderBy(x => x.First.Element.Index)
                .ThenBy(x => x.Second.Element.Index)
                .Select(x => (x.First.Element.Index, x.Second.Element.Index, 
                    Math.Round(x.First.LengthAlong, 6), Math.Round(x.Second.LengthAlong, 6)))
                .ToArray();
            
            Assert.Equal(exp, tst);
        }
    }
    
}