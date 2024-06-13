namespace LaserCut.Geometry;

public enum ContourRelation
{
    /// <summary>
    /// Disjoint means that the first contour and the second contour do not share any of the same space.  They do not
    /// intersect and neither encloses the other without touching.  They are completely separate from each other in
    /// any interpretation.
    /// </summary>
    DisjointTo,
    
    /// <summary>
    /// This means that the first contour is completely enclosed by the second contour.  They may intersect, but the
    /// first contour never extends outside the space defined by the second contour.
    /// </summary>
    EnclosedBy,
    
    /// <summary>
    /// This means the first contour completely encloses the second contour.  They may intersect, but the second
    /// contour never extends outside the space defined by the first contour.
    /// </summary>
    Encloses,
    
    /// <summary>
    /// Each contour occupies some of the same space as the other contour, and some space unique to itself.  The
    /// boundaries intersect, and they share some space, but neither completely encloses the other.
    /// </summary>
    Intersects
}