using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public struct ContourIntersection
{
    public Point2D Point;
    public int Index0;
    public int Index1;
    
    public ContourIntersection(Point2D point, int index0, int index1)
    {
        Point = point;
        Index0 = index0;
        Index1 = index1;
    }
}