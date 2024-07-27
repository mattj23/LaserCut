using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Primitives;

public class AabbTests
{
    [Fact]
    public void StressTestFarthest()
    {
        var r = new RandomValues();
        var larger = new Aabb2(-20, -20, 20, 20);
        var smaller = new Aabb2(-10, -10, 10, 10);

        for (var i = 0; i < 10000; i++)
        {
            var p = r.Point(larger);
            var b = Aabb2.FromPoints([r.Point(smaller), r.Point(smaller)]);
            
            var distances = new []{ 
                p.DistanceTo(new Point2D(b.MinX, b.MinY)), p.DistanceTo(new Point2D(b.MinX, b.MaxY)),
                p.DistanceTo(new Point2D(b.MaxX, b.MinY)), p.DistanceTo(new Point2D(b.MaxX, b.MaxY)) };
            var expected = distances.Max();

            Assert.Equal(expected, b.FarthestDistance(p), 1e-10);
        }

    }

    [Fact]
    public void StressTestClosestOther()
    {
        var r = new RandomValues();
        var envelope = new Aabb2(-50, -50, 50, 50);

        for (var i = 0; i < 10000; i++)
        {
            var b0 = r.Box(envelope, 10, 10);
            var b1 = r.Box(envelope, 10, 10);

            if (b0.Intersects(b1))
            {
                Assert.Equal(0, b0.ClosestDistance(b1), 1e-10);
            }
            else
            {
                var v0 = b0.Corners().Select(x => b1.ClosestDistance(x)).ToList();
                var v1 = b1.Corners().Select(x => b0.ClosestDistance(x)).ToList();
                var expected = v0.Concat(v1).Min();
                
                Assert.Equal(expected, b0.ClosestDistance(b1), 1e-10);
            }
        }
    }
    
    [Fact]
    public void StressTestFarthestOther()
    {
        var r = new RandomValues();
        var envelope = new Aabb2(-50, -50, 50, 50);

        for (var i = 0; i < 10000; i++)
        {
            var b0 = r.Box(envelope, 10, 10);
            var b1 = r.Box(envelope, 10, 10);

            var v0 = b0.Corners().Select(x => b1.FarthestDistance(x)).ToList();
            var v1 = b1.Corners().Select(x => b0.FarthestDistance(x)).ToList();
            var expected = v0.Concat(v1).Max();
            
            Assert.Equal(expected, b0.FarthestDistance(b1), 1e-10);
        }
    }
}