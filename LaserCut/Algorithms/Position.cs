using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

/// <summary>
/// Represents a position on a geometric manifold element, identified by the length along the element.
/// </summary>
/// <param name="L">The length (in world units) along the associated element referenced by the position.</param>
/// <param name="Element">The element of the manifold on which the position exists.</param>
public readonly record struct Position(double L, IContourElement Element)
{
    /// <summary>
    /// Gets the surface triplet (point, direction, normal) at the position on the element.
    /// </summary>
    public SurfacePoint Surface => Element.AtLength(L);

    /// <summary>
    /// Gets whether this position is empty, meaning that the element is null. Because this is a value type, it is not
    /// possible to have a null Position, so use this property to check for emptiness (as might be returned by
    /// `FirstOrDefault`).
    /// </summary>
    public bool Empty => Element == null;
}
