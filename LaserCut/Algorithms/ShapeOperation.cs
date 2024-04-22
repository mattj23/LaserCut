using LaserCut.Geometry;

namespace LaserCut.Algorithms;

public static class ShapeOperation
{
    public enum ResultType
    {
        /// <summary>
        /// The shape and the tool are completely disjoint, so the original shape should be left unmodified
        /// </summary>
        Disjoint,
        
        /// <summary>
        /// The shape is completely subsumed by the tool, so the result is the tool
        /// </summary>
        Subsumed,
        
        /// <summary>
        /// The shape is completely destroyed by the tool, and no loop remains
        /// </summary>
        Destroyed,
        
        /// <summary>
        /// The shape and the tool intersect and the result of an operation is a set of one or more loops
        /// </summary>
        Merged,
        
        /// <summary>
        /// The shape completely encloses the tool. If the two entities have the same polarity, the result should be
        /// the shape as it is effectively a no-op. If the entities have opposite polarity the conceptual result is
        /// the shape with the tool as a new internal boundary.
        /// </summary>
        ShapeEnclosesTool
    }
    
    public static PointLoop SimpleMergeLoops(PointLoop loop0, PointLoop loop1)
    {
        // This will return a list of intersection pairs between the two loops. For each intersection, segment 0 will
        // refer to the segment in loop0, and segment 1 will refer to the segment in loop1
        var intersections = loop0.Intersections(loop1);
        
        // If we managed to find a merge, we will return it
        if (MergeOne(loop0, loop1, intersections) is { } merged)
            return merged;

        // If we didn't find a merge, we will return the original loop
        return loop0;
    }
    
    /// <summary>
    /// Performs a shape operation on a loop using another loop as a tool. This method is a building block of cohesive
    /// boolean shape operations on bodies. Shape operations on loops are not algebraically closed, so the potential
    /// result of any operation may vary, and this method primarily returns an enum representing the kind of result
    /// which the operation produces.  Only in the case of a merge operation from intersecting loops will a set of
    /// loops be returned, in all other cases the array will be empty and the result type will indicate what should be
    /// done with the original shape.
    ///
    /// Because this is a shape operation, it is not commutative. The shape is the loop which is being acted on, and
    /// the result will represent a transformation (if any) of the shape. The tool is not directly a part of the result,
    /// as it is not conceptually modified by the shape operation.
    /// </summary>
    /// <param name="shape">The loop which represents the shape which is being acted on by the tool. May have positive
    /// or negative area.</param>
    /// <param name="tool">The loop which represents the tool which is acting on the shape. May have positive or
    /// negative area.</param>
    /// <returns></returns>
    public static (ResultType Encloses, PointLoop[]) Operate(PointLoop shape, PointLoop tool)
    {
        /*
         * There are a few different cases that can occur when performing a shape operation on a loop using another
         * loop as a tool:
         *
         * 1. The two loops are completely disjoint, in which case the original shape is unaffected by the operation
         * 
         * 2. The shape is completely inside the tool, in which case the result depends on the polarity of the
         * two entities:
         *    - If the entities have the same polarity, the result is the tool
         *    - If the entities have opposite polarity, the original shape is completely destroyed
         * 
         * 3. If the tool is completely inside the shape:
         *    - If the entities have the same polarity, the result is the shape (this is effectively a no-op)
         *    - If the entities have opposite polarity, the result is theoretically the original shape with an internal
         *      boundary, but this is not representable in a single loop
         *
         * 4. The loops intersect, in which case the result is a merge operation from a start point on the shape. If
         * there is no appropriate start point it will be because the shape is effectively "inside" the tool, and the
         * result is the same as case 2.
         */
        
        // This will return a list of intersection pairs between the two loops. For each intersection, segment 0 will
        // refer to the segment in `shape`, and segment 1 will refer to the segment in `tool`
        var intersections = shape.Intersections(tool);

        // If there are no intersections, we will determine if the shapes are disjoint or if one is inside the other
        if (intersections.Count == 0)
        {
            if (shape.ContainsPoint(tool.Head))
            {
                return (ResultType.ShapeEnclosesTool, []);
            }

            if (tool.ContainsPoint(shape.Head))
            {
                return (shape.Polarity == tool.Polarity ? ResultType.Subsumed : ResultType.Destroyed, []);
            }

            return (ResultType.Disjoint, []);
        }
        
        // Find the start point for the merge operation
        var start = FindStart(shape, tool, intersections);

        if (start.Empty)
        {
            // No starting point means that the shape never emerges from inside the tool, which is effectively the 
            // same as it being enclosed by the tool
            return (shape.Polarity == tool.Polarity ? ResultType.Subsumed : ResultType.Destroyed, []);
        }
        

        var results = new List<PointLoop>();
        
        // While we have intersections, we will merge them
        while (MergeOne(shape, tool, intersections) is { } merged)
        {
            results.Add(merged);
        }

        return (ResultType.Merged, results.ToArray());
    }
    
