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

public class PointCheck : IEqualityComparer<Point2D>, IEqualityComparer<Point3D>
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
    public bool Equals(Point3D x, Point3D y)
    {
        return x.DistanceTo(y) < _tolerance;
    }

    public int GetHashCode(Point3D obj)
    {
        return obj.GetHashCode();
    }
}

public class VectorCheck : IEqualityComparer<Vector3D>, IEqualityComparer<UnitVector3D>
{
    private readonly double _tolerance;

    private VectorCheck(double tolerance)
    {
        _tolerance = tolerance;
    }


    public static VectorCheck Tol(double tolerance) => new(tolerance);

    public static VectorCheck Default => Tol(GeometryConstants.DistEquals);
    public bool Equals(Vector3D x, Vector3D y)
    {
        return x.ToPoint3D().DistanceTo(y.ToPoint3D()) < _tolerance;
    }

    public int GetHashCode(Vector3D obj)
    {
        return obj.GetHashCode();
    }

    public bool Equals(UnitVector3D x, UnitVector3D y)
    {
        return Equals(x.ToVector3D(), y.ToVector3D());
    }

    public int GetHashCode(UnitVector3D obj)
    {
        throw new NotImplementedException();
    }
}
