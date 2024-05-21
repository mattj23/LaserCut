using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Primitives;

public class CircleTests
{
    [Fact]
    public void CircleFromPoints()
    {
        var p0 = new Point2D(0, 0);
        var p1 = new Point2D(2, 0);
        var p2 = new Point2D(1, 1);
        var circle = new Circle2(p0, p1, p2);

        Assert.Equal(new Point2D(1, 0), circle.Center, PointCheck.Default);
        Assert.Equal(1, circle.Radius, 1e-10);
    }

    [Fact]
    public void ThetaOfXPos()
    {
        var c = new Circle2(0, 0, 1);
        var p = new Point2D(2, 0);
        Assert.Equal(0, c.ThetaOf(p), 1e-10);
    }
    
    [Fact]
    public void ThetaOfYPos()
    {
        var c = new Circle2(0, 0, 1);
        var p = new Point2D(0, 2);
        Assert.Equal(Math.PI / 2, c.ThetaOf(p), 1e-10);
    }
    
    [Fact]
    public void ThetaOfXNeg()
    {
        var c = new Circle2(0, 0, 1);
        var p = new Point2D(-2, 0);
        Assert.Equal(Math.PI, c.ThetaOf(p), 1e-10);
    }
    
    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(Math.PI / 2, 0, 1)]
    [InlineData(Math.PI, -1, 0)]
    [InlineData(3 * Math.PI / 2, 0, -1)]
    public void PointAt(double theta, double x, double y)
    {
        var c = new Circle2(0, 0, 1);
        var p = c.PointAt(theta);
        Assert.Equal(new Point2D(x, y), p, PointCheck.Default);
    }

    [Fact]
    public void LineIntersectionDisjoint()
    {
        var c = new Circle2(0, 0, 1);
        var line = new Line2(new Point2D(2, 0), new Vector2D(0, 1));
        var i = c.Intersections(line);
        Assert.Empty(i);
    }
    
    [Fact]
    public void LineIntersectionTangent()
    {
        var c = new Circle2(0, 0, 1);
        var line = new Line2(new Point2D(1, 0), new Vector2D(0, -1));
        var i = c.Intersections(line);
        Assert.Single(i);
        Assert.Equal(new Point2D(1, 0), i[0], PointCheck.Default);
    }
    
    [Fact]
    public void LineIntersectionTwoPoints()
    {
        var c = new Circle2(0, 0, 1);
        var line = new Line2(new Point2D(0, -10), new Vector2D(0, 1));
        var i = c.Intersections(line).OrderBy(p => p.Y).ToArray();
        Assert.Equal(2, i.Length);
        Assert.Equal(new Point2D(0, -1), i[0], PointCheck.Default);
        Assert.Equal(new Point2D(0, 1), i[1], PointCheck.Default);
    }
    
    [Fact]
    public void CircleIntersectionDisjoint()
    {
        var c0 = new Circle2(0, 0, 1);
        var c1 = new Circle2(3, 0, 1);
        var i = c0.Intersections(c1);
        Assert.Empty(i);
    }
    
    [Fact]
    public void CircleIntersectionTangent()
    {
        var c0 = new Circle2(0, 0, 1);
        var c1 = new Circle2(2, 0, 1);
        var i = c0.Intersections(c1);
        Assert.Single(i);
        Assert.Equal(new Point2D(1, 0), i[0], PointCheck.Default);
    }
    
    [Fact]
    public void CircleIntersectionTwoPoints()
    {
        var c0 = new Circle2(0, 0, 1);
        var c1 = new Circle2(1, 0, 1);
        var i = c0.Intersections(c1).OrderBy(p => p.Y).ToArray();
        var y = Math.Sqrt(1 - 0.5 * 0.5);
        Assert.Equal(2, i.Length);
        Assert.Equal(new Point2D(0.5, -y), i[0], PointCheck.Default);
        Assert.Equal(new Point2D(0.5, y), i[1], PointCheck.Default);
    }

    [Fact]
    public void TangentsToPoint()
    {
        var c = new Circle2(0, 0, 1);
        var p = new Point2D(1, 1);
        var t = c.TangentsTo(p).OrderBy(i => i.X).ToArray();
        Assert.Equal(2, t.Length);
        
        Assert.Equal(new Point2D(0, 1), t[0], PointCheck.Default);
        Assert.Equal(new Point2D(1, 0), t[1], PointCheck.Default);
    }
}
