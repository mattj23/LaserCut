using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// This class consolidates the algorithms for working with merges of `Contour` objects.
/// </summary>
public static class ShapeOps
{
    public static (MutateResult, Contour[]) Mutate(this Contour working, Contour tool)
    {

        var (relation, intersections) = working.RelationTo(tool);

        return relation switch
        {
            // The working shape completely encloses the tool, so the result is the working shape, unless the two shapes
            // are opposite positive/negative *and* there are intersections, in which case there is a shared edge
            ContourRelation.Encloses => intersections.Length == 0 || working.IsPositive == tool.IsPositive
                ? (MutateResult.ShapeEnclosesTool, [working])
                : MergeFromPairs(working, tool, intersections),
            
            // The tool completely encloses the working shape, so if the two shapes have the same polarity, the result
            // is that the working shape is subsumed by the tool, or if they have opposite polarities, the result is
            // that the working shape is destroyed.
            ContourRelation.EnclosedBy => working.IsPositive == tool.IsPositive ?
                (MutateResult.Subsumed, [tool]) : (MutateResult.Destroyed, []),
            
            // The working shape and the tool are completely disjoint, so the result is the unmodified working shape
            ContourRelation.DisjointTo => (MutateResult.Disjoint, [working]),
            
            // There is at least one intersection between the working shape and the tool, so the result is computed
            // by the merge method
            ContourRelation.Intersects => MergeFromPairs(working, tool, intersections),
            
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
        };
    }

    private static (MutateResult, Contour[]) MergeFromPairs(Contour working, Contour tool, IntersectionPair[] pairs)
    {
        var workingPairs = pairs.ToList();
        var results = new List<Contour>();
        while (AttemptOneMerge(working, tool, workingPairs) is { } merged)
        {
            results.Add(merged);
        }

        return (MutateResult.Merged, results.ToArray());
    }

