using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public static class MatchExtensions
{
    /// <summary>
    /// Checks if the geometric properties, but NOT positions, of two boundary elements match.  If both elements are
    /// segments, it checks if the directions are parallel.  If both elements are arcs, it checks if the centers are
    /// the same and the arcs are both clockwise or both counterclockwise.  This is used for identifying redundant
    /// portions of boundary curves in cases where it is already known that the vertices of interest match.
    /// </summary>
    public static bool Matches(this IBoundaryElement a, IBoundaryElement b)
    {
        if (a is Segment segA && b is Segment segB)
        {
            return segA.Direction.DotProduct(segB.Direction) > 1.0 - GeometryConstants.DistEquals;
        }

        if (a is Arc arcA && b is Arc arcB)
        {
            return arcA.Center.DistanceTo(arcB.Center) < GeometryConstants.DistEquals && arcA.IsCcW == arcB.IsCcW;
        }

        return false;
    }
    
    
}