using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// Represents an intersection between two geometric elements, with the corresponding positions on each element.
/// </summary>
/// <param name="First"></param>
/// <param name="Second"></param>
public readonly record struct IntersectionPair(Position First, Position Second)
{
    public bool Empty => First.Empty || Second.Empty;
    
    public Point2D Point => First.Surface.Point;
    
    /// <summary>
    /// Returns true if the first element 'exits' the second element at the intersection. This requires the direction
    /// of the first surface to be pointing in the same direction as the normal of the second surface, and the position
    /// of the intersection to be less than the length of the first element. Essentially, there must be *some* amount
    /// of the first element beyond the intersection point.
    /// </summary>
    public bool FirstExitsSecond => FirstDirDotSecondNorm > 0 && First.LengthAlong < First.Element.Length;

    /// <summary>
    /// Returns true if the first element 'enters' the second element at the intersection. This requires the direction
    /// of the first surface to be pointing in the opposite direction as the normal of the second surface, and the
    /// position of the intersection to be less than the length of the first element. Essentially, there must be *some*
    /// of the first element beyond the intersection point.
    /// </summary>
    public bool FirstEntersSecond => FirstDirDotSecondNorm < 0 && First.LengthAlong < First.Element.Length;
    
    private double FirstDirDotSecondNorm => First.Surface.Direction.DotProduct(Second.Surface.Normal);
    
}