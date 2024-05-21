using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Primitives;

public class ArcTests
{
    [Fact]
    public void CreateFromEndsCCW()
    {
        var s = new Point2D(0, 0);
        var e = new Point2D(1, 1);
        var c = new Point2D(0, 1);
        var a = new Arc(s, e, c, false);
        
        Assert.Equal(1, a.Radius, 1e-10);
        Assert.Equal(Math.PI / 2, a.Theta, 1e-10);
        Assert.Equal(-Math.PI / 2.0, a.Theta0, 1e-10);
        
        Assert.Equal(s, a.PointAtFraction(0), PointCheck.Default);
        Assert.Equal(e, a.PointAtFraction(1), PointCheck.Default);
        Assert.Equal(new Vector2D(1, 0), a.DirectionAtFraction(0), VecCheck.Default);
        Assert.Equal(new Vector2D(0, 1), a.DirectionAtFraction(1), VecCheck.Default);
    }
    
    [Fact]
    public void CreateFromEndsCW()
    {
        var e = new Point2D(0, 0);
        var s = new Point2D(1, 1);
        var c = new Point2D(0, 1);
        var a = new Arc(s, e, c, true);
        
        Assert.Equal(1, a.Radius, 1e-10);
        Assert.Equal(-Math.PI / 2, a.Theta, 1e-10);
        Assert.Equal(0, a.Theta0, 1e-10);
        
        Assert.Equal(s, a.PointAtFraction(0), PointCheck.Default);
        Assert.Equal(e, a.PointAtFraction(1), PointCheck.Default);
        Assert.Equal(new Vector2D(0, -1), a.DirectionAtFraction(0), VecCheck.Default);
        Assert.Equal(new Vector2D(-1, 0), a.DirectionAtFraction(1), VecCheck.Default);
    }
}