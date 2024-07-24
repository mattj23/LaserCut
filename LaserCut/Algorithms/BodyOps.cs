using System.Diagnostics;
using LaserCut.Geometry;
using LaserCut.Helpers;

namespace LaserCut.Algorithms;

public static class BodyOps
{
    public static Body[] Operate(this Body body, BoundaryLoop tool)
    {
        return tool.IsPositive ? body.OperatePositive(tool) : body.OperateNegative(tool);
    }

    private static Body[] OperatePositive(this Body body, BoundaryLoop tool)
    {
        if (!tool.IsPositive)
            throw new InvalidOperationException("Cannot perform a positive operation with a negative tool");

        /*
         * In the case of a positive tool (union operation):
         * 1. Consider the operation with the outer loop:
         *  - If the two are completely disjoint, we create a second body
         *  - If the outer loop is subsumed, the resulting body is the tool with no internal boundaries
         *  - The outer loop cannot be destroyed because the outer loop is positive and so is the tool
         *  - If the outer loop is merged with the tool, we replace the outer loop with the result
         *
         * 2. At this point we know that the tool protrudes into the body to some extent, and the outer loop may
         * have been modified but in doing so it will have grown and so cannot intersect with any of the existing
         * inner loops. The only thing that can happen at this point is that inner boundaries can be shrunk or
         * destroyed by the tool.  None of them will merge with each other as the result of a union, though they
         * can be split into multiple loops.
         * 3. We iterate through all the inner loops and merge them with the tool. They can be one of the following
         * results:
         *  - Disjoint: we leave them alone
         *  - Destroyed: we remove them from the inner loop
         *  - Merged: we replace them with the result of the merge
         *  - Shape encloses tool: this is the odd case, as it would theoretically result in the creation of a new
         *    body which does not intersect with this one.  In this case we will ignore it, considering it to be
         *    something which performs no modification to the body (technically at this point we should be able to
         *    simply return the original body unmodified)
         */

        // var (outerResult, outerLoops) = ShapeOperation.Operate(Outer, tool);
        var outer = body.Outer.Copy();
        var (outerResult, loops) = outer.Union(tool);

        if (outerResult == BoundaryOpResult.Destroyed)
            throw new UnreachableException();

        // We only got here if the tool had some portion of it which was inside the body (Merged or ShapeEnclosesTool).
        // In either case, we expect that there is one contour in the outerLoops array.  We know that there will only
        // be one body result because the tool is positive and the outer loop is positive, so the result will always be
        // a single body.  However, it is possible for the merge result to have produced one positive and one or more
        // negative contours, such as when a concave shape is capped off.  In this case, the negative contours will
        // be added to the inner boundaries of the new body.
        var positive = loops.Where(x => x.IsPositive).ToList();
        if (positive.Count != 1)
            throw new InvalidOperationException($"Expected a single positive loop, got {positive.Count}");
        var workingBody = new Body(positive[0]);

        var workingInners = new Queue<BoundaryLoop>(body.Inners.Select(x => x.Copy()));
        foreach (var outerLoop in loops)
            if (!outerLoop.IsPositive)
                workingInners.Enqueue(outerLoop);

        // Now we can re-iterate through the inner loops, merge them with the tool, and insert them into the new 
        // body.
        while (workingInners.TryDequeue(out var loop))
        {
            var (result, mergedLoops) = loop.Union(tool);
            switch (result)
            {
                case BoundaryOpResult.Merged:
                    workingBody.Inners.AddRange(mergedLoops.Where(x => !x.IsPositive));
                    break;
                case BoundaryOpResult.Destroyed:
                    // In the case of a union operation, this can only mean that the resulting shape fills the entire
                    // 2D plane, which in our case means the hole has been destroyed.
                    break;
                case BoundaryOpResult.Unchanged:
                    // The tool is a subset of the area the hole allows, so nothing changes
                    workingBody.Inners.Add(loop);
                    break;
                case BoundaryOpResult.Replaced:
                    // This would require that the hole is a subset of the tool, which is not possible for a negative
                    // hole and a positive tool
                    throw new UnreachableException();
                case BoundaryOpResult.UnchangedMerged:
                    // This would require that the hole and the tool are completely disjoint, which can only happen if
                    // the tool is enclosed by the hole.  In this case, the hole is unchanged and we can add it back to
                    // the list of holes.
                    workingBody.Inners.Add(loop);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        return [workingBody];
    }

    private static Body[] OperateNegative(this Body body, BoundaryLoop tool)
    {
        if (tool.IsPositive)
            throw new InvalidOperationException("Cannot perform a negative operation with a positive tool");

        var outer = body.Outer.Copy();
        var (outerResult, resultLoops) = outer.Intersection(tool);
        
        var innerLoops = body.Inners.Select(x => x.Copy()).ToList();
        innerLoops.AddRange(resultLoops.Where(x => !x.IsPositive));

        var working = resultLoops
            .Where(x => x.IsPositive)
            .Select(x => new BodyBoundarySet(x, innerLoops))
            .ToArray();

        return working.Select(x => x.Resolve()).ToArray();
    }

    private class BodyBoundarySet
    {
        public BodyBoundarySet(BoundaryLoop outer, IEnumerable<BoundaryLoop> holes)
        {
            Outer = outer;
            Holes = holes.ToArray();
        }

        public BoundaryLoop Outer { get; set; }
        public BoundaryLoop[] Holes { get; }

        public Body Resolve()
        {
            // The boundary set is composed of a potentially new outer boundary and a set of inner boundaries which
            // may intersect with each other and may or may not intersect with the outer boundary.  To resolve them,
            // we need to perform all possible merges.
            
            // We start by merging everything against the outer boundary.  We know that none of these intersections 
            // will destroy the outer boundary, but they may split it.
            var workingOuter = Outer.Copy();
            var workingHoles = new Queue<BoundaryLoop>(Holes);
            var verified = new Queue<BoundaryLoop>();
            
            while (workingHoles.TryDequeue(out var hole))
            {
                var (result, loops) = workingOuter.Intersection(hole);

                switch (result)
                {
                    case BoundaryOpResult.Destroyed:
                        // For this to happen, the positive outer boundary would need to be completely encircled by
                        // a negative inner boundary, which shouldn't have been possible
                        throw new UnreachableException();
                    case BoundaryOpResult.Unchanged:
                        // This is what happens if the hole is outside the outer boundary.  The outer boundary will not
                        // get bigger during this process, so this hole will never intersect with it or anything 
                        // inside it.  We effectively drop it here.
                        break;
                    case BoundaryOpResult.Replaced:
                        // This should never happen, as it would require that the outer boundary is a superset
                        // of the hole, which is not possible with a hole having a negative area
                        throw new UnreachableException();
                    case BoundaryOpResult.Merged:
                        // This means we had boundary intersections and the outer boundary was modified.  We need to
                        // update the working outer boundary and add the new holes to the working queue, but we don't
                        // preserve the hole we were working on.
                        var positives = loops.Where(x => x.IsPositive).ToArray();
                        if (positives.Length != 1) 
                            throw new ArgumentException($"Expected a single positive loop, got {positives.Length}");
                        workingHoles.EnqueueAll(loops.Where(x => !x.IsPositive));
                        workingOuter = positives[0];
                        
                        // We also need to transfer all the verified holes back to the working queue, because the
                        // outer boundary has changed and we need to re-verify them.
                        verified.TransferTo(workingHoles);
                        break;
                    case BoundaryOpResult.UnchangedMerged:
                        // This occurs when the outer boundary and the hole have a shape intersection, but their
                        // boundary paths don't touch each other (basically the hole is inside the border and doesn't
                        // touch it).  This is a regular hole, and we can add it to the verified queue.
                        verified.Enqueue(hole);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // Now we have a final outer boundary, so we transfer everything from the verified queue to the working
            // queue.  All the holes which are outside the outer boundary have been dropped.
            verified.TransferTo(workingHoles);
            var finalHoles = new List<BoundaryLoop>();
            while (workingHoles.TryDequeue(out var hole))
            {
                bool discard = false;
                for (var i = 0; i < finalHoles.Count; i++)
                {
                    bool breakLoop = false;
                    var (result, loops) = hole.Intersection(finalHoles[i]);

                    switch (result)
                    {
                        case BoundaryOpResult.Destroyed:
                            // It shouldn't be possible to have two negative loops be completely disjoint
                            throw new UnreachableException();
                        case BoundaryOpResult.Unchanged:
                            // This is what happens when "hole" completely encloses the final hole. We can discard
                            // the final hole, add "hole" back to the queue, and start again from the beginning.
                            finalHoles.RemoveAt(i);
                            workingHoles.Enqueue(hole);
                            finalHoles.TransferTo(workingHoles);
                            breakLoop = true;
                            break;
                        case BoundaryOpResult.Replaced:
                            // This will happen if "hole" is a superset of the final hole, which will occur if "hole"
                            // is inside and smaller than the final hole.  We can discard "hole" and move on to the
                            // next item in the queue.
                            breakLoop = true;
                            discard = true;
                            break;
                        case BoundaryOpResult.Merged:
                            // This means we had boundary intersections and the final hole was modified. We need to
                            // remove both the final hole and "hole" and add the contents of the merge to the working
                            // queue.  We also need to start over from the beginning.
                            var positives = loops.Where(x => x.IsPositive).ToArray();
                            if (positives.Length != 0)
                                throw new ArgumentException($"Got a positive region during a hole merge");
                            workingHoles.EnqueueAll(loops.Where(x => !x.IsPositive));
                            finalHoles.RemoveAt(i);
                            finalHoles.TransferTo(workingHoles);
                            discard = true;
                            breakLoop = true;
                            break;
                        case BoundaryOpResult.UnchangedMerged:
                            // This occurs when the two holes are disjoint.  
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    if (breakLoop) break;
                }

                if (!discard)
                {
                    finalHoles.Add(hole);
                }
            }


            return new Body(workingOuter, finalHoles);
        }
    }
    
    // public static BodyContourSet[] ResolveInsidesToOutsides(IReadOnlyList<BoundaryLoop> outsides,
    //     IReadOnlyList<BoundaryLoop> insides)
    // {
    //     var validated = new List<BodyContourSet>();
    //
    //     var unvalidated = new Queue<BodyContourSet>();
    //     foreach (var outside in outsides) unvalidated.Enqueue(new BodyContourSet(outside, insides));
    //
    //     while (unvalidated.Count != 0)
    //     {
    //         var current = unvalidated.Dequeue();
    //         var uncheckedHoles = new Queue<BoundaryLoop>();
    //         current.Holes.TransferTo(uncheckedHoles);
    //
    //         var hadMerge = false;
    //         while (uncheckedHoles.TryDequeue(out var testHole))
    //         {
    //             // Try a merge operation on the outer loop. If it succeeds, replace the outer loop with the result
    //             // and transfer everything from working back into temp, otherwise (if it fails) add it back to the
    //             // working queue
    //             var (result, loops) = current.Outer.Mutate(testHole);
    //
    //             // The above operation is always mutating a positive loop (the outer boundary) with a negative
    //             // loop (the inner boundary) which did not merge with the previous outer boundary.  It should
    //             // not be possible to get a result which includes any negative loops, but it is possible that
    //             // we will change the number of outer boundaries.  For example, a hollow rectangle shape may be
    //             // split into two separate bodies by a U shaped tool that shortens both ends.
    //
    //             switch (result)
    //             {
    //                 case MutateResult.Disjoint:
    //                     // The two loops are completely disjoint, which means the inner boundary is not inside this
    //                     // outer boundary.  We don't need it for this body anymore, though it may be important to
    //                     // a different body.
    //                     break;
    //                 case MutateResult.Destroyed:
    //                     // The modified outer boundary is completely destroyed by the inner boundary. I'm not sure
    //                     // yet if this is theoretically possible.
    //                     throw new NotImplementedException("Not sure what to do here");
    //                 case MutateResult.Subsumed:
    //                     // This should not be possible, as the outer boundary is always positive and the inner
    //                     // boundary is always negative
    //                     throw new UnreachableException(
    //                         "Subsumed outer should not be possible with a negative body and tool");
    //                 case MutateResult.ShapeEnclosesTool:
    //                     // The outer boundary encloses the inner boundary as we would expect, so we add it back to
    //                     // the working queue
    //                     if (loops.Length != 1)
    //                         throw new InvalidOperationException($"Expected a single loop, got {loops.Length}");
    //                     current.Holes.Enqueue(testHole);
    //                     break;
    //                 case MutateResult.Merged:
    //                     // We have now modified the outer boundary, so we need to update the working positive loops
    //                     // and start over.  We'll add the unchecked holes back to the queue (leaving out this one)
    //                     // and then make a copy of the current BodyContourSet with the same holes but each new outer
    //                     // loop and add them back to the unvalidated queue.
    //                     uncheckedHoles.TransferTo(current.Holes);
    //
    //                     foreach (var newLoop in loops) unvalidated.Enqueue(new BodyContourSet(newLoop, current.Holes));
    //
    //                     hadMerge = true;
    //                     break;
    //
    //                 default:
    //                     throw new ArgumentOutOfRangeException($"Unexpected result type: {result}");
    //             }
    //         }
    //         // Under normal circumstances, getting to this point means we've worked through all the unchecked holes
    //         // and the current body set is valid.  However, if we had a merge, we discard the current body set and
    //         // allow the loop to continue.
    //         if (!hadMerge) validated.Add(current);
    //     }
    //
    //     return validated.ToArray();
    // }
    //

}