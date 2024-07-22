using LaserCut.Algorithms;
using LaserCut.Geometry;

namespace LaserCut.Helpers;

public interface ILoopOpHelper
{
    void Data(BoundaryLoop l0, BoundaryLoop l1, ShapeRelation relation, IntersectionPair[] intersections);
    
}