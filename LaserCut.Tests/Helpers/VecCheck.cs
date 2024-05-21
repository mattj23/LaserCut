using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Helpers;

public class VecCheck : IEqualityComparer<Vector2D>
{
    private readonly double _tolerance;

    private VecCheck(double tolerance)
    {
        _tolerance = tolerance;
    }

    public bool Equals(Vector2D x, Vector2D y)
    {
        return (x - y).Length < _tolerance;
    }

    public int GetHashCode(Vector2D obj)
    {
        return obj.GetHashCode();
    }
    
    public static VecCheck Tol(double tolerance) => new(tolerance);

    public static VecCheck Default => Tol(GeometryConstants.DistEquals);
}