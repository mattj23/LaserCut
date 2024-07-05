using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using LaserCut.Tests.Helpers;

namespace LaserCut.Tests.Contours;

public class RedundancyContourTests : ShapeOpTestBase
{
    [Fact]
    public void RemoveCollinearPoints()
    {
        var c = Loop((0, 0), (1, 0), (1.5, 0), (2, 0), (2, 1), (2, 2), (0, 2));
        var expected = ExpectedPoints((0, 0), (2, 0), (2, 2), (0, 2));

        c.RemoveAdjacentRedundancies();
        
        AssertLoop(expected, c);
    }

    [Fact]
    public void RemoveAdjacentArcs()
    {
        var c = new BoundaryLoop();
        var cursor = c.GetCursor();
        cursor.ArcAbs(1, 0, 0, 0, false);
        cursor.ArcAbs(0, 1, 0, 0, false);
        cursor.ArcAbs(-1, 0, 0, 0, false);
        cursor.ArcAbs(0, -0, 0, 0, false);
        
        c.RemoveAdjacentRedundancies();

        Assert.Single(c.Elements);
        Assert.IsType<Arc>(c.Elements[0]);
        Assert.Equal(2.0 * Math.PI, (c.Elements[0] as Arc)!.Theta, 1e-6);
    }
    
}