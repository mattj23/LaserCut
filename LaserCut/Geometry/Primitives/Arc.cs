using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace LaserCut.Geometry.Primitives;

public class Arc : IContourElement
{
    public Arc(Circle2 circle, double theta0, double theta)
    {
        Circle = circle;
        Theta = Angles.AsOneRotSigned(theta);
        Theta0 = Angles.AsSignedAbs(theta0);

        Start = Circle.PointAt(Theta0);
        End = Circle.PointAt(Theta0 + Theta);
        Length = Math.Abs(Theta) * Circle.Radius;
        
        UpdateBounds();
    }

    public Arc(Point2D center, double radius, double theta, double theta0) 
        : this(new Circle2(center, radius), theta0, theta)
    {
    }
    
    public Arc(double cx, double cy, double radius, double theta, double theta0) 
        : this(new Point2D(cx, cy), radius, theta, theta0)
    {
    }

    public static Arc FromEnds(Point2D start, Point2D end, Point2D center, bool clockwise)
    {
        if (Math.Abs(start.DistanceTo(center) - end.DistanceTo(center)) > GeometryConstants.DistEquals)
            throw new ArgumentException("Start and end points must be equidistant from the center");

        var c = new Circle2(center, start.DistanceTo(center));
        var t0 = c.ThetaOf(start);

        var v0 = start - center;
        var v1 = end - center;
        var t = clockwise ? -v0.AngleToCw(v1) : v0.AngleToCcw(v1);
        return new Arc(c, t0, t);
    }

    public Point2D Start { get; }

    public Point2D End { get; }

    public Aabb2 Bounds { get; private set; }

    public double Length { get; }

    /// <summary>
    ///     Gets the sweep angle of the arc in radians.  Positive values are counterclockwise, negative values are
    ///     clockwise.  This value will be between -2π and 2π.
    /// </summary>
    public double Theta { get; }

    /// <summary>
    ///     Gets the starting angle of the arc in radians.  This is the angle of the start point relative to the center of
    ///     the underlying circle, with 0 being the positive x direction.  This value will be between -π and π.
    /// </summary>
    public double Theta0 { get; }

    public Circle2 Circle { get; }

    public double Radius => Circle.Radius;

    public Point2D Center => Circle.Center;

    public bool IsCcW => Theta >= 0;

    public double FractionToLength(double fraction)
    {
        return fraction * Length;
    }
    
    public double LengthToFraction(double length)
    {
        return length / Length;
    }

    public double FractionToTheta(double fraction)
    {
        return Theta0 + Theta * fraction;
    }

    /// <summary>
    /// Given an angle theta (defined as a position from the positive x axis), return the fraction of the arc that is
    /// traversed to reach that angle.
    /// </summary>
    /// <param name="theta"></param>
    /// <returns></returns>
    public double ThetaToFraction(double theta)
    {
        theta = Angles.AsSignedAbs(theta);
        if (IsCcW)
        {
            return Angles.BetweenCcw(Theta0, theta) / Theta;
        }
        return Angles.BetweenCw(Theta0, theta) / -Theta;
    }
    
    public SurfacePoint AtFraction(double fraction)
    {
        // TODO: Should this clip? or error if out of bounds?
        var theta = Theta0 + Theta * fraction;

        // The point on the arc
        var p = Circle.PointAt(theta);
        
        // The radial vector direction from the center to the point
        var v = (p - Center).Normalize();
        
        var rot = Angle.FromRadians(Theta > 0 ? Math.PI / 2 : -Math.PI / 2);
        return new SurfacePoint(p, v.Rotate(rot));
    }

    public SurfacePoint AtLength(double length)
    {
        // TODO: Should this clip? or error if out of bounds?
        var f = length / Length;
        return AtFraction(f);
    }

    public bool IsThetaOnArc(double test)
    {
        test = Angles.AsSignedAbs(test);
        var theta1 = Angles.AsSignedAbs(Theta0 + Theta);

        // Check the endpoints, this may need to adapt to the arc length associated with the geometric distance equals
        // constant in the future
        if (Math.Abs(test - Theta0) < 1e-10) return true;
        if (Math.Abs(test - theta1) < 1e-10) return true;

        // Check if the arc is counterclockwise or clockwise
        if (IsCcW)
            // Counterclockwise
            return Angles.BetweenCcw(Theta0, test) <= Theta;

        // Clockwise
        return Angles.BetweenCw(Theta0, test) <= -Theta;
    }

    public Position Closest(Point2D point)
    {
        var theta = Circle.ThetaOf(point);
        if (IsThetaOnArc(theta))
        {
            return new Position(FractionToLength(ThetaToFraction(theta)), this);
        }
        
        return point.DistanceTo(Start) < point.DistanceTo(End) ? new Position(0, this) : new Position(Length, this);
    }

    /// <summary>
    /// Computes the intersections of the arc with the given line.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public Position[] Intersections(Line2 line)
    {
        return ValidPositionsFromPoints(Circle.Intersections(line));
    }
    
    /// <summary>
    /// Computes the intersections of the arc with the given circle.
    /// </summary>
    /// <param name="circle"></param>
    /// <returns></returns>
    public Position[] Intersections(Circle2 circle)
    {
        return ValidPositionsFromPoints(Circle.Intersections(circle));
    }
    
    public ElementIntersection[] MatchIntersections(IEnumerable<Position> positions)
    {
        var results = new List<ElementIntersection>();
        foreach (var other in positions)
        {
            var theta = Circle.ThetaOf(other.Surface.Point);
            if (IsThetaOnArc(theta))
            {
                var position = new Position(FractionToLength(ThetaToFraction(theta)), this);
                results.Add(new ElementIntersection(position, other));
            }
        }

        return results.ToArray();
    }

    private Position[] ValidPositionsFromPoints(IEnumerable<Point2D> points)
    {
        var results = new List<Position>();
        foreach (var p in points)
        {
            var theta = Circle.ThetaOf(p);
            if (IsThetaOnArc(theta))
            {
                results.Add(new Position(FractionToLength(ThetaToFraction(theta)), this));
            }
        }

        return results.ToArray();
    }
    
    private void UpdateBounds()
    {
        var bounds = Aabb2.Empty;
        bounds.ExpandToContain(Start);
        bounds.ExpandToContain(End);
        for (var i = 0; i < 4; i++)
        {
            var angle = Math.PI / 2.0 * i;
            if (IsThetaOnArc(angle)) bounds.ExpandToContain(Circle.PointAt(angle));
        }

        Bounds = bounds;
    }
}