    private static PointLoop? MergeOne(PointLoop loop0, PointLoop loop1, List<SegPairIntersection> intersections)
    {
        var initialIntersections = intersections.Count;
        if (initialIntersections == 0) return null;
        
        // First, we want to find an intersection pair where segment 0's direction has a positive dot product with 
        // segment 1's normal. This is an intersection where this loop is emerging from inside the other loop.
        var start = FindStart(loop0, loop1, intersections);
        if (start.Empty) return null;
        
        // We will create a new point loop and begin from the start intersection
        var working = new PointLoop();
        var workingCursor = working.GetCursor();
        workingCursor.InsertAbs(start.Point);

        // Now we will work our way through the loop.
        var lastT = start.T0;
        var readCursor = loop0.GetCursor(start.Segment0.Index);
        bool isLoop0 = true;

        while (intersections.Count != 0)
        {
            // Are there any intersections between where we are and the next segment?
            if (PopNext(intersections, lastT, isLoop0, readCursor.CurrentId) is { } next)
            {
                // Had to change this to pass the DegenerateMerge case and keep it from getting stuck because three
                // intersections were being returned
                // if (next.Equals(start)) 
                
                if (next.Point.DistanceTo(start.Point) < GeometryConstants.DistEquals)
                {
                    // We have reached the end of the loop
                    break;
                }
                
                // If so, we will insert the intersection point into the working loop
                workingCursor.InsertAbs(next.Point);
                
                // Now we will switch to the other loop
                isLoop0 = !isLoop0;
                lastT = isLoop0 ? next.T0 : next.T1;
                readCursor = isLoop0 ? loop0.GetCursor(next.Segment0.Index) : loop1.GetCursor(next.Segment1.Index);
            }
            // Otherwise, we will advance to the start of the next segment and insert it into the loop
            else
            {
                // Detect if we're stuck in an infinite loop
                if (working.Count > loop0.Count + loop1.Count + initialIntersections) 
                {
                    throw new Exception("Merge is stuck in a loop");
                }
                
                readCursor.MoveForward();
                workingCursor.InsertAbs(readCursor.Current);
                lastT = 0;
            }
        }

        working.RemoveAdjacentDuplicates();

        return working;
    }

    private static SegPairIntersection FindStart(PointLoop loop0, PointLoop loop1,
        List<SegPairIntersection> intersections)
    {
        if (loop0.Area > 0 && loop1.Area > 0)
            return intersections.FirstOrDefault(i => i.Seg0ExitsSeg1);
        
        if (loop0.Area < 0 && loop1.Area < 0)
            return intersections.FirstOrDefault(i => i.Seg0EntersSeg1);
        
        if (loop0.Area < 0 && loop1.Area > 0)
            return intersections.FirstOrDefault(i => i.Seg0ExitsSeg1);
        
        return intersections.FirstOrDefault(i => i.Seg0EntersSeg1);
    }

    private static SegPairIntersection? PopNext(List<SegPairIntersection> intersections, double lastT, bool isLoop0, int currentId)
    {
        var more = isLoop0
            ? intersections.Where(i => i.Segment0.Index == currentId && i.T0 > lastT).ToList()
            : intersections.Where(i => i.Segment1.Index == currentId && i.T1 > lastT).ToList();
        
        if (more.Count == 0) return null;

        var next = isLoop0
            ? more.MinBy(i => i.T0)
            : more.MinBy(i => i.T1);

        intersections.Remove(next);
        return next;
    }
    
    
    
}