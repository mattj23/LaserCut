using System.Collections;
using System.Diagnostics;
using LaserCut.Geometry;

namespace LaserCut.Algorithms;

public static class BodyMergeOps
{
    private static Body? ReduceMergeOne(ref List<Body> remaining)
    {
        Debug.WriteLine($"Reducing bodies, {remaining.Count} remaining");
        int merged = 0;

        // These are the bodies to be merged. We sort them and then transfer them into a queue
        remaining.Sort((b0, b1) => b1.Area.CompareTo(b0.Area));
        var queue = new Queue<Body>(remaining);

        // We clear the list of bodies left to merge, we will fill it with the ones that are left.
        remaining.Clear();

        // These are the bodies that are finished
        var finished = new Queue<Body>();
        if (queue.Count == 0) return null;

        var working = queue.Dequeue();
        while (queue.TryDequeue(out var body))
        {
            var a0 = working.Area;
            var (relation, _) = working.Outer.ShapeRelationTo(body.Outer);
            Debug.WriteLine($"Relation ({body.Area}): {relation}");
            switch (relation)
            {
                case ShapeRelation.DisjointTo:
                    finished.Enqueue(body);
                    break;
                case ShapeRelation.IsSubsetOf:
                    // Uno reverso
                    queue.Enqueue(working);
                    working = body;
                    merged++;
                    finished.TransferTo(queue);
                    break;
                case ShapeRelation.IsSupersetOf or ShapeRelation.Intersects:
                    try
                    {
                        // This operation can fail if the bodies are connected by a single point or other degenerate
                        // condition.  If we don't have a problem, we can do the operation and move on.
                        var temp = working.OperateAssertSingle(body.Outer);

                        foreach (var hole in body.Inners)
                        {
                            temp = temp.OperateAssertSingle(hole);
                        }

                        // We made it successfully, so we can move on to the next body
                        working = temp;
                        merged++;

                        // Now we need to empty the finished queue back to the main queue
                        finished.TransferTo(queue);
                    }
                    catch (InvalidOperationException)
                    {
                        // If we failed to merge, we need to keep the body and try again later.
                        Debug.WriteLine($"Failed to merge a={body.Area}, trying again later");

                        if (queue.Count == 0)
                        {
                            // If we have no more bodies to try, we need to keep this one.
                            remaining.Add(body);
                        }
                        else
                        {
                            // If we have more bodies to try, we need to try again later.
                            queue.Enqueue(body);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        remaining.AddRange(finished);
        Debug.WriteLine($" * Merged {merged} bodies, leaving {remaining.Count} remaining");
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
        while (ReduceMergeOne(ref working) is { } reduced)
        {
            finished.Add(reduced);
        }

        return finished.ToArray();
    }

}
