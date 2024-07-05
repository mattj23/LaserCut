using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

/// <summary>
/// An interface for objects that can be intersected with a bounding volume hierarchy, including with the elements
/// inside the hierarchy.
/// </summary>
public interface IBvhIntersect
{
    bool RoughIntersects(Aabb2 box);

    Position[] Intersections(IBoundaryElement element);
}