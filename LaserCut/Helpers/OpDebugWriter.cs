using LaserCut.Algorithms;
using LaserCut.Geometry;

namespace LaserCut.Helpers;

public class OpDebugWriter : ILoopOpHelper
{
    private readonly string _writePath;

    public OpDebugWriter(string writePath)
    {
        _writePath = writePath;
    }

    public void Data(BoundaryLoop l0, BoundaryLoop l1, ShapeRelation relation, IntersectionPair[] intersections)
    {
        var lines = new List<string>
        {
            $"Relation={relation}",
            $"Loop0={l0.Serialize()}",
            $"Loop1={l1.Serialize()}"
        };

        foreach (var pair in intersections)
        {
            var p = pair.First.Surface.Point;
            lines.Add(
                $"Pair={pair.First.Element.Index}@{pair.First.L:F6};{pair.Second.Element.Index}@{pair.Second.L:F6};{p.X:F6},{p.Y:F6}");
        }

        File.WriteAllLines(_writePath, lines);
    }
}