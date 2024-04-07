using System.Security.Cryptography.X509Certificates;
using LaserCut.Geometry;
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

    [Theory]
    [InlineData(11.0, 0.7, -4.2, -2.7, -0.1, -4.7, 1.8, 0.0, 2.0, 1.5)]
    [InlineData(6.2, 3.0, 1.3, -1.9, -1.3, 8.3, 3.1, -1.7, -1.0, 2.0)]
    [InlineData(3.6, -3.3, -4.1, 3.2, 9.9, 0.9, 3.0, 2.0, 0.0, -2.1)]
    [InlineData(7.4, -10.1, -2.4, 3.2, -7.0, -7.7, 4.2, 2.8, 2.5, 2.0)]
    [InlineData(-1.6, 4.3, -0.0, -2.3, -9.8, 7.1, -4.1, 3.7, 2.0, -2.0)]
    [InlineData(1.7, -0.5, -0.5, -3.3, 3.9, -2.3, 0.5, -1.5, -1.0, -3.4)]
    [InlineData(4.7, 0.4, -0.6, 0.9, 4.7, 1.3, -3.0, -0.0, 1.0, 0.2)]
    [InlineData(-4.8, -2.0, -1.1, -1.3, -0.8, -21.0, -0.4, 4.8, -2.0, 4.5)]
    [InlineData(9.1, 5.7, 4.9, 1.6, -9.1, -15.5, 2.1, 4.5, -2.0, 4.0)]
    [InlineData(2.8, 15.7, 0.7, 3.0, -3.9, -7.1, 1.3, 3.6, -4.0, 3.0)]
    [InlineData(-5.0, 6.4, -2.6, 0.6, 5.3, 2.3, -2.4, 4.0, -3.5, 0.5)]
    [InlineData(0.4, -1.9, 2.5, 1.5, 7.6, -19.0, -3.2, 4.2, -1.6, 3.5)]
    [InlineData(10.6, 5.9, 2.0, 0.7, -2.0, 7.1, 2.6, -4.7, -5.0, 1.0)]
    [InlineData(7.3, -0.1, -3.5, 0.9, 14.4, 14.6, -4.4, -3.0, 3.0, 4.0)]
    [InlineData(-5.3, 11.5, -0.8, 4.6, -15.7, 10.0, -3.1, 2.5, -2.5, -4.0)]
    [InlineData(4.8, 1.9, 1.0, 1.0, 4.8, 1.9, -1.1, 1.0, 0.0, 0.0)]
    public void IntersectionParameters(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double t0, double t1)
    {
        var v0 = new Vector2D(x1, y1);
        var v1 = new Vector2D(x3, y3);
        
        var l0 = new Line2(new Point2D(x0, y0), v0);
        var l1 = new Line2(new Point2D(x2, y2), v1);
        
        var (t0Actual, t1Actual) = l0.IntersectionParams(l1);
        
        Assert.Equal(t0 * v0.Length, t0Actual, 1e-10);
        Assert.Equal(t1 * v1.Length, t1Actual, 1e-10);
    }

    [Theory]
    [InlineData(-5.0, 2.8, 2.2, 1.8, -4.2, -0.2, 6.6, 5.4)]
    [InlineData(3.3, 2.5, 4.0, 1.0, 3.2, 0.7, -20.0, -5.0)]
    [InlineData(4.2, -2.3, -0.6, 1.4, -1.0, 0.5, -2.4, 5.6)]
    [InlineData(-1.1, 2.0, 5.0, 4.0, 4.9, -2.8, 19.5, 15.6)]
    [InlineData(2.4, -3.0, -1.8, -2.6, 0.1, 0.7, 7.2, 10.4)]
    [InlineData(1.2, 2.1, 4.3, -1.0, -1.4, 3.9, 8.6, -2.0)]
    [InlineData(-4.8, -2.0, -0.1, -0.5, 3.0, -3.6, 0.4, 2.0)]
    [InlineData(-4.4, -0.4, 3.1, 1.1, 3.1, 4.9, -3.1, -1.1)]
    [InlineData(-1.0, -0.1, 0.1, 1.0, 1.2, -2.8, 0.3, 3.0)]
    [InlineData(4.7, -3.7, 4.0, -2.5, 2.5, -0.4, -11.2, 7.0)]
    [InlineData(-1.2, 0.4, 3.9, 0.9, 2.7, -4.6, -11.7, -2.7)]
    [InlineData(-4.8, 4.1, 4.4, -3.0, 4.3, 3.2, -6.6, 4.5)]
    [InlineData(-3.7, 4.7, -1.4, 2.6, 1.0, -4.5, -2.1, 3.9)]
    [InlineData(-0.1, 2.6, 1.4, -0.3, -0.6, -1.5, -4.2, 0.9)]
    [InlineData(1.8, -2.0, 4.5, -2.0, 0.5, -4.7, 18.0, -8.0)]
    public void IntersectionParallelFail(double x0, double y0, double x1, double y1, double x2, double y2, double x3,
        double y3)
    {
        var l0 = new Line2(new Point2D(x0, y0), new Vector2D(x1, y1));
        var l1 = new Line2(new Point2D(x2, y2), new Vector2D(x3, y3));
        var result = l0.IntersectionParams(l1);
        Assert.True(double.IsNaN(result.Item1));
        Assert.True(double.IsNaN(result.Item2));
    }
}