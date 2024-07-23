using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// Represents an intersection between two geometric elements, with the corresponding positions on each element.
/// </summary>
/// <param name="First"></param>
/// <param name="Second"></param>
public readonly record struct IntersectionPair(Position First, Position Second)
{
    /// <summary>
    /// Gets whether this intersection pair is empty, meaning that one or both of the positions are null. Because this
    /// is a value type, it is not possible to have a null IntersectionPair, so use this property to check for
    /// emptiness (as might be returned by `FirstOrDefault`)
    /// </summary>
    public bool Empty => First.Empty || Second.Empty;
    
    /// <summary>
    /// Gets the point of intersection between the two elements.
    /// </summary>
    public Point2D Point => First.Surface.Point;
    
    /// <summary>
    /// Returns true if the first element 'exits' the second element at the intersection. This requires the direction
    /// of the first surface to be pointing in the same direction as the normal of the second surface, and the position
    /// of the intersection to be less than the length of the first element. Essentially, there must be *some* amount
    /// of the first element beyond the intersection point.
    /// </summary>
    public bool FirstExitsSecond => FirstDirDotSecondNorm > 0 && First.L < First.Element.Length - GeometryConstants.DistEquals;
    
    /// <summary>
    /// Returns true if the first element 'enters' the second element at the intersection. This requires the direction
    /// of the first surface to be pointing in the opposite direction as the normal of the second surface, and the
    /// position of the intersection to be less than the length of the first element. Essentially, there must be *some*
    /// of the first element beyond the intersection point.
    /// </summary>
    public bool FirstEntersSecond => FirstDirDotSecondNorm < 0 && First.L < First.Element.Length - GeometryConstants.DistEquals;
    
    public bool SecondExitsFirst => SecondDirDotFirstNorm > 0 && Second.L < Second.Element.Length - GeometryConstants.DistEquals;
    
    public bool SecondEntersFirst => SecondDirDotFirstNorm < 0 && Second.L < Second.Element.Length - GeometryConstants.DistEquals;
    
    /// <summary>
    /// Shortcut for the dot product of the direction of the first surface and the normal of the second surface.
    /// </summary>
    private double FirstDirDotSecondNorm => First.Surface.Direction.DotProduct(Second.Surface.Normal);
    
    /// <summary>
    /// Shortcut for the dot product of the direction of the second surface and the normal of the first surface.
    /// </summary>
    private double SecondDirDotFirstNorm => Second.Surface.Direction.DotProduct(First.Surface.Normal);

    /// <summary>
    /// Checks if the two intersection pairs are equivalent, meaning that the elements are the same and the point of
    /// intersection is the same.  However, the first and second elements/positions may be swapped.  This is useful
    /// for identifying redundant intersections.
    /// </summary>
    /// <param name="other">The other intersection pair to compare against</param>
    /// <returns></returns>
    public bool IsEquivalentTo(IntersectionPair other)
    {
        var elementsMatch = (First.Element == other.First.Element && Second.Element == other.Second.Element) ||
                            (First.Element == other.Second.Element && Second.Element == other.First.Element);

        return elementsMatch && Point.DistanceTo(other.Point) < GeometryConstants.DistEquals;
    }
    
    /// <summary>
    /// Returns a new IntersectionPair with the first and second elements swapped.  
    /// </summary>
    /// <returns></returns>
    public IntersectionPair Swapped() => new(Second, First);
}