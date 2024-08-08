using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia.HitTesting;

public interface IHitTestable : IHasBounds
{
    bool Hit(Point2D point);

}