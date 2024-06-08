namespace LaserCut.Algorithms;

public enum MutateResult
{
    /// <summary>
    /// The shape and the tool are completely disjoint, so the original shape should be left unmodified
    /// </summary>
    Disjoint,
    
    /// <summary>
    /// The shape is completely subsumed by the tool, so the result is the tool
    /// </summary>
    Subsumed,
    
    /// <summary>
    /// The shape is completely destroyed by the tool, and no loop remains
    /// </summary>
    Destroyed,
    
    /// <summary>
    /// The shape and the tool intersect and the result of an operation is a set of one or more loops
    /// </summary>
    Merged,
    
    /// <summary>
    /// The shape completely encloses the tool. If the two entities have the same polarity, the result should be
    /// the shape as it is effectively a no-op. If the entities have opposite polarity the conceptual result is
    /// the shape with the tool as a new internal boundary.
    /// </summary>
    ShapeEnclosesTool
}

