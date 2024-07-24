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
            ShapeRelation.Intersects => IntersectionResult(l0, l1, intersections),
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
        };
    }
    
    private static (BoundaryOpResult, BoundaryLoop[]) IntersectionResult(BoundaryLoop l0, BoundaryLoop l1, IntersectionPair[] pairs)
    {
        var result = OperateFromPairs(l0, l1, pairs, OpType.Intersection);
        return result.Length == 0 ? (BoundaryOpResult.Destroyed, result) : (BoundaryOpResult.Merged, result);
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
    
    private static bool ValidForFirst(IntersectionPair pair, OpType opType)
    {
        return opType switch {
            OpType.Union => pair.FirstExitsSecond,
            OpType.Intersection => pair.FirstEntersSecond,
            _ => throw new ArgumentOutOfRangeException(nameof(opType), opType, null)
        };
    }
    
    private static bool ValidForSecond(IntersectionPair pair, OpType opType)
    {
        return opType switch {
            OpType.Union => pair.SecondExitsFirst,
            OpType.Intersection => pair.SecondEntersFirst,
            _ => throw new ArgumentOutOfRangeException(nameof(opType), opType, null)
        };
    }
    
    private static BoundaryLoop[] OperateFromPairs(BoundaryLoop l0, BoundaryLoop l1, IntersectionPair[] pairs, OpType opType)
    {
        // Filter out any pairs which are not valid for the operation type
        var workingPairs = pairs.Where(i => ValidForFirst(i, opType) || ValidForSecond(i, opType)).ToList();
        if (workingPairs.Count == 0)
            throw new UnreachableException();
        
        var results = new List<BoundaryLoop>();
        while (ExtractOneLoop(l0, l1, workingPairs, opType) is { } loop)
        {
            if (!loop.IsNullSet) results.Add(loop);
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
                if (start.Start.IsEquivalentTo(next))
                {
                    // We've returned to the starting point, so we can close the loop and return the result
                    break;
                }
                
                // Switch the read cursor to the loop which is valid after this intersection.
                merge.SwitchReadAtPair(next);
                merge.WriteSplitAfter(next);
            }
            else
            {
                merge.Read.MoveForward();
                merge.WriteFullEntity(merge.Read.Current);
            }
            
        }
        
        merge.Working.RemoveThinSections();
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
            LastL += GeometryConstants.DistEquals * 1.5;
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
            if (ValidForFirst(pair, _opType))
            {
                IsOnL0 = true;
            }
            else if (ValidForSecond(pair, _opType))
            {
                IsOnL0 = false;
            }
            else
            {
                throw new InvalidOperationException("Invalid pair for operation");
            }
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
            
            // If this is the first entity we've written, we need to pad the LastL to avoid picking up the start again
            if (Working.Count == 1)
            {
                 LastL += GeometryConstants.DistEquals * 2;
            }
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
                ? WorkingPairs.Where(i => i.First.Element.Index == Read.CurrentId && i.First.L > LastL - GeometryConstants.DistEquals).ToList()
                : WorkingPairs.Where(i => i.Second.Element.Index == Read.CurrentId && i.Second.L > LastL - GeometryConstants.DistEquals).ToList();
            
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

}