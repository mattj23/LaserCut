﻿using LaserCut.Geometry;

namespace LaserCut.Tests.Contours;

public class OffsetContourTests
{
    [Fact]
    public void SimpleOffsetCircle()
    {
        var c = Contour.Circle(0, 0, 1);
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
        var c = Contour.Rectangle(0, 0, 1, 1);
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
        var c = new Contour();
        var cursor = c.GetCursor();
        cursor.ArcAbs(0, 0, 0, 1, false);
        cursor.SegAbs(0, 2);
        
        Assert.Equal(Math.PI / 2.0, c.Area, 1e-5);
        
        var o = c.Offset(0.5);
        Assert.Equal((1.5 * 1.5 * Math.PI) / 2.0, o.Area, 1e-5);
    }
    
}