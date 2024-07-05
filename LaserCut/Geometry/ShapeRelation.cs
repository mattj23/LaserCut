namespace LaserCut.Geometry;

public enum ShapeRelation
{
    /// <summary>
    /// Disjoint means that the two shapes do not share any of the same space (the intersection of the two shapes is
    /// empty)
    /// </summary>
    DisjointTo,
    
    /// <summary>
    /// This means that the first shape is a subset of the second shape...that is, the first shape is completely
    /// contained within the second shape.
    /// </summary>
    IsSubsetOf,
    
    /// <summary>
    /// This means that the first shape is a superset of the second shape...that is, the second shape is completely
    /// contained within the first shape.
    /// </summary>
    IsSupersetOf,
    
    /// <summary>
    /// This means that the two shapes share some of the same space, but also have some space unique to themselves.
    /// </summary>
    Intersects
}