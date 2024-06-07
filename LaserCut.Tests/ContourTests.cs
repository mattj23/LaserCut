using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class ContourTests
{
    [Fact]
    public void ContourStartsEmpty()
    {
        var contour = new Contour();
        Assert.Equal(0, contour.Count);
    }

    [Fact]
    public void SingleElementContourFull()
    {
        // Creates a single element which is a full circle arc starting at 0
        var contour = new Contour();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new ContourArc(new Point2D(0, 0), new Point2D(1, 0), false));
        
        Assert.Equal(1, contour.Count);

        var e = contour.Elements[0];
        Assert.IsType<Arc>(e);
        var arc = (Arc)e;
        Assert.Equal(Math.PI * 2, arc.Theta, 1e-10);
    }

    [Fact]
    public void RectangleArea()
    {
        var contour = new Contour();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new ContourLine(new Point2D(0, 0)));
        cursor.InsertAfter(new ContourLine(new Point2D(2, 0)));
        cursor.InsertAfter(new ContourLine(new Point2D(2, 1)));
        cursor.InsertAfter(new ContourLine(new Point2D(0, 1)));
        
        Assert.Equal(2.0, contour.Area, 1e-10);
    }
    
    [Fact]
    public void RectangleAreaNegative()
    {
        var contour = new Contour();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new ContourLine(new Point2D(0, 0)));
        cursor.InsertAfter(new ContourLine(new Point2D(0, 1)));
        cursor.InsertAfter(new ContourLine(new Point2D(2, 1)));
        cursor.InsertAfter(new ContourLine(new Point2D(2, 0)));
        
        Assert.Equal(-2.0, contour.Area, 1e-10);
    }

    [Fact]
    public void PillArea()
    {
        var contour = new Contour();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new ContourLine(new Point2D(0, 0)));
        cursor.InsertAfter(new ContourArc(new Point2D(1, 0), new Point2D(1, 1), false));
        cursor.InsertAfter(new ContourLine(new Point2D(1, 2)));
        cursor.InsertAfter(new ContourArc(new Point2D(0, 2), new Point2D(0, 1), false));
        
        Assert.Equal(Math.PI + 2, contour.Area, 1e-5);
    }

    
}