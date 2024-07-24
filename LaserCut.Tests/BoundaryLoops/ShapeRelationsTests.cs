using LaserCut.Geometry;

namespace LaserCut.Tests.BoundaryLoops;

public class ShapeRelationsTests
{
    [Fact]
    public void EnclosedNegative()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 3, 3);
        var loop1 = BoundaryLoop.Rectangle(1, 1, 1, 1).Reversed();

        var (relation, _) = loop0.ShapeRelationTo(loop1);
        
        Assert.Equal(ShapeRelation.Intersects, relation);
    }
    
    [Fact]
    public void UnEnclosedNegative()
    {
        var loop0 = BoundaryLoop.Rectangle(0, 0, 3, 3);
        var loop1 = BoundaryLoop.Rectangle(10, 1, 1, 1).Reversed();

        var (relation, _) = loop0.ShapeRelationTo(loop1);
        
        Assert.Equal(ShapeRelation.IsSubsetOf, relation);
    }
    
}