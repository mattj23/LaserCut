using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Primitives;

public class ArcTests
{
    [Fact]
    public void CreateFromEndsCcw()
    {
        var s = new Point2D(0, 0);
        var e = new Point2D(1, 1);
        var c = new Point2D(0, 1);
        var a = Arc.FromEnds(s, e, c, false);
        
        Assert.Equal(1, a.Radius, 1e-10);
        Assert.Equal(Math.PI / 2, a.Theta, 1e-10);
        Assert.Equal(-Math.PI / 2.0, a.Theta0, 1e-10);
        
        Assert.Equal(s, a.AtFraction(0).Point, PointCheck.Default);
        Assert.Equal(e, a.AtFraction(1).Point, PointCheck.Default);
        Assert.Equal(new Vector2D(1, 0), a.AtFraction(0).Direction, VecCheck.Default);
        Assert.Equal(new Vector2D(0, 1), a.AtFraction(1).Direction, VecCheck.Default);
    }
    
    [Fact]
    public void CreateFromEndsCw()
    {
        var e = new Point2D(0, 0);
        var s = new Point2D(1, 1);
        var c = new Point2D(0, 1);
        var a = Arc.FromEnds(s, e, c, true);
        
        Assert.Equal(1, a.Radius, 1e-10);
        Assert.Equal(-Math.PI / 2, a.Theta, 1e-10);
        Assert.Equal(0, a.Theta0, 1e-10);
        
        Assert.Equal(s, a.AtFraction(0).Point, PointCheck.Default);
        Assert.Equal(e, a.AtFraction(1).Point, PointCheck.Default);
        Assert.Equal(new Vector2D(0, -1), a.AtFraction(0).Direction, VecCheck.Default);
        Assert.Equal(new Vector2D(-1, 0), a.AtFraction(1).Direction, VecCheck.Default);
    }

    [Fact]
    public void AreaTest()
    {
        var a = new Arc(10, 10, 5, 0, 2.0 * Math.PI);
        var area = a.CrossProductWedge / 2.0;
        Assert.Equal(Math.PI * a.Radius * a.Radius, area, 1e-4);
    }

    [Fact]
    public void StressTestIsThetaOnArc()
    {
        var r = new RandomValues();
        var n = 10;
        
        for (int i = 0; i < 10000; i++)
        {
            var t0 = r.Double(-Math.PI, Math.PI);
            var t = r.Double(-2.0 * Math.PI, 2.0 * Math.PI);
            var a = new Arc(new Point2D(0, 0), 1, t0, t);
            var step = t / (n - 1);
            
            // Check the angles which should be on the arc
            for (int k = 0; k < n; k++)
            {
                var test = t0 + step * k;
                Assert.True(a.IsThetaOnArc(test));
            }
            
            // Check the angles which are past the arc
            var tc = Angles.Compliment(t);
            if (Math.Abs(tc) > 1e-2)
            {
                var tc0 = t0 + (double.IsPositive(t) ? -1e-3 : 1e-3);
                var tc1 = tc + (double.IsPositive(t) ? 2e-3 : -2e-3);
                var stepc = tc1 / (n - 1);
                for (int k = 0; k < n; k++)
                {
                    var test = tc0 + stepc * k;
                    Assert.False(a.IsThetaOnArc(test));
                }
            }
            Assert.True(a.IsThetaOnArc(t0 + tc));
        }
    }

    [Fact]
    public void StressTestBounds()
    {
        var r = new RandomValues();
        
        for (int i = 0; i < 10000; i++)
        {
            var t0 = r.Double(-Math.PI, Math.PI);
            var t = r.Double(-2.0 * Math.PI, 2.0 * Math.PI);
            var c = r.Point(10);
            var a = new Arc(c, r.Double(0.1, 5.0), t0, t);

            var n = (int)Math.Ceiling(a.Length / 0.01);
            n = int.Max(100, n);
            var points = new List<Point2D>();
            for (int k = 0; k < n; k++)
            {
                var x = a.Length * k / (n - 1);
                points.Add(a.AtLength(x).Point);
            }

            var expected = Aabb2.FromPoints(points);
            Assert.Equal(expected.MinX, a.Bounds.MinX, 1e-4);
            Assert.Equal(expected.MinY, a.Bounds.MinY, 1e-4);
            Assert.Equal(expected.MaxX, a.Bounds.MaxX, 1e-4);
            Assert.Equal(expected.MaxY, a.Bounds.MaxY, 1e-4);
        }
    }

    [Fact]
    public void StressTestLengthFractionTheta()
    {
        var r = new RandomValues();
        for (int i = 0; i < 1000; i++)
        {
            var t0 = r.Double(-Math.PI, Math.PI);
            var t = r.Double(-2.0 * Math.PI, 2.0 * Math.PI);
            var c = r.Point(10);
            var a = new Arc(c, r.Double(0.1, 5.0), t0, t);

            for (int k = 0; k < 1000; k++)
            {
                var f = r.Double(0.0, 1.0);

                // Round trip fraction/theta
                var theta = a.FractionToTheta(f);
                Assert.Equal(f, a.ThetaToFraction(theta), 1e-6);
                
                // Round trip fraction/length
                var len = a.FractionToLength(f);
                Assert.Equal(f, a.LengthToFraction(len), 1e-6);
            }
        }
    }

