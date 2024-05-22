using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public interface IBvhTest
{
    bool RoughIntersects(Aabb2 box);

    Position[] Intersections(IContourElement element);
}