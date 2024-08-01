using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.BoundaryLoops;

public class OffsetTests
{
    [Fact]
    public void SimpleOffsetCircle()
    {
        var c = BoundaryLoop.Circle(0, 0, 1);
        var o = c.Offset(0.5);

        Assert.Equal(1.5 * 1.5 * Math.PI, o.Area, 1e-5);
        Assert.Equal(-1.5, o.Bounds.MinX, 1e-5);
        Assert.Equal(-1.5, o.Bounds.MinY, 1e-5);
        Assert.Equal(1.5, o.Bounds.MaxX, 1e-5);
        Assert.Equal(1.5, o.Bounds.MaxY, 1e-5);
    }
    
    [Fact]
    public void SimpleOffsetRectangle()
    {
        var c = BoundaryLoop.Rectangle(0, 0, 1, 1);
        var o = c.Offset(0.5);

        Assert.Equal(2 * 2, o.Area, 1e-5);
        Assert.Equal(-0.5, o.Bounds.MinX, 1e-5);
        Assert.Equal(-0.5, o.Bounds.MinY, 1e-5);
        Assert.Equal(1.5, o.Bounds.MaxX, 1e-5);
        Assert.Equal(1.5, o.Bounds.MaxY, 1e-5);
    }

    [Fact]
    public void SimpleOffsetHalfCircle()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();
        cursor.ArcAbs(0, 0, 0, 1, false);
        cursor.SegAbs(0, 2);
        
        Assert.Equal(Math.PI / 2.0, c.Area, 1e-5);
        
        var o = c.Offset(0.5);

        Assert.IsType<BoundaryArc>(o.Head);
        Assert.IsType<BoundaryLine>(o.Tail);
        
        var testArc = (Arc)o.ElementsById[o.HeadId];
        var testSeg = (Segment)o.ElementsById[o.TailId];
        
        Assert.Equal(new Point2D(0, 1), testArc.Center, PointCheck.Default);
        Assert.Equal(-0.5, testSeg.Start.X, 1e-5);
        Assert.Equal(-0.5, testSeg.End.X, 1e-5);
        
        Assert.Equal(testSeg.Start, testArc.End, PointCheck.Default);
        Assert.Equal(testArc.Start, testSeg.End, PointCheck.Default);
    }
    
}