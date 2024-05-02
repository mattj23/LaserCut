using LaserCut.Algorithms.Loop;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public interface IPointLoopCursor : ILoopCursor<Point2D>
{
    int InsertAbs(Point2D p);
    
    int InsertAbs(double x, double y);
    
    int InsertRel(Vector2D v);
    
    int InsertRel(double x, double y);
    
    int InsertRelX(double x);
    
    int InsertRelY(double y);

    int InsertRadius(Point2D start, Point2D end, Point2D center, int segments);
}