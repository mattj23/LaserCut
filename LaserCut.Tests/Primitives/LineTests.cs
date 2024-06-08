using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Primitives;

public class LineTests
{
    [Theory]
    [InlineData(0, 1, 0, 0, 1, 0, 0, -1)]
    [InlineData(0, -1, 0, 0, 1, 0, 0, 1)]
    [InlineData(1, 1, 0, 0, 1, -1, -1, -1)]
    [InlineData(-1, -1, 0, 0, 1, -1, 1, 1)]
    public void PointMirroring(double px, double py, double sx, double sy, double dx, double dy, double ex, double ey)
    {
        var line = new Line2(new Point2D(sx, sy), new Vector2D(dx, dy));
        var result = line.Mirror(new Point2D(px, py));
        
        var expected = new Point2D(ex, ey);
        
        Assert.Equal(expected, result, PointCheck.Default);
    }
    
    
}