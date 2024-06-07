using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Tests.Algorithms;

public class IntersectionOperationTests
{
    [Fact]
    public void IntersectionPairEquivalentDuplicate()
    {
        var e0 = new Segment(0, 0, 1, 0, 0);
        var e1 = new Segment(0.5, 0, 0.5, 1, 1);
        var m0 = e0.MatchIntersections(e0.Intersections(e1)).First();
        var m1 = e0.MatchIntersections(e0.Intersections(e1)).First();
        
        Assert.True(m0.IsEquivalentTo(m1));
    }
    
    [Fact]
    public void IntersectionPairEquivalentReversed()
    {
        var e0 = new Segment(0, 0, 1, 0, 0);
        var e1 = new Segment(0.5, 0, 0.5, 1, 1);
        var m0 = e0.MatchIntersections(e0.Intersections(e1)).First();
        var m1 = e1.MatchIntersections(e1.Intersections(e0)).First();
        
        Assert.True(m0.IsEquivalentTo(m1));
    }
    
    [Fact]
    public void IntersectionPairEquivalentDifferent()
    {
        // Half circle arc from 2,0 to 0,0 through 1,1
        var e0 = new Arc(1, 0, 1, 0, Math.PI, 0);
        
        // Horizontal segment at y=0.5
        var e1 = new Segment(0, 0.5, 2, 0.5, 1);
        
        // There should be two intersections resulting
        var i = e0.MatchIntersections(e0.Intersections(e1));
        Assert.Equal(2, i.Length);
        
        Assert.False(i.First().IsEquivalentTo(i.Last()));
    }
    
}