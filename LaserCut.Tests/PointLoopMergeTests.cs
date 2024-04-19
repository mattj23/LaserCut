using LaserCut.Geometry;

namespace LaserCut.Tests;

public class PointLoopMergeTests
{
    [Fact]
    public void MergeTwoRectangles()
    {
        var loop0 = Rect(-2, -2, 4, 4);
        var loop1 = Rect(-3, -1, 6, 2);

        // throw new NotImplementedException();
    }


    private PointLoop Rect(double x0, double y0, double width, double height)
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(x0, y0);
        cursor.InsertRelX(width);
        cursor.InsertRelY(height);
        cursor.InsertRelX(-width);
        return loop;
    }
}