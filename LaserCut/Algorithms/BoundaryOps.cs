using System.Diagnostics;
using LaserCut.Geometry;
using LaserCut.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// This class consolidates the algorithms for working with merges of `BoundaryLoop` objects.
/// </summary>
public static class BoundaryOps
{
    public enum OpType
    {
        Union,
        Intersection
    }
    
    public static (BoundaryOpResult, BoundaryLoop[]) Intersection(this BoundaryLoop l0, BoundaryLoop l1, ILoopOpHelper? helper=null)
    {
        var (relation, intersections) = l0.ShapeRelationTo(l1);
        
        helper?.Data(l0, l1, relation, intersections);

        return relation switch
        {
            // The areas enclosed by the loops are completely disjoint, so the result is the empty set
            ShapeRelation.DisjointTo => (BoundaryOpResult.Destroyed, []),

            // Shape 0 is a subset of shape 1, so the result is shape 0
            ShapeRelation.IsSubsetOf => (BoundaryOpResult.Unchanged, [l0]),
            
            // Shape 1 is a subset of shape 0, so the result is shape 1
            ShapeRelation.IsSupersetOf => (BoundaryOpResult.Replaced, [l1]),
            
            // The two shapes have intersection, so we need to compute the result
            ShapeRelation.Intersects => (BoundaryOpResult.Merged,
                OperateFromPairs(l0, l1, intersections, OpType.Intersection)),
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
        };
    }
    
    public static (BoundaryOpResult, BoundaryLoop[]) Union(this BoundaryLoop l0, BoundaryLoop l1, ILoopOpHelper? helper=null)
    {
        var (relation, intersections) = l0.ShapeRelationTo(l1);
        helper?.Data(l0, l1, relation, intersections);

        return relation switch
        {
            // The areas enclosed by the loops are completely disjoint, so the result is both loops together
            ShapeRelation.DisjointTo => (BoundaryOpResult.Merged, [l0, l1]),

            // Shape 0 is a subset of shape 1, so the result is shape 1
            ShapeRelation.IsSubsetOf => (BoundaryOpResult.Replaced, [l1]),
            
            // Shape 1 is a subset of shape 0, so the result is shape 0
            ShapeRelation.IsSupersetOf => (BoundaryOpResult.Unchanged, [l0]),
            
            // The two shapes have intersection, so we need to compute the result
            ShapeRelation.Intersects => (BoundaryOpResult.Merged,
                OperateFromPairs(l0, l1, intersections, OpType.Union)),
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
        };
    }
    
    private static BoundaryLoop[] OperateFromPairs(BoundaryLoop l0, BoundaryLoop l1, IntersectionPair[] pairs, OpType opType)
    {
        // Filter out any pairs which are not valid for the operation type
        var workingPairs = opType switch {
            OpType.Union => pairs.Where(i => i.FirstExitsSecond || i.SecondExitsFirst).ToList(),
            OpType.Intersection => pairs.Where(i => i.FirstEntersSecond || i.SecondEntersFirst).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(opType), opType, null)
        };
        if (workingPairs.Count == 0)
            throw new UnreachableException();
        
        var results = new List<BoundaryLoop>();
        while (ExtractOneLoop(l0, l1, workingPairs, opType) is { } loop)
        {
            results.Add(loop);
        }

