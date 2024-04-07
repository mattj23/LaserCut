using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class LineTests
{
    [Fact]
    public void CollinearIsTrue()
    {
        var l0 = new Line2(Point2D.Origin, Vector2D.XAxis);
        var l1 = new Line2(new Point2D(1.0, 0.0), Vector2D.XAxis);
        
        Assert.True(l0.IsCollinear(l1));
    }
    
    [Fact]
    public void CollinearIsFalse()
    {
        var l0 = new Line2(Point2D.Origin, Vector2D.XAxis);
        var l1 = new Line2(new Point2D(1.0, 0.5), Vector2D.XAxis);
        
        Assert.False(l0.IsCollinear(l1));
    }
    
    [Fact]
    public void SignedDistancePositive()
    {
        var l = new Line2(Point2D.Origin, Vector2D.XAxis);
        var p = new Point2D(1.0, -1.0);
        
        Assert.Equal(1.0, l.SignedDistanceTo(p), 1e-10);
    }
    
}