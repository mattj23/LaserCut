using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Helpers;

public static class MakePoint
{
    public static Point2D AtDirection(double x0, double y0, double dx, double dy, double r)
    {
        return new Point2D(x0, y0) + r * new Vector2D(dx, dy).Normalize();
    }

    public static Point2D AtDirection(double dx, double dy, double r) => AtDirection(0, 0, dx, dy, r);

    public static Point2D AtDirection(Point2D point, double angle, double distance)
    {
        var x = point.X + distance * Math.Cos(angle);
        var y = point.Y + distance * Math.Sin(angle);
        return new Point2D(x, y);
    }
}

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