using LaserCut.Geometry;

namespace LaserCut.Tests.Contours;

public class RelationsContourTests
{
    [Fact]
    public void DisjointContours()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(2, 2, 1, 1);
        
        Assert.Equal(BoundaryRelation.DisjointTo, c0.LoopRelationTo(c1).Item1);
    }

    [Fact]
    public void EnclosedContoursWithoutIntersections()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 2);
        var c1 = BoundaryLoop.Rectangle(0.5, 0.5, 1, 1);
        
        Assert.Equal(BoundaryRelation.Encloses, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.EnclosedBy, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsPosToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2);
        
        Assert.Equal(BoundaryRelation.EnclosedBy, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Encloses, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsPosToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2).Reversed();
        
        Assert.Equal(BoundaryRelation.EnclosedBy, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Encloses, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsNegToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1).Reversed();
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2);
        
        Assert.Equal(BoundaryRelation.EnclosedBy, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Encloses, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsNegToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1).Reversed();
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2).Reversed();
        
        Assert.Equal(BoundaryRelation.EnclosedBy, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Encloses, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void IntersectingContours()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(0.5, 0, 1, 1);
        
        Assert.Equal(BoundaryRelation.Intersects, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Intersects, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedPosToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3);
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1).Reversed();
        
        Assert.Equal(BoundaryRelation.Intersects, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Intersects, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedPosToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3);
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1);
        
        Assert.Equal(BoundaryRelation.Intersects, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Intersects, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedNegToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3).Reversed();
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1);
        
        Assert.Equal(BoundaryRelation.Intersects, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Intersects, c1.LoopRelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedNegToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3).Reversed();
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1).Reversed();
        
        Assert.Equal(BoundaryRelation.Intersects, c0.LoopRelationTo(c1).Item1);
        Assert.Equal(BoundaryRelation.Intersects, c1.LoopRelationTo(c0).Item1);
    }

}