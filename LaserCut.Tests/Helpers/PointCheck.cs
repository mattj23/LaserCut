using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Helpers;

public class PointCheck : IEqualityComparer<Point2D>
{
    private readonly double _tolerance;

    private PointCheck(double tolerance)
    {
        _tolerance = tolerance;
    }

    public bool Equals(Point2D x, Point2D y)
    {
        return x.DistanceTo(y) < _tolerance;
    }

    public int GetHashCode(Point2D obj)
    {
        return obj.GetHashCode();
    }
    
    public static PointCheck Tol(double tolerance) => new(tolerance);

    public static PointCheck Default => Tol(GeometryConstants.DistEquals);
}