using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

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
        cursor.InsertAfter(new Arc(0, 0, 1, 2 * Math.PI, 0));
        
        Assert.Equal(1, contour.Count);
        Assert.True(contour.IsClosed);
    }

    [Fact]
    public void SingleElementContourOpen()
    {
        // Creates a single element which is a half circle arc starting at 0
        var contour = new Contour();
        var cursor = contour.GetCursor();
        cursor.InsertAfter(new Arc(0, 0, 1, Math.PI, 0));
        
        Assert.Equal(1, contour.Count);
        Assert.False(contour.IsClosed);
    }
    
}