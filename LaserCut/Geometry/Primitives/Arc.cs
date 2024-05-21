using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace LaserCut.Geometry.Primitives;

public class Arc
{
    public Arc(Point2D center, double radius, double theta, double theta0)
    {
        Circle = new Circle2(center, radius);
        Theta = theta;
        Theta0 = Circle.ThetaOf(Circle.PointAt(theta0));
    }
    
    public Arc(Point2D start, Point2D end, Point2D center, bool clockwise)
    {
        if (Math.Abs(start.DistanceTo(center) - end.DistanceTo(center)) > GeometryConstants.DistEquals)
        {
            throw new ArgumentException("Start and end points must be equidistant from the center");
        }

        Circle = new Circle2(center, start.DistanceTo(center));
        Theta0 = Circle.ThetaOf(start);
        
        var v0 = start - center;
        var v1 = end - center;
        Theta = clockwise ? -v0.AngleToCw(v1) : v0.AngleToCcw(v1);
    }
    
    public double Theta { get; }
    public double Theta0 { get; }
    public Circle2 Circle { get; }
    
    public double Radius => Circle.Radius;
    
    public Point2D Center => Circle.Center;
    
    public Point2D PointAtFraction(double fraction)
    {
        var theta = Theta0 + Theta * fraction;
        return Circle.PointAt(theta);
    }
    
    public Vector2D DirectionAtFraction(double fraction)
    {
        var p = PointAtFraction(fraction);
        var v = (p - Center).Normalize();
        var rot = Angle.FromRadians(Theta > 0 ? Math.PI / 2 : -Math.PI / 2);
        return v.Rotate(rot);
    }

    public bool IsThetaOnArc(double theta)
    {
        // Theta0 is a value between -pi and pi, while theta is a value between -2pi and 2pi
        
        // First we convert theta to a value between -pi and pi
        var test = Circle.ThetaOf(Circle.PointAt(theta));
        
        // Now we look if the arc direction is clockwise or counterclockwise
        if (Theta >= 0)
        {
           // return  
        }

        throw new NotImplementedException();
    }
    
}