using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Tests;

public class BvhTests
{
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
            
            Assert.Equal(expected, results);

        }
        
    }
    
}