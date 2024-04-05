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
    
}