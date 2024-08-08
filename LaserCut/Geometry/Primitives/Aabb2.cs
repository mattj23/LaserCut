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

    public static Aabb2 FromPoints(Point2D p0, Point2D p1)
    {
        return new Aabb2(Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y), Math.Max(p0.X, p1.X), Math.Max(p0.Y, p1.Y));
    }
    
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
    
    /// <summary>
    /// Creates a new bounding box that is a copy of this one.
    /// </summary>
    /// <returns></returns>
    public Aabb2 Clone()
    {
        return new Aabb2(MinX, MinY, MaxX, MaxY);
    }

    /// <summary>
    /// Return a new bounding box that is expanded to contain the given point.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public void ExpandToContain(Point2D p)
    {
        ExpandToContain(p.X, p.Y);
    }

    /// <summary>
    /// Return a new bounding box that is expanded to contain the given point.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public void ExpandToContain(double x, double y)
    {
        if (double.IsNaN(MinX) || x < MinX)
            MinX = x;
        
        if (double.IsNaN(MinY) || y < MinY)
            MinY = y;
        
        if (double.IsNaN(MaxX) || x > MaxX)
            MaxX = x;
        
        if (double.IsNaN(MaxY) || y > MaxY)
            MaxY = y;
    }
    
    /// <summary>
    /// Expand the bounding box by the given amount.
    /// </summary>
    /// <param name="amount"></param>
    public void Expand(double amount)
    {
        MinX -= amount;
        MinY -= amount;
        MaxX += amount;
        MaxY += amount;
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
    
    /// <summary>
    /// Finds the closest distance between this bounding box and a point.
    /// </summary>
    /// <param name="point">The point to test the distance from.</param>
    /// <returns>A finite double value between greater or equal to zero</returns>
    public double ClosestDistance(Point2D point)
    {
        // The closest point on the bounding box is either the point itself if it is inside the box, or the test point
        // clamped to the box bounds.
        if (Contains(point))
            return 0;
        
        var dx = point.X - point.X.Clamp(MinX, MaxX);
        var dy = point.Y - point.Y.Clamp(MinY, MaxY);
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    public double ClosestDistance(Aabb2 other)
    {
        if (Intersects(other))
            return 0;
        
        var dx = Math.Max(0, Math.Max(MinX - other.MaxX, other.MinX - MaxX));
        var dy = Math.Max(0, Math.Max(MinY - other.MaxY, other.MinY - MaxY));
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    /// <summary>
    /// Finds the farthest distance between this bounding box and a point.
    /// </summary>
    /// <param name="point">The point to test the distance from.</param>
    /// <returns>A double value which will be at least zero</returns>
    public double FarthestDistance(Point2D point)
    {
        // The farthest point on the box will be one of the corners.
        var dx = Math.Max(Math.Abs(MinX - point.X), Math.Abs(MaxX - point.X));
        var dy = Math.Max(Math.Abs(MinY - point.Y), Math.Abs(MaxY - point.Y));

        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double FarthestDistance(Aabb2 other)
    {
        var dx = Math.Max(MaxX - other.MinX, other.MaxX - MinX);
        var dy = Math.Max(MaxY - other.MinY, other.MaxY - MinY);
        
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    public Point2D[] Corners()
    {
        return
        [
            new Point2D(MinX, MinY),
            new Point2D(MinX, MaxY),
            new Point2D(MaxX, MinY),
            new Point2D(MaxX, MaxY)
        ];
    }
    
    public Aabb2 Translate(double x, double y) => new Aabb2(MinX + x, MinY + y, MaxX + x, MaxY + y);
    
}