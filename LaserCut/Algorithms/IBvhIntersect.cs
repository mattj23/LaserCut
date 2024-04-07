using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public interface IBvhIntersect
{
    bool RoughIntersects(Aabb2 box);

    SegIntersection? Intersects(Segment segment);
}