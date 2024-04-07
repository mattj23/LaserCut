using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class PointLoopTests
{
    [Fact]
    public void PointLoopStartsEmpty()
    {
        var loop = new PointLoop();
        Assert.Equal(0, loop.Count);
    }

    [Fact]
    public void DrawRectangle()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertRelX(4);
        cursor.InsertRelY(2);
        cursor.InsertRelX(-4);
        
        var expected = new[]
        {
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 2),
            new Point2D(0, 2)
        };
        
        var values = loop.ToItemArray();
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void ReverseRectangle()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertRelX(4);
        cursor.InsertRelY(2);
        cursor.InsertRelX(-4);
        
        loop.Reverse();
        
        var expected = new[]
        {
            new Point2D(0, 2),
            new Point2D(4, 2),
            new Point2D(4, 0),
            new Point2D(0, 0),
        };
        
        var values = loop.ToItemArray();
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void AreaOfRectangle()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertRelX(4);
        cursor.InsertRelY(2);
        cursor.InsertRelX(-4);
        
        Assert.Equal(8, loop.Area);
    }
    
    [Fact]
    public void AreaOfInverseRectangle()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertRelX(4);
        cursor.InsertRelY(2);
        cursor.InsertRelX(-4);
        
        loop.Reverse();
        
        Assert.Equal(-8, loop.Area);
    }

    [Fact]
    public void OffsetPositive()
    {
        var loop = CreateRectangle();
        loop.Offset(0.5);
        
        var expected = new[]
        {
            new Point2D(-0.5, -0.5),
            new Point2D(4.5, -0.5),
            new Point2D(4.5, 2.5),
            new Point2D(-0.5, 2.5)
        };
        
        var values = loop.ToItemArray();
        Assert.Equal(expected, values);
    }
    
    private PointLoop CreateRectangle()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertRelX(4);
        cursor.InsertRelY(2);
        cursor.InsertRelX(-4);
        
        return loop;
    }
}