using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Tests.Contours;

public class IntersectionContourTests
{
    [Fact]
    public void IntersectionPairOrderCorrect()
    {
        // Verifies that the values in the intersection pairs are returned as the documentation reports
        var c0 = Contour.Rectangle(0, 0, 2, 2);
        var c1 = Contour.Circle(0, 0, 1);

        var pairs = c0.IntersectionPairs(c1);
        
        Assert.Equal(2, pairs.Length);
        Assert.IsType<Segment>(pairs[0].First.Element);
        Assert.IsType<Arc>(pairs[0].Second.Element);
    }

    [Fact]
    public void RelationIntersects()
    {
        var c0 = Contour.Circle(0, 0, 1);
        var c1 = Contour.Circle(1, 0, 1);
        
        Assert.Equal(ContourRelation.Intersects, c0.RelationTo(c1));
    }
    
    [Fact]
    public void RelationEncloses()
    {
        var c0 = Contour.Circle(0, 0, 2);
        var c1 = Contour.Circle(0, 0, 1);
        
        Assert.Equal(ContourRelation.Encloses, c0.RelationTo(c1));
        Assert.Equal(ContourRelation.EnclosedBy, c1.RelationTo(c0));
    }
    
    [Fact]
    public void RelationDisjoint()
    {
        var c0 = Contour.Circle(0, 0, 1);
        var c1 = Contour.Circle(3, 0, 1);
        
        Assert.Equal(ContourRelation.DisjointTo, c0.RelationTo(c1));
    }
    
}