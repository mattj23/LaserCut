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

    [Fact]
    public void RelationOutside()
    {
        var loop0 = CreateRectangle(0, 0, 5, 5);
        var loop1 = CreateRectangle(1, 1, 3, 3);
        
        Assert.Equal(BoundaryRelation.DisjointTo, loop0.RelationTo(loop1));
    }
    
    [Fact]
    public void RelationInside()
    {
        var loop0 = CreateRectangle(0, 0, 5, 5);
        var loop1 = CreateRectangle(1, 1, 3, 3);
        
        Assert.Equal(BoundaryRelation.EnclosedBy, loop1.RelationTo(loop0));
    }
    
    [Fact]
    public void RelationIntersecting()
    {
        var loop0 = CreateRectangle(0, 0, 5, 5);
        var loop1 = CreateRectangle(1, 1, 6, 6);
        
        Assert.Equal(BoundaryRelation.Intersects, loop0.RelationTo(loop1));
        Assert.Equal(BoundaryRelation.Intersects, loop1.RelationTo(loop0));
    }

    [Fact]
    public void MirrorAreaStaysTheSame()
    {
        var loop0 = CreateRectangle();
        var loop1 = loop0.Copy();
        loop1.MirrorY();
        Assert.Equal(loop0.Area, loop1.Area);
    }

    [Fact]
    public void FixSelfIntersections()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertRelY(2.0 - 0.05);
        cursor.InsertRel(0.05, 0.05);
        cursor.InsertRelX(4 - 0.05);
        cursor.InsertRelY(-2.0);

        var items = loop.OffsetAndFixed(0.1);
        // TODO: Make this a better test
        Assert.Single(items);
    }

    [Fact]
    public void RemoveAdjacentDuplicate()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertAbs(1, 0);
        cursor.InsertAbs(1, 0);
        cursor.InsertAbs(1, 1);
        cursor.InsertAbs(0, 1);
        
        loop.RemoveAdjacentDuplicates();
        
        var expected = new[]
        {
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(1, 1),
            new Point2D(0, 1)
        };
        
        var values = loop.ToItemArray();
        Assert.Equal(expected, values);
    }

    [Fact]
    public void RemoveAdjacentCollinear()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(0, 0);
        cursor.InsertAbs(1, 0);
        cursor.InsertAbs(1, 0.5);
        cursor.InsertAbs(1, 1);
        cursor.InsertAbs(0, 1);
        
        loop.RemoveAdjacentCollinear();
        
        var expected = new[]
        {
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(1, 1),
            new Point2D(0, 1)
        };
        
        var values = loop.ToItemArray();
        Assert.Equal(expected, values);
    }

    private PointLoop CreateRectangle(double x0, double y0, double height, double width)
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(x0, y0);
        cursor.InsertRelX(width);
        cursor.InsertRelY(height);
        cursor.InsertRelX(-width);
        return loop;
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