using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace LaserCut.Geometry.Primitives;

public struct SurfacePoint
{
    public SurfacePoint(Point2D point, Vector2D direction)
    {
        Point = point;
        Direction = direction.Normalize();
        // Rotated -90 degrees
        Normal = new Vector2D(Direction.Y, -Direction.X);
    }
    
    public Point2D Point { get; }
    public Vector2D Normal { get; }
    public Vector2D Direction { get; }

    public double DistanceTo(SurfacePoint other)
    {
        return DistanceTo(other.Point);
    }

    public double DistanceTo(Point2D other)
    {
        return Point.DistanceTo(other);
    }
}