        return results.ToArray();
    }
    
    private static OpStart GetStart(List<IntersectionPair> filteredPairs, OpType opType)
    {
        var startPair = filteredPairs[0];
        var onL0 = opType switch {
            OpType.Union => startPair.FirstExitsSecond,
            OpType.Intersection => startPair.FirstEntersSecond,
            _ => throw new ArgumentOutOfRangeException(nameof(opType), opType, null)
        };
        return new OpStart(onL0, startPair);
    }
    
    private static BoundaryLoop? ExtractOneLoop(BoundaryLoop l0, BoundaryLoop l1, List<IntersectionPair> pairs, OpType opType)
    {
        // The pairs should already be filtered to only include valid pairs for the operation type
        if (pairs.Count == 0) return null;

        var start = GetStart(pairs, opType);

        // We have a starting intersection, so we can begin the merge process.  We'll create a new contour to hold the
        // merged result, and we'll maintain a write cursor pointing at its tail.  We'll begin by adding the starting
        // contour point from the starting intersection element.
        var merge = new SingleMergeState(l0, l1, pairs, opType, start);

        while (merge.CheckRunning())
        {
            if (merge.PopNext() is { } next)
            {
                // Changing this from `start.IsEquivalentTo(next)` was part of fixing ShapeOpsTests.DegenerateMerge
                if (next.Point.DistanceTo(start.Start.Point) < GeometryConstants.DistEquals)
                {
                    // We've returned to the starting point, so we can close the loop and return the result
                    break;
                }
                
                // Switch the read cursor to the other contour on this intersection pair
                merge.SwitchReadAtPair(next);
                merge.WriteSplitAfter(next);
            }
            else
            {
                merge.Read.MoveForward();
                merge.WriteFullEntity(merge.Read.Current);
            }
            
        }
        
        // I'm not 100% sure about this, but it was part of fixing ShapeOpsTests.DegenerateMerge
        pairs.Remove(start.Start);
        merge.Working.RemoveAdjacentRedundancies();
        
        return merge.Working;
    }

    private record OpStart(bool IsOnL0, IntersectionPair Start);

    private class SingleMergeState
    {
        private readonly BoundaryLoop _l0;
        private readonly BoundaryLoop _l1;
        private readonly int _initialPairs;
        private int _iterCount;
        private readonly OpType _opType;

        public SingleMergeState(BoundaryLoop l0, BoundaryLoop l1, List<IntersectionPair> workingPairs, OpType opType, OpStart start)
        {
            _l0 = l0;
            _l1 = l1;
            WorkingPairs = workingPairs;
            _opType = opType;
            IsOnL0 = start.IsOnL0;
            _initialPairs = workingPairs.Count;
            
            Working = new BoundaryLoop();
            Write = Working.GetCursor();

            Read = MakeRead(start.Start);
            WriteSplitAfter(start.Start);
        }
        
        public List<IntersectionPair> WorkingPairs { get; }
        public BoundaryLoop Working { get; }
        public IBoundaryLoopCursor Write { get; }
        public IBoundaryLoopCursor Read { get; private set; }
        public double LastL { get; private set; }
        public bool IsOnL0 { get; private set; }

        public Position PairPosition(IntersectionPair pair)
        {
            return IsOnL0 ? pair.First : pair.Second;
        }

        public void SwitchReadAtPair(IntersectionPair pair)
        {
            IsOnL0 = !IsOnL0;
            Read = MakeRead(pair);
        }

        public bool CheckRunning()
        {
            _iterCount++;
            if (_iterCount > _l0.Count + _l1.Count + _initialPairs)
            {
                throw new InvalidOperationException("Merge operation did not terminate");
            }
            return WorkingPairs.Count > 0;
        }

        public void WriteSplitAfter(IntersectionPair p)
        {
            var position = PairPosition(p);
            LastL = position.L;
            Write.InsertFromElement(position.Element.SplitAfter(LastL));
        }

        public void WriteFullEntity(BoundaryPoint p)
        {
            LastL = 0;
            Write.InsertAfter(p);
        }

        /// <summary>
        /// This method will check the current entity that the `Read` cursor is pointing at to see if there are any
        /// intersections further along it than the length of the last inserted entity.  If there are, it will
        /// remove it from the working pairs and return it.  If there are not, it will return null.
        ///
        /// A `null` value means there's nothing further to do on this entity and the read cursor can advance. A
        /// non-`null` value means that we need to insert the next entity from the intersection pair and switch to the
        /// other contour.
        /// </summary>
        /// <returns></returns>
        public IntersectionPair? PopNext()
        {
            // We want to check if there are any more valid intersections on the current entity that are further along
            // than the last inserted length.  The `more` list will contain all such intersections.
            var more = IsOnL0 
                ? WorkingPairs.Where(i => i.First.Element.Index == Read.CurrentId && i.First.L > LastL).ToList()
                : WorkingPairs.Where(i => i.Second.Element.Index == Read.CurrentId && i.Second.L > LastL).ToList();
            
            // If the `more` list is empty, there's nothing for us to do, and we can return null to indicate that this
            // entity is clear of further intersections.
            if (more.Count == 0) return null;

            // If there *are* more intersections, we want to find the one that is next along the length of the entity.
            var next = IsOnL0 ? more.MinBy(i => i.First.L) : more.MinBy(i => i.Second.L);

            // We remove the intersection pair from the working list of pairs to mark that it's been consumed and 
            // doesn't need to be considered again by this algorithm or the outer merge algorithm.  This mechanism will
            // ultimately be how the outer merge algorithm knows that all valid contours have been assembled.
            WorkingPairs.Remove(next);
            return next;
        }
        
        private IBoundaryLoopCursor MakeRead(IntersectionPair pair)
        {
            return IsOnL0 ? _l0.GetCursor(pair.First.Element.Index) : _l1.GetCursor(pair.Second.Element.Index);
        }
    }

    // public static (MutateResult, BoundaryLoop[]) Mutate(this BoundaryLoop working, BoundaryLoop tool)
    // {
    //
    //     var (relation, intersections) = working.RelationTo(tool);
    //
    //     return relation switch
    //     {
    //         // The working shape completely encloses the tool, so the result is the working shape, unless the two shapes
    //         // are opposite positive/negative *and* there are intersections, in which case there is a shared edge
    //         BoundaryRelation.Encloses => intersections.Length == 0 || working.IsPositive == tool.IsPositive
    //             ? (MutateResult.ShapeEnclosesTool, [working])
    //             : MergeFromPairs(working, tool, intersections),
    //         
    //         // The tool completely encloses the working shape, so if the two shapes have the same polarity, the result
    //         // is that the working shape is subsumed by the tool, or if they have opposite polarities, the result is
    //         // that the working shape is destroyed.
    //         BoundaryRelation.EnclosedBy => working.IsPositive == tool.IsPositive ?
    //             (MutateResult.Subsumed, [tool]) : (MutateResult.Destroyed, []),
    //         
    //         // The working shape and the tool are completely disjoint, so the result is the unmodified working shape
    //         BoundaryRelation.DisjointTo => (MutateResult.Disjoint, [working]),
    //         
    //         // There is at least one intersection between the working shape and the tool, so the result is computed
    //         // by the merge method
    //         BoundaryRelation.Intersects => MergeFromPairs(working, tool, intersections),
    //         
    //         _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
    //     };
    // }
    //
    //
    // private static (MutateResult, BoundaryLoop[]) MergeFromPairs(BoundaryLoop working, BoundaryLoop tool, IntersectionPair[] pairs)
    // {
    //     var workingPairs = pairs.ToList();
    //     var results = new List<BoundaryLoop>();
    //     while (AttemptOneMerge(working, tool, workingPairs) is { } merged)
    //     {
    //         results.Add(merged);
    //     }
    //
    //     return (MutateResult.Merged, results.ToArray());
    // }
    //
    // private static BoundaryLoop? AttemptOneMerge(BoundaryLoop working, BoundaryLoop tool, List<IntersectionPair> pairs)
    // {
    //     /* This single attempt merge algorithm attempts to assemble out one complete contour which starts on the working
    //      * contour and hops between the working and tool contours at each intersection as it traces its way through,
    //      * until it comes back to its starting point.  As it goes, it removes intersection pairs from the working
    //      * `pairs` list, leaving the remaining pairs to be used in subsequent attempts.  Once this algorithm returns
    //      * `null`, it means that no further complete contours can be assembled from the remaining intersection pairs,
    //      * and the outer algorithm can terminate.
    //      *
    //      * The `IntersectionPair` objects each contain a set of two `Position` objects, which are the corresponding
    //      * positions on the working and tool entities where the intersection occurs. The `First` property will always
    //      * refer to the position on the working contour's entity, and the `Second` property will always refer to the
    //      * corresponding position on the tool contour's entity.
    //      *
    //      * The first challenge is to know for sure that we will be assembling a contour which includes the *valid* part
    //      * of the working contour, and not a part of it that gets obliterated by the tool.  If we can manage to identify
    //      * a valid portion of the remaining working contour, then we can start on it and proceed to hop between the
    //      * two contours whenever we run into an intersection, knowing that by doing so we will be returning the correct
    //      * part of the result.  If we were to start on the *wrong* portion of the working contour, we'd return a contour
    //      * that corresponds with the redundant part of the working and tool contours.
    //      *
    //      * To identify a valid starting point, we need to know if the tool represents a positive or negative shape. If
    //      * the tool is positive, a valid part of the working contour will start at any intersection where the working
    //      * contour boundary is *exiting* the tool (passing through the tool boundary from inside to outside).  If the
    //      * tool is negative, a valid part of the working contour will start at any intersection where the working
    //      * contour is *entering* the tool (passing through the tool boundary from outside to inside).  These rules
    //      * hold regardless of whether the working contour is positive or negative.
    //      *
    //      * With a valid starting point, we begin the merge process.  We put a read cursor on the working contour at
    //      * the starting intersection and a write cursor on a new contour.  Initially we'll add the portion of the
    //      * working contour starting entity that begins at the intersection.
    //      *
    //      * We'll have to keep track of what entity we're currently on, and how far along the length of that entity
    //      * we've inserted the boundary into the result contour.  The current entity is defined by the ID of the entity
    //      * under the read cursor and knowledge of whether the read cursor is pointing at the working or tool contour.
    //      *
    //      * At each step, we'll need to look at the remaining intersection pairs and see if there are any on the the
    //      * current entity that are further along than the current position.  If there are, we'll pick the one that is
    //      * next along the length of the entity.
    //      * TODO: Can we use knowledge of if we should be entering or exiting to filter out weird cases?
    //      *
    //      * If we've found a valid intersection on the current entity, we will use this to hop to the other contour. We
    //      * must *remove* that intersection pair from the working list of pairs, as this is the mechanism to track the
    //      * progress of the outer merge.
    //      *
    //      * We want to move the read cursor to the entity on the opposite contour of the intersection pair. Then we'll
    //      * need to insert the portion of that entity that comes *after* the intersection point.  We'll update the
    //      * last inserted length.
    //      *
    //      * We must then check *again* if there are any intersections on the current entity that are further along than
    //      * the last inserted length.  If there are, we again pick the next one and repeat these steps.
    //      *
    //      * Then, when there are no more intersections left on the current entity in the forward direction along the
    //      * contour boundary, we can finally move the read cursor forward to the next entity in whatever contour it is
    //      * pointing at.  We can insert this entity (remembering that the "entity" really just refers to the start
    //      * point and the type of path it uses to get to its next neighbor) and update the last inserted length to 0.
    //      *
    //      * We'll continue this process until we pop the starting intersection again, at which point we will have
    //      * closed the contour (without re-adding the starting intersection) and we can terminate the merge, returning
    //      * the result.
    //      */
    //
    //     if (pairs.Count == 0) return null;
    //     
    //     // First we want to find a starting intersection pair, which is a location where we know we're starting on a
    //     // valid section of the working contour.  When the tool is positive, this will be an intersection where
    //     // the working contour exits the tool, and when the tool is negative, this will be an intersection where
    //     // working enters the tool.  
    //     var start = FindStart(tool, pairs);
    //     
    //     // If no such intersection exists, there are no portions of c0 that will remain after the merge with an 
    //     // intersection still in the working list of pairs
    //     if (start.Empty) return null;
    //
    //     // We have a starting intersection, so we can begin the merge process.  We'll create a new contour to hold the
    //     // merged result, and we'll maintain a write cursor pointing at its tail.  We'll begin by adding the starting
    //     // contour point from the starting intersection element.
    //     var merge = new SingleMergeState(working, tool, pairs);
    //     merge.SetReadTo(start);
    //     merge.WriteSplitAfter(start);
    //
    //     while (merge.CheckRunning())
    //     {
    //         if (merge.PopNext() is { } next)
    //         {
    //             // Changing this from `start.IsEquivalentTo(next)` was part of fixing ShapeOpsTests.DegenerateMerge
    //             if (next.Point.DistanceTo(start.Point) < GeometryConstants.DistEquals)
    //             {
    //                 // We've returned to the starting point, so we can close the loop and return the result
    //                 break;
    //             }
    //             
    //             // Switch the read cursor to the other contour on this intersection pair
    //             merge.SwitchReadAtPair(next);
    //             merge.WriteSplitAfter(next);
    //         }
    //         else
    //         {
    //             merge.Read.MoveForward();
    //             merge.WriteFullEntity(merge.Read.Current);
    //         }
    //         
    //     }
    //     
    //     // I'm not 100% sure about this, but it was part of fixing ShapeOpsTests.DegenerateMerge
    //     pairs.Remove(start);
    //     merge.Working.RemoveAdjacentRedundancies();
    //     
    //     return merge.Working;
    // }
}