    private static Contour? AttemptOneMerge(Contour working, Contour tool, List<IntersectionPair> pairs)
    {
        /* This single attempt merge algorithm attempts to assemble out one complete contour which starts on the working
         * contour and hops between the working and tool contours at each intersection as it traces its way through,
         * until it comes back to its starting point.  As it goes, it removes intersection pairs from the working
         * `pairs` list, leaving the remaining pairs to be used in subsequent attempts.  Once this algorithm returns
         * `null`, it means that no further complete contours can be assembled from the remaining intersection pairs,
         * and the outer algorithm can terminate.
         *
         * The `IntersectionPair` objects each contain a set of two `Position` objects, which are the corresponding
         * positions on the working and tool entities where the intersection occurs. The `First` property will always
         * refer to the position on the working contour's entity, and the `Second` property will always refer to the
         * corresponding position on the tool contour's entity.
         *
         * The first challenge is to know for sure that we will be assembling a contour which includes the *valid* part
         * of the working contour, and not a part of it that gets obliterated by the tool.  If we can manage to identify
         * a valid portion of the remaining working contour, then we can start on it and proceed to hop between the
         * two contours whenever we run into an intersection, knowing that by doing so we will be returning the correct
         * part of the result.  If we were to start on the *wrong* portion of the working contour, we'd return a contour
         * that corresponds with the redundant part of the working and tool contours.
         *
         * To identify a valid starting point, we need to know if the tool represents a positive or negative shape. If
         * the tool is positive, a valid part of the working contour will start at any intersection where the working
         * contour boundary is *exiting* the tool (passing through the tool boundary from inside to outside).  If the
         * tool is negative, a valid part of the working contour will start at any intersection where the working
         * contour is *entering* the tool (passing through the tool boundary from outside to inside).  These rules
         * hold regardless of whether the working contour is positive or negative.
         *
         * With a valid starting point, we begin the merge process.  We put a read cursor on the working contour at
         * the starting intersection and a write cursor on a new contour.  Initially we'll add the portion of the
         * working contour starting entity that begins at the intersection.
         *
         * We'll have to keep track of what entity we're currently on, and how far along the length of that entity
         * we've inserted the boundary into the result contour.  The current entity is defined by the ID of the entity
         * under the read cursor and knowledge of whether the read cursor is pointing at the working or tool contour.
         *
         * At each step, we'll need to look at the remaining intersection pairs and see if there are any on the the
         * current entity that are further along than the current position.  If there are, we'll pick the one that is
         * next along the length of the entity.
         * TODO: Can we use knowledge of if we should be entering or exiting to filter out weird cases?
         *
         * If we've found a valid intersection on the current entity, we will use this to hop to the other contour. We
         * must *remove* that intersection pair from the working list of pairs, as this is the mechanism to track the
         * progress of the outer merge.
         *
         * We want to move the read cursor to the entity on the opposite contour of the intersection pair. Then we'll
         * need to insert the portion of that entity that comes *after* the intersection point.  We'll update the
         * last inserted length.
         *
         * We must then check *again* if there are any intersections on the current entity that are further along than
         * the last inserted length.  If there are, we again pick the next one and repeat these steps.
         *
         * Then, when there are no more intersections left on the current entity in the forward direction along the
         * contour boundary, we can finally move the read cursor forward to the next entity in whatever contour it is
         * pointing at.  We can insert this entity (remembering that the "entity" really just refers to the start
         * point and the type of path it uses to get to its next neighbor) and update the last inserted length to 0.
         *
         * We'll continue this process until we pop the starting intersection again, at which point we will have
         * closed the contour (without re-adding the starting intersection) and we can terminate the merge, returning
         * the result.
         */

        if (pairs.Count == 0) return null;
        
        // First we want to find a starting intersection pair, which is a location where we know we're starting on a
        // valid section of the working contour.  When the tool is positive, this will be an intersection where
        // the working contour exits the tool, and when the tool is negative, this will be an intersection where
        // working enters the tool.  
        var start = FindStart(tool, pairs);
        
        // If no such intersection exists, there are no portions of c0 that will remain after the merge with an 
        // intersection still in the working list of pairs
        if (start.Empty) return null;

        // We have a starting intersection, so we can begin the merge process.  We'll create a new contour to hold the
        // merged result, and we'll maintain a write cursor pointing at its tail.  We'll begin by adding the starting
        // contour point from the starting intersection element.
        var merge = new SingleMergeState(working, tool, pairs);
        merge.SetReadTo(start);
        merge.WriteSplitAfter(start);

        while (merge.CheckRunning())
        {
            if (merge.PopNext() is { } next)
            {
                if (start.IsEquivalentTo(next))
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
        
        return merge.Working;
    }

    private class SingleMergeState
    {
        private readonly Contour _c0;
        private readonly Contour _c1;
        private readonly int _initialPairs;
        private int _iterCount;

        public SingleMergeState(Contour c0, Contour c1, List<IntersectionPair> workingPairs)
        {
            _c0 = c0;
            _c1 = c1;
            WorkingPairs = workingPairs;
            _initialPairs = workingPairs.Count;
            
            Working = new Contour();
            Write = Working.GetCursor();

            IsOnC0 = true;
        }
        
        public List<IntersectionPair> WorkingPairs { get; }
        public Contour Working { get; }
        public IContourCursor Write { get; }
        public IContourCursor Read { get; private set; }
        public double LastL { get; private set; }
        public bool IsOnC0 { get; private set; }

        public Position PairPosition(IntersectionPair pair)
        {
            return IsOnC0 ? pair.First : pair.Second;
        }

        public void SetReadTo(IntersectionPair pair)
        {
            Read = IsOnC0 ? _c0.GetCursor(pair.First.Element.Index) : _c1.GetCursor(pair.Second.Element.Index);
        }

        public void SwitchReadAtPair(IntersectionPair pair)
        {
            IsOnC0 = !IsOnC0;
            SetReadTo(pair);
        }

        public bool CheckRunning()
        {
            _iterCount++;
            if (_iterCount > _c0.Count + _c1.Count + _initialPairs)
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

        public void WriteFullEntity(ContourPoint p)
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
            // We want to check if there are any more intersections on the current entity that are further along than
            // the last inserted length.  The `more` list will contain all such intersections.
            var more = IsOnC0 
                ? WorkingPairs.Where(i => i.First.Element.Index == Read.CurrentId && i.First.L > LastL).ToList()
                : WorkingPairs.Where(i => i.Second.Element.Index == Read.CurrentId && i.Second.L > LastL).ToList();
            
            // If the `more` list is empty, there's nothing for us to do, and we can return null to indicate that this
            // entity is clear of further intersections.
            if (more.Count == 0) return null;

            // If there *are* more intersections, we want to find the one that is next along the length of the entity.
            var next = IsOnC0 ? more.MinBy(i => i.First.L) : more.MinBy(i => i.Second.L);

            // We remove the intersection pair from the working list of pairs to mark that it's been consumed and 
            // doesn't need to be considered again by this algorithm or the outer merge algorithm.  This mechanism will
            // ultimately be how the outer merge algorithm knows that all valid contours have been assembled.
            WorkingPairs.Remove(next);
            return next;
        }
    }
    
    private static IntersectionPair FindStart(Contour c1, IReadOnlyList<IntersectionPair> pairs)
    {
        return c1.IsPositive
            ? pairs.FirstOrDefault(i => i.FirstExitsSecond)
            : pairs.FirstOrDefault(i => i.FirstEntersSecond);
    }

}