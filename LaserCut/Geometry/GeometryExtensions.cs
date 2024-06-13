using System.Collections.Generic;
using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public static class GeometryExtensions
{
    /// <summary>
    /// Converts a 3D point to a 2D point by dropping the Z coordinate.
    /// </summary>
    /// <param name="point">The 3D point to be converted</param>
    /// <returns>A 2D point equal to the 3D point without the Z coordinate</returns>
    public static Point2D ToPoint2D(this Point3D point)
    {
        return new Point2D(point.X, point.Y);
    }

    /// <summary>
    /// Calculates the shortest, signed angle which would rotate vector `v1` to align with vector `v2`.  The result will
    /// be positive if the rotation is counterclockwise, and negative if the rotation is clockwise.  The value returned
    /// will be between -π and π.
    /// </summary>
    /// <param name="v1">The reference vector</param>
    /// <param name="v2">The target vector</param>
    /// <returns>An angle between -π and π.</returns>
    public static double SignedAngle(this Vector2D v1, Vector2D v2)
    {
        return Math.Atan2(v1.X * v2.Y - v1.Y * v2.X, v1.X * v2.X + v1.Y * v2.Y);
    }
    
    /// <summary>
    /// Calculates the angle from `v1` to `v2` in the counterclockwise direction. This is the angle that `v1` would
    /// need to be rotated by, in the CCW direction, to align with `v2`.  The result will always be a positive number.
    /// </summary>
    /// <param name="v1">The reference vector</param>
    /// <param name="v2">The target vector</param>
    /// <returns>The CCW angle (in radians) which `v1` would need to rotate to align with `v2`</returns>
    public static double AngleToCcw(this Vector2D v1, Vector2D v2)
    {
        return v1.SignedAngleTo(v2).Radians;
    }
    
    /// <summary>
    /// Calculates the angle from `v1` to `v2` in the clockwise direction. This is the angle that `v1` would need to be
    /// rotated by, in the CW direction, to align with `v2`.  The result will always be a positive number, even though
    /// the convention for clockwise rotations is typically negative. To rotate using the result of this function,
    /// multiply by -1.
    /// </summary>
    /// <param name="v1">The reference vector</param>
    /// <param name="v2">The target vector</param>
    /// <returns>The CW angle (in radians) which `v1` would need to rotate to align with `v2`. Be aware this value
    /// will always be positive</returns>
    public static double AngleToCw(this Vector2D v1, Vector2D v2)
    {
        return v1.SignedAngleTo(v2, true).Radians;
    }
    
    public static Point2D[] ToPoint2Ds(this IEnumerable<Point3D> points, bool removeAdjacentDuplicates = false)
    {
        var points2 = new List<Point2D>();

        foreach (var point in points)
        {
            var p = new Point2D(point.X, point.Y);
            if (removeAdjacentDuplicates && points2.Count > 0 &&
                points2.Last().DistanceTo(p) <= GeometryConstants.DistEquals) continue;
            points2.Add(p);
        }

        return points2.ToArray();
    }

    /// <summary>
    /// Calculate the combined bounds of a collection of items that have bounds.
    /// </summary>
    /// <param name="items">A collection of items that have `Aabb2` bounds</param>
    /// <returns>A single `Aabb2` which encompasses all the bounds from the collection.</returns>
    public static Aabb2 CombinedBounds(this IEnumerable<IHasBounds> items)
    {
        var bounds = Aabb2.Empty;
        foreach (var item in items)
        {
            bounds = bounds.Union(item.Bounds);
        }

        return bounds;
    }
    
    public static Aabb2 CombinedBounds(this IEnumerable<Aabb2> items)
    {
        var bounds = Aabb2.Empty;
        foreach (var item in items)
        {
            bounds = bounds.Union(item);
        }

        return bounds;
    }
}