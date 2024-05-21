using System.Net.Mail;
using MathNet.Numerics;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

/// <summary>
/// This is a base class for a 2D parametric line in the form of a point and a direction vector.  As a line, it has no
/// start or end, so it exists for all values of t. The line can be used to calculate intersections with other lines,
/// or to model relations with other entities.
/// </summary>
public class Line2
{
    public Line2(Point2D start, Vector2D direction)
    {
        Start = start;
        Direction = direction.Normalize();
        Normal = new Vector2D(Direction.Y, -Direction.X);
    }
    
    /// <summary>
    /// Gets a point on the line that is considered the start point.  This is arbitrary, as the line extends infinitely
    /// in both directions, however the start point is associated with t=0.
    /// </summary>
    public Point2D Start { get; }
    
    /// <summary>
    /// Gets the direction of the line, which has been normalized to a unit vector.
    /// </summary>
    public Vector2D Direction { get; }
    
    /// <summary>
    /// Gets the normal direction of the line, which is a vector that is the direction rotated clockwise by 90
    /// degrees. This represents a surface normal for a line in which the counterclockwise direction is considered
    /// to face outward.
    /// </summary>
    public Vector2D Normal { get; }
    
    /// <summary>
    /// Calculate a point on the line at the given parameter t.  Because the direction vector has been normalized,
    /// t will correspond with the length along the line.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Point2D PointAt(double t)
    {
        return Start + Direction * t;
    }
    
    public double Determinant(Line2 other)
    {
        return Direction.X * other.Direction.Y - Direction.Y * other.Direction.X;
    }
    
    public bool IsCollinear(Line2 other)
    {
        return IsParallel(other) && DistanceTo(other.Start) < GeometryConstants.DistEquals;
    }

    /// <summary>
    /// Creates a new line that is offset from this line by the given distance.  The offset is in the direction of the
    /// line normal, and can be positive or negative.  The direction of the line will not change.
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Line2 Offset(double distance)
    {
        return new Line2(Start + Normal * distance, Direction);
    }

    /// <summary>
    /// Returns the parameter t at which the projection of the point p onto the line is closest to the start point. This
    /// can also be thought of as the distance along the line from the start point to the projection of p.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public double ProjectionParam(Point2D p)
    {
        var n = p - Start;
        return Direction.DotProduct(n);
    }
    
    /// <summary>
    /// Returns the point on the line that is closest to the given point p, also known as the projection of p onto the
    /// line.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Point2D Projection(Point2D p)
    {
        return Start + Direction * ProjectionParam(p);
    }
    
    /// <summary>
    /// Returns the distance from the closest point on the line to the given point p.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public double DistanceTo(Point2D p)
    {
        return (p - Projection(p)).Length;
    }
    
    /// <summary>
    /// Returns the signed distance from the line to the point p.  The sign indicates which side of the line the point
    /// on, with the normal vector pointing to the positive side.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public double SignedDistanceTo(Point2D p)
    {
        var v = p - Projection(p);
        return v.DotProduct(Normal);
    }
    
    /// <summary>
    /// Returns true if the two lines are parallel within the tolerance of the numeric zero (by default 1e-6).
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsParallel(Line2 other)
    {
        return Math.Abs(Determinant(other)) < GeometryConstants.NumericZero;
    }
    
    /// <summary>
    /// Returns the intersection parameters for this line and the other line.  The parameters are the values of t for
    /// the intersection point.  They will be NaN if the lines are parallel.
    ///
    /// The first value in the tuple represents the parameter for this line, and the second value represents the
    /// parameter for the other line.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public (double, double) IntersectionParams(Line2 other)
    {
        var det = Determinant(other);
        if (Math.Abs(det) < GeometryConstants.NumericZero)
        {
            return (double.NaN, double.NaN);
        }
        
        var dx = other.Start.X - Start.X;
        var dy = other.Start.Y - Start.Y;
        
        var t0 = (other.Direction.Y * dx - other.Direction.X * dy) / det;
        var t1 = (Direction.Y * dx - Direction.X * dy) / det;

        return (t0, t1);
    }

    protected (double, double) SlabAabbBase(Aabb2 box)
    {
        double xInv = 1.0 / Direction.X;
        double yInv = 1.0 / Direction.Y;
        double tX0 = xInv * (box.MinX - Start.X);
        double tX1 = xInv * (box.MaxX - Start.X);
        double tMin = Math.Min(tX0, tX1);
        double tMax = Math.Max(tX0, tX1);
        
        double tY0 = yInv * (box.MinY - Start.Y);
        double tY1 = yInv * (box.MaxY - Start.Y);
        tMin = Math.Max(tMin, Math.Min(tY0, tY1));
        tMax = Math.Min(tMax, Math.Max(tY0, tY1));
        
        return (tMin, tMax);
    }
}