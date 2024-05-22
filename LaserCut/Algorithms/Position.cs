using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

/// <summary>
/// Represents a position on a geometric manifold element, identified by the length along the element.
/// </summary>
/// <param name="LengthAlong"></param>
/// <param name="Element"></param>
public readonly record struct Position(double LengthAlong, IContourElement Element)
{
    public SurfacePoint Surface => Element.AtLength(LengthAlong);

    public bool Empty => Element == null;
}
