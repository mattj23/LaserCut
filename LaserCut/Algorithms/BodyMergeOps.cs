using System.Collections;
using LaserCut.Geometry;

namespace LaserCut.Algorithms;

public static class BodyMergeOps
{
    private static Body? ReduceMergeOne(List<Body> remaining)
    {
        var queue = new Queue<Body>(remaining);
        remaining.Clear();
        var finished = new Queue<Body>();
        if (queue.Count == 0) return null;
        
        var working = queue.Dequeue();
        var c = 0;
        while (queue.TryDequeue(out var body))
        {
            if (c++ == 5)
            {
                Console.WriteLine("Problem");
            }
            
            var a0 = working.Area;
            var (relation, _) = working.Outer.ShapeRelationTo(body.Outer);
            switch (relation)
            {
                case ShapeRelation.DisjointTo:
                    finished.Enqueue(body);
                    break;
                case ShapeRelation.IsSubsetOf:
                    throw new NotImplementedException();
                case ShapeRelation.IsSupersetOf or ShapeRelation.Intersects:
                    working = working.OperateAssertSingle(body.Outer);
                    foreach (var hole in body.Inners)
                    {
                        working = working.OperateAssertSingle(hole);
                    }
                    
                    Console.WriteLine($"{a0:F2} -> {working.Area:F2} ({body})");
                    
                    // Now we need to empty the finished queue
                    finished.TransferTo(queue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        remaining.AddRange(finished);
        return working;
    }

    private static Body OperateAssertSingle(this Body body, BoundaryLoop tool)
    {
        var result = body.Operate(tool);
        if (result.Length != 1) throw new InvalidOperationException("Expected a single result");
        return result[0];
    }
    
    public static Body[] MergeBodies(this IEnumerable<Body> bodies)
    {
        var finished = new List<Body>();
        var working = bodies.ToList();
        while (ReduceMergeOne(working) is { } reduced)
        {
            finished.Add(reduced);
        }

        return finished.ToArray();
    }
    
}