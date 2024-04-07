using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public static class GeometryExtensions
{
    public static Contour ToContour(this IEnumerable<Point2D> points)
    {
        return new Contour(points);
    }
    
    public static Contour ToContour(this IEnumerable<Point3D> points)
    {
        return new Contour(points.Select(p => new Point2D(p.X, p.Y)));
    }
    
    public static Contour ToContour(this PolyLine3D poly)
    {
        return new Contour(poly.Vertices.Select(p => new Point2D(p.X, p.Y)));
    }

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
}