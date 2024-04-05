using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public class Line2D
{
    public Line2D(Point2D start, Vector2D direction)
    {
        Start = start;
        Direction = direction.Normalize();
        Normal = new Vector2D(-Direction.Y, Direction.X);
    }
    
    public Point2D Start { get; }
    public Vector2D Direction { get; }
    public Vector2D Normal { get; }
    
    public Point2D PointAt(double t)
    {
        return Start + Direction * t;
    }
    
    public double Determinant(Line2D other)
    {
        return Direction.X * other.Direction.Y - Direction.Y * other.Direction.X;
    }
    
    public bool Parallel(Line2D other)
    {
        return Math.Abs(Determinant(other)) < 1e-6;
    }
    
    public Vector2D IntersectionParams(Line2D other)
    {
        var det = Determinant(other);
        if (Math.Abs(det) < 1e-6)
        {
            return new Vector2D(double.NaN, double.NaN);
        }
        
        var dx = other.Start.X - Start.X;
        var dy = other.Start.Y - Start.Y;
        
        var t0 = (other.Direction.Y * dx - other.Direction.X * dy) / det;
        var t1 = (Direction.Y * dx - Direction.X * dy) / det;

        return new Vector2D(t0, t1);
    }
}