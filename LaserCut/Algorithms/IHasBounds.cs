using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public interface IHasBounds
{
    Aabb2 Bounds { get; }
}