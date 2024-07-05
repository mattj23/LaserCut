using LaserCut.Geometry;

namespace LaserCut.Tests.Contours;

public class RelationsContourTests
{
    [Fact]
    public void DisjointContours()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(2, 2, 1, 1);
        
        Assert.Equal(ContourRelation.DisjointTo, c0.RelationTo(c1).Item1);
    }

    [Fact]
    public void EnclosedContoursWithoutIntersections()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 2);
        var c1 = BoundaryLoop.Rectangle(0.5, 0.5, 1, 1);
        
        Assert.Equal(ContourRelation.Encloses, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.EnclosedBy, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsPosToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2);
        
        Assert.Equal(ContourRelation.EnclosedBy, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Encloses, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsPosToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2).Reversed();
        
        Assert.Equal(ContourRelation.EnclosedBy, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Encloses, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsNegToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1).Reversed();
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2);
        
        Assert.Equal(ContourRelation.EnclosedBy, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Encloses, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void EnclosedContoursWithIntersectionsNegToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1).Reversed();
        var c1 = BoundaryLoop.Rectangle(0, 0, 1, 2).Reversed();
        
        Assert.Equal(ContourRelation.EnclosedBy, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Encloses, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void IntersectingContours()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var c1 = BoundaryLoop.Rectangle(0.5, 0, 1, 1);
        
        Assert.Equal(ContourRelation.Intersects, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Intersects, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedPosToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3);
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1).Reversed();
        
        Assert.Equal(ContourRelation.Intersects, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Intersects, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedPosToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3);
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1);
        
        Assert.Equal(ContourRelation.Intersects, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Intersects, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedNegToPos()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3).Reversed();
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1);
        
        Assert.Equal(ContourRelation.Intersects, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Intersects, c1.RelationTo(c0).Item1);
    }

    [Fact]
    public void SharedSideIntersectingNotEnclosedNegToNeg()
    {
        var c0 = BoundaryLoop.Rectangle(0, 0, 2, 3).Reversed();
        var c1 = BoundaryLoop.Rectangle(2, 1, 1, 1).Reversed();
        
        Assert.Equal(ContourRelation.Intersects, c0.RelationTo(c1).Item1);
        Assert.Equal(ContourRelation.Intersects, c1.RelationTo(c0).Item1);
    }

}