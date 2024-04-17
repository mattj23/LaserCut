using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public static class GeometryExtensions
{
    public static Point2D ToPoint2D(this Point3D point)
    {
        return new Point2D(point.X, point.Y);
    }
    
    public static Point2D[] ToPoint2Ds(this IEnumerable<Point3D> points, bool removeAdjacentDuplicates = false)
    {
        var points2 = new List<Point2D>();
        
        foreach (var point in points)
        {
            var p = new Point2D(point.X, point.Y);
            if (removeAdjacentDuplicates && points2.Count > 0 && points2.Last().DistanceTo(p) <= GeometryConstants.DistEquals)
            {
                continue;
            }
            points2.Add(p);
        }
        
        return points2.ToArray();
    }

    public static Aabb2 CombinedBounds(this IEnumerable<IHasBounds> items)
    {
        var bounds = Aabb2.Empty;
        foreach (var item in items)
        {
            bounds = bounds.Union(item.Bounds);
        }

        return bounds;
    }
}