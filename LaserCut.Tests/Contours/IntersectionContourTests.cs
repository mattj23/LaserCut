using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;

namespace LaserCut.Tests.Contours;

public class IntersectionContourTests
{
    [Fact]
    public void IntersectionPairOrderCorrect()
    {
        // Verifies that the values in the intersection pairs are returned as the documentation reports
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 2);
        var c1 = BoundaryLoop.Circle(0, 0, 1);

        var pairs = c0.IntersectionPairs(c1);
        
        Assert.Equal(2, pairs.Length);
        Assert.IsType<Segment>(pairs[0].First.Element);
        Assert.IsType<Arc>(pairs[0].Second.Element);
    }

    [Fact]
    public void RelationIntersects()
    {
        var c0 = BoundaryLoop.Circle(0, 0, 1);
        var c1 = BoundaryLoop.Circle(1, 0, 1);
        
        Assert.Equal(BoundaryRelation.Intersects, c0.LoopRelationTo(c1).Item1);
    }
    
    [Fact]
    public void RelationEncloses()
    {
        var c0 = BoundaryLoop.Circle(0, 0, 2);
        var c1 = BoundaryLoop.Circle(0, 0, 1);
        
        Assert.Equal(BoundaryRelation.Encloses, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.EnclosedBy, c1.LoopRelationTo(c0).Item1);
    }
    
    [Fact]
    public void RelationDisjoint()
    {
        var c0 = BoundaryLoop.Circle(0, 0, 1);
        var c1 = BoundaryLoop.Circle(3, 0, 1);
        
        Assert.Equal(BoundaryRelation.DisjointTo, c0.LoopRelationTo(c1).Item1);
    }

    [Fact]
    public void CircleNoSelfIntersection()
    {
        var c = BoundaryLoop.Circle(10, 10, 1);
        var i = c.SelfIntersections();
        
        Assert.Empty(i);
    }

    [Fact]
    public void HalfCircleNoSelfIntersection()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();

        cursor.SegAbs(0, 0);
        cursor.ArcAbs(2, 0, 1, 0, false);
        
        var i = c.SelfIntersections();
        Assert.Empty(i);
    }

    [Fact]
    public void HalfCircleSelfIntersection()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();

        var arc1 = cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        var seg2 = cursor.SegAbs(0, 1);

        var i = c.SelfIntersections();
        Assert.Single(i);
        var m = i.First();
        var (ai, si) = m.First.Element.Index == arc1 ? (m.First, m.Second) : (m.Second, m.First);
        
        Assert.Equal(arc1, ai.Element.Index);
        Assert.Equal(seg2, si.Element.Index);

        Assert.Equal(0.4, m.Point.X, 1e-6);
        Assert.Equal(0.8, m.Point.Y, 1e-6);
    }

    [Fact]
    public void HalfCircleTwoSelfIntersections()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();

        var arc1 = cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        var seg2 = cursor.SegAbs(0, 0.5);
        cursor.SegAbs(2, 0.5);

        var i = c.SelfIntersections();
        Assert.Equal(2, i.Length);
        Assert.All(i, m => Assert.True(m.HasIndices(arc1, seg2)));

        var points = i.Select(x => x.Point).OrderBy(x => x.X).ToArray();
        Assert.Equal(0.133974, points[0].X, 1e-5);
        Assert.Equal(0.5, points[0].Y, 1e-5);
        
        Assert.Equal(1.866025, points[1].X, 1e-5);
        Assert.Equal(0.5, points[1].Y, 1e-5);
    }

    [Fact]
    public void HalfCircleMiddleSelfIntersection()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();

        cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        cursor.SegAbs(1, 1);
        
        var i = c.SelfIntersections();
        Assert.Equal(2, i.Length);
        Assert.All(i, x => Assert.Equal(1.0, x.Point.X, 1e-6));
        Assert.All(i, x => Assert.Equal(1.0, x.Point.Y, 1e-6));
    }

    [Fact]
    public void SplitAtSelfIntersection()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();
        cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        cursor.SegAbs(0, 1);

        var i = c.SelfIntersections().First();

        var (s0, s1) = c.SplitAtSelfIntersection(i);

        var (a, b) = i.First.Element is Arc ? (s0, s1) : (s1, s0);
        
        Assert.False(a.IsPositive);
        Assert.True(b.IsPositive);
        Assert.Equal(c.Area, a.Area + b.Area, 1e-6);
    }

    [Fact]
    public void NonSelfIntersectingReturnsOriginalContour()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();

        cursor.SegAbs(0, 0);
        cursor.ArcAbs(2, 0, 1, 0, false);

        var result = c.NonSelfIntersectingLoops();
        Assert.Single(result);
        Assert.Equal(c, result.First());
    }

    [Fact]
    public void SingleSelfIntersectionReturnsTwoContours()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();
        cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        cursor.SegAbs(0, 1);
        
        var result = c.NonSelfIntersectingLoops();
        Assert.Equal(2, result.Length);
        Assert.Equal(c.Area, result.Sum(x => x.Area), 1e-6);
    }

    [Fact]
    public void MiddleSelfIntersectionReturnsTwoContours()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();
        cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        cursor.SegAbs(1, 1);
        
        var result = c.NonSelfIntersectingLoops();
        Assert.Equal(2, result.Length);
        Assert.Equal(c.Area, result.Sum(x => x.Area), 1e-6);
    }

    [Fact]
    public void DoubleSelfIntersectionReturnsThreeContours()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();
        cursor.ArcAbs(2, 0, 1, 0, false);
        cursor.SegAbs(0, 0);
        cursor.SegAbs(0, 0.5);
        cursor.SegAbs(2, 0.5);
        
        var result = c.NonSelfIntersectingLoops();
        Assert.Equal(3, result.Length);
        Assert.Equal(c.Area, result.Sum(x => x.Area), 1e-6);
    }
}