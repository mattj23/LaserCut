﻿using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

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

    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
    public Point2D Center => new Point2D((MinX + MaxX) / 2, (MinY + MaxY) / 2);

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
        return new Aabb2(
            Math.Min(MinX, other.MinX),
            Math.Min(MinY, other.MinY),
            Math.Max(MaxX, other.MaxX),
            Math.Max(MaxY, other.MaxY)
        );
    }
}