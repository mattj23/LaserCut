using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace LaserCut.Geometry.Primitives;

public class Circle2
{
    public Circle2(Point2D center, double radius)
    {
        Center = center;
        Radius = radius;
    }
    
    public Circle2(double x, double y, double radius) : this(new Point2D(x, y), radius)
    {
    }

    public Circle2(Point2D p0, Point2D p1, Point2D p2)
    {
        var temp = p1.X * p1.X + p1.Y * p1.Y;
        var bc = (p0.X * p0.X + p0.Y * p0.Y - temp) / 2.0;
        var cd = (temp - p2.X * p2.X - p2.Y * p2.Y) / 2.0;
        var det = (p0.X - p1.X) * (p1.Y - p2.Y) - (p1.X - p2.X) * (p0.Y - p1.Y);
        
        if (System.Math.Abs(det) < 1.0e-6)
        {
            throw new ArgumentException("Points are collinear");
        }
        
        var cx = (bc * (p1.Y - p2.Y) - cd * (p0.Y - p1.Y)) / det;
        var cy = ((p0.X - p1.X) * cd - (p1.X - p2.X) * bc) / det;
        Center = new Point2D(cx, cy);
        Radius = Center.DistanceTo(p0);
    }
    
    public Point2D Center { get; }
    
    public double Radius { get; }

    public bool IsPointInside(Point2D p)
    {
        return Center.DistanceTo(p) <= Radius;
    }

    /// <summary>
    /// Given a point, return the angle in radians of the point relative to the center of the circle, where the
    /// zero-angle line is the positive x direction.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public double ThetaOf(Point2D p)
    {
        var a = p - Center;
        return Math.Atan2(a.Y, a.X);
    }


    public Point2D Project(Point2D p)
    {
        return PointAt(ThetaOf(p));
    }

    public Point2D PointAt(double theta)
    {
        var v = new Vector2D(Radius, 0).Rotate(Angle.FromRadians(theta));
        return Center + v;
    }

    public Point2D[] Intersections(Line2 line)
    {
        var linePoint = line.Projection(Center);
        var offset = linePoint.DistanceTo(Center);
        if (Math.Abs(offset - Radius) < GeometryConstants.DistEquals)
        {
            return [linePoint];
        }
        if (offset > Radius)
        {
            return [];
        }
        
        // The line intersects the circle at two points. The distance from the line point to the intersection points
        // is the height of a right triangle with hypotenuse `Radius` and base `offset`. The height is `height`.
        var height = Math.Sqrt(Radius * Radius - offset * offset);
        return [linePoint + line.Direction * height, linePoint - line.Direction * height];
    }

    public Point2D[] Intersections(Circle2 other)
    {
       var separation = other.Center - Center;
       var gap = separation.Length;
       
       if (gap > Radius + other.Radius || gap < Math.Abs(Radius - other.Radius))
       {
           return [];
       }
       
       var a = (Radius * Radius - other.Radius * other.Radius + gap * gap) / (2 * gap);
       var separationUnit = separation.Normalize();
       var cp = Center + a * separationUnit;
       var d = separationUnit.Rotate(Angle.FromRadians(Math.PI / 2.0));
       return Intersections(new Line2(cp, d));
    }

    public double DistanceFromEdge(Point2D p)
    {
        var d = Center.DistanceTo(p);
        if (d < Radius)
        {
            return Radius - d;
        }
        return d - Radius;
    }
    
    public Point2D[] TangentsTo(Point2D p)
    {
        var d = Center.DistanceTo(p);
        if (d < Radius)
        {
            return [];
        }
        
        var angle = Math.Asin(Radius / d);
        var theta = Math.Atan2(p.Y - Center.Y, p.X - Center.X);
        return new[]
        {
            new Point2D(Center.X + Radius * Math.Cos(theta + angle), Center.Y + Radius * Math.Sin(theta + angle)),
            new Point2D(Center.X + Radius * Math.Cos(theta - angle), Center.Y + Radius * Math.Sin(theta - angle))
        };
    }
}