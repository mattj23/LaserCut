using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Contours;

public class BasicContourTests
{
    [Fact]
    public void ContourStartsEmpty()
    {
        var contour = new BoundaryLoop();
        Assert.Equal(0, contour.Count);
    }

    [Fact]
    public void SingleElementContourFull()
    {
        // Creates a single element which is a full circle arc starting at 0
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new BoundaryArc(new Point2D(0, 0), new Point2D(1, 0), false));
        
        Assert.Equal(1, contour.Count);

        var e = contour.Elements[0];
        Assert.IsType<Arc>(e);
        var arc = (Arc)e;
        Assert.Equal(Math.PI * 2, arc.Theta, 1e-10);
    }

    [Fact]
    public void RectangleArea()
    {
        var contour = BoundaryLoop.Rectangle(0, 0, 2, 1);
        Assert.Equal(2.0, contour.Area, 1e-10);
    }
    
    [Fact]
    public void RectangleAreaNegative()
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new BoundaryLine(new Point2D(0, 0)));
        cursor.InsertAfter(new BoundaryLine(new Point2D(0, 1)));
        cursor.InsertAfter(new BoundaryLine(new Point2D(2, 1)));
        cursor.InsertAfter(new BoundaryLine(new Point2D(2, 0)));
        
        Assert.Equal(-2.0, contour.Area, 1e-10);
    }

    [Fact]
    public void CircleArea()
    {
        var contour = BoundaryLoop.Circle(0, 0, 1);
        Assert.Equal(Math.PI, contour.Area, 1e-5);
    }
    
    [Fact]
    public void CircleAreaNegative()
    {
        var contour = BoundaryLoop.Circle(0, 0, 1, true);
        Assert.Equal(-Math.PI, contour.Area, 1e-5);
    }
    
    [Fact]
    public void ReversedCircleArea()
    {
        var contour = BoundaryLoop.Circle(0, 0, 1);
        var r = contour.Reversed();
        Assert.Equal(-Math.PI, r.Area, 1e-5);
    }

    [Fact]
    public void PillArea()
    {
        var contour = PillContour();
        Assert.Equal(Math.PI + 2, contour.Area, 1e-5);
    }

    [Fact]
    public void ReversedPillArea()
    {
        var contour = PillContour();
        var r = contour.Reversed();
        Assert.Equal(-(Math.PI + 2), r.Area, 1e-5);
    }

    /// <summary>
    /// Creates a contour that is a rectangle with a half circle on each end.
    /// </summary>
    /// <returns></returns>
    private BoundaryLoop PillContour()
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new BoundaryLine(new Point2D(0, 0)));
        cursor.InsertAfter(new BoundaryArc(new Point2D(1, 0), new Point2D(1, 1), false));
        cursor.InsertAfter(new BoundaryLine(new Point2D(1, 2)));
        cursor.InsertAfter(new BoundaryArc(new Point2D(0, 2), new Point2D(0, 1), false));
        return contour;
    }
    
}