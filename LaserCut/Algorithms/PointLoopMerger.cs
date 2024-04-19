using LaserCut.Geometry;

namespace LaserCut.Algorithms;

public static class PointLoopMerge
{
    public static PointLoop Merged(PointLoop loop0, PointLoop loop1)
    {
        // This will return a list of intersection pairs between the two loops. For each intersection, segment 0 will
        // refer to the segment in loop0, and segment 1 will refer to the segment in loop1
        var intersections = loop0.Intersections(loop1);

        // If there are no intersections, we can just return loop0 as is
        if (intersections.Count == 0) return loop0.Copy();
        
        // First, we want to find an intersection pair where segment 0's direction has a positive dot product with 
        // segment 1's normal. This is an intersection where this loop is emerging from inside the other loop.
        var start = FindStart(loop0, loop1, intersections);
        
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
                if (next.Equals(start))
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
            return intersections.First(i => i.Seg0ExitsSeg1);
        
        if (loop0.Area < 0 && loop1.Area < 0)
            return intersections.First(i => i.Seg1ExitsSeg0);
        
        if (loop0.Area < 0 && loop1.Area > 0)
            return intersections.First(i => i.Seg0ExitsSeg1);
        
        return intersections.First(i => i.Seg1ExitsSeg0);
    }

    private static SegPairIntersection? PopNext(List<SegPairIntersection> intersections, double lastT, bool isLoop0, int currentId)
    {
        var more = isLoop0
            ? intersections.Where(i => i.Segment0.Index == currentId && i.T0 > lastT).ToList()
            : intersections.Where(i => i.Segment1.Index == currentId && i.T0 > lastT).ToList();
        
        if (more.Count == 0) return null;

        var next = isLoop0
            ? more.MinBy(i => i.T0)
            : more.MinBy(i => i.T1);

        intersections.Remove(next);
        return next;
    }
    
    
    
}