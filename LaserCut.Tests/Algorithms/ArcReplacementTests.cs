using LaserCut.Algorithms;
using LaserCut.Geometry;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Algorithms;

public class ArcReplacementTests
{
    [Fact]
    public void SimpleArcReplacement()
    {
        var loop = ToSegments(HalfCircle());
        
        loop.ReplaceLinesWithArcs(4, 0.01);
        
        Assert.Equal(2, loop.Count);
        Assert.True(loop.Head is BoundaryLine);
        Assert.True(loop.Tail is BoundaryArc);
        Assert.Equal(Point2D.Origin, (loop.Tail as BoundaryArc)!.Center, PointCheck.Default);
        Assert.Equal(new Point2D(0, -1), (loop.Tail as BoundaryArc)!.Point, PointCheck.Default);
    }

    [Fact]
    public void RoundedRectangle()
    {
        var loop = ToSegments(BoundaryLoop.RoundedRectangle(0, 0, 5, 4, 1));
        loop.ReplaceLinesWithArcs(4, 0.01);
        
        Assert.Equal(8, loop.Count);
    }

    [Fact]
    public void FullCircleReplacement()
    {
        var loop = ToSegments(BoundaryLoop.Circle(0, 0, 1.0));
        loop.ReplaceLinesWithArcs(4, 0.01);
        
        Assert.Equal(1, loop.Count);
        Assert.True(loop.Head is BoundaryArc);
        Assert.Equal(Point2D.Origin, (loop.Tail as BoundaryArc)!.Center, PointCheck.Default);
    }

    private BoundaryLoop HalfCircle()
    {
        var loop = new BoundaryLoop();
        var cursor = loop.GetCursor();
        cursor.ArcAbs(0, -1, 0, 0, false);
        cursor.SegAbs(0, 1);
        return loop;
    }

    private BoundaryLoop ToSegments(BoundaryLoop loop)
    {
        var working = new BoundaryLoop();
        var write = working.GetCursor();

        foreach (var e in loop.Elements)
        {
            var l = 0.0;
            while (l < e.Length)
            {
                var p = e.AtLength(l).Point;
                write.SegAbs(p.X, p.Y);
                l += 0.02;
            }
            write.SegAbs(e.End.X, e.End.Y);
        }
        
        working.RemoveZeroLengthElements();
        working.RemoveAdjacentRedundancies();

        return working;
    }
}