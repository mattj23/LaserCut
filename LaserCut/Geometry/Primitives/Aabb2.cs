using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

/// <summary>
/// An axis-aligned bounding box in 2D space.
/// </summary>
public struct Aabb2
{
    public double MinX;
    public double MinY;
    public double MaxX;
    public double MaxY;

    public Aabb2(double minX, double minY, double maxX, double maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }
    
    public static Aabb2 Empty => new Aabb2(double.NaN, double.NaN, double.NaN, double.NaN);
    
    public double Area => Width * Height;

    public bool IsEmpty => double.IsNaN(MinX) || double.IsNaN(MinY) || double.IsNaN(MaxX) || double.IsNaN(MaxY);

    public double Width => MaxX - MinX;
    
    public double Height => MaxY - MinY;
    
    public Point2D Center => new Point2D((MinX + MaxX) / 2, (MinY + MaxY) / 2);
    
    public static Aabb2 FromPoints(IEnumerable<Point2D> points)
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        foreach (var point in points)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        return new Aabb2(minX, minY, maxX, maxY);
    }

    public bool Contains(Point2D point)
    {
        return point.X >= MinX && point.X <= MaxX && point.Y >= MinY && point.Y <= MaxY;
    }

    public bool Contains(Aabb2 other)
    {
        return other.MinX >= MinX && other.MaxX <= MaxX && other.MinY >= MinY && other.MaxY <= MaxY;
    }

    public bool Intersects(Aabb2 other)
    {
        return MinX <= other.MaxX && MaxX >= other.MinX && MinY <= other.MaxY && MaxY >= other.MinY;
    }

    public Aabb2 Union(Aabb2 other)
    {
        if (IsEmpty)
            return other;
        if (other.IsEmpty)
            return this;
        
        return new Aabb2(
            Math.Min(MinX, other.MinX),
            Math.Min(MinY, other.MinY),
            Math.Max(MaxX, other.MaxX),
            Math.Max(MaxY, other.MaxY)
        );
    }
}