    [Fact]
    public void DegenerateIntersection0()
    {
        // This test is based off of an observed degenerate intersection case. When constructed this way, one of the 
        // returned positions is the start of the arc, but at 2 * Math.PI, which is not a valid position on the arc.
        var a = new Arc(1, 0, 1, 0, Math.PI);
        var s = new Segment(new Point2D(0, 1), a.Start, 0);
        var p = s.Intersections(a);
        
        Assert.All(p, x => Assert.True(x.L >= 0 && x.L <= a.Length));
    }
    
    [Fact]
    public void DegenerateIntersection1()
    {
        // This test is based off of an observed degenerate intersection case. When constructed this way, there is no
        // valid intersection point, but there should be one at the segment's 0 position.
        var a = new Arc(1, 0, 1, 0, Math.PI);
        var s = new Segment(1, 1, 1.5, 0.5, 1);
        var i = s.Intersections(a);
        Assert.Single(i);
        
        Assert.Equal(1, i[0].Surface.Point.X, 1e-10);
        Assert.Equal(1, i[0].Surface.Point.Y, 1e-10);
    }

    [Fact]
    public void SplitAfterNull()
    {
        var arc = new Arc(1, 0, 1, 0, Math.PI, 0);
        var after = arc.SplitAfter(Math.PI);
        Assert.Null(after);
    }
    
    [Fact]
    public void SplitBeforeNull()
    {
        var arc = new Arc(1, 0, 1, 0, Math.PI, 0);
        var before = arc.SplitBefore(0);
        Assert.Null(before);
    }

    [Fact]
    public void SplitBeforeCcw()
    {
        var arc = new Arc(1, 0, 1, 0, Math.PI, 0);
        var before = arc.SplitBefore(Math.PI / 4);
        
        Assert.NotNull(before);
        if (before is Arc b)
        {
            Assert.Equal(arc.IsCcW, b.IsCcW);
            Assert.Equal(arc.Center, b.Center, PointCheck.Default);
            Assert.Equal(arc.Radius, b.Radius, 1e-10);
            Assert.Equal(arc.Start, b.Start, PointCheck.Default);
            Assert.Equal(MakePoint.AtDirection(1, 0, 1, 1, 1), b.End, PointCheck.Default);
        }
        else
        {
            Assert.Fail("SplitBefore did not return an arc");
        }
    }
    
    [Fact]
    public void SplitBeforeCw()
    {
        var arc = new Arc(1, 0, 1, 0, -Math.PI, 0);
        var before = arc.SplitBefore(Math.PI / 4);
        
        Assert.NotNull(before);
        if (before is Arc b)
        {
            Assert.Equal(arc.IsCcW, b.IsCcW);
            Assert.Equal(arc.Center, b.Center, PointCheck.Default);
            Assert.Equal(arc.Radius, b.Radius, 1e-10);
            Assert.Equal(arc.Start, b.Start, PointCheck.Default);
            Assert.Equal(MakePoint.AtDirection(1, 0, 1, -1, 1), b.End, PointCheck.Default);
        }
        else
        {
            Assert.Fail("SplitBefore did not return an arc");
        }
    }
    
    [Fact]
    public void SplitAfterCcw()
    {
        var arc = new Arc(1, 0, 1, 0, Math.PI, 0);
        var before = arc.SplitAfter(Math.PI / 4);
        
        Assert.NotNull(before);
        if (before is Arc b)
        {
            Assert.Equal(arc.IsCcW, b.IsCcW);
            Assert.Equal(arc.Center, b.Center, PointCheck.Default);
            Assert.Equal(arc.Radius, b.Radius, 1e-10);
            Assert.Equal(arc.End, b.End, PointCheck.Default);
            Assert.Equal(MakePoint.AtDirection(1, 0, 1, 1, 1), b.Start, PointCheck.Default);
        }
        else
        {
            Assert.Fail("SplitBefore did not return an arc");
        }
    }
    
    [Fact]
    public void SplitAfterCw()
    {
        var arc = new Arc(1, 0, 1, 0, -Math.PI, 0);
        var before = arc.SplitAfter(Math.PI / 4);
        
        Assert.NotNull(before);
        if (before is Arc b)
        {
            Assert.Equal(arc.IsCcW, b.IsCcW);
            Assert.Equal(arc.Center, b.Center, PointCheck.Default);
            Assert.Equal(arc.Radius, b.Radius, 1e-10);
            Assert.Equal(arc.End, b.End, PointCheck.Default);
            Assert.Equal(MakePoint.AtDirection(1, 0, 1, -1, 1), b.Start, PointCheck.Default);
        }
        else
        {
            Assert.Fail("SplitBefore did not return an arc");
        }
    }
}