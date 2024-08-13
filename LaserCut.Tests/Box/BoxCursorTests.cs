using LaserCut.Box;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Box;

public class BoxCursorTests
{
    [Fact]
    public void CursorConstructsTransforms()
    {
        var box = SimpleBox();

        var e = box.Right.Top;

        // Edge to face transform
        Assert.Equal(e.EnvelopeCursor.Start, new Point2D(box.Length / 2, box.Height / 2), PointCheck.Default);
        Assert.Equal(e.EnvelopeCursor.End, new Point2D(-box.Length / 2, box.Height / 2), PointCheck.Default);

        // Edge to world transform
        Assert.Equal(box.Right.Envelope.C, e.EnvelopeCursor.StartWorld, PointCheck.Default);
        Assert.Equal(box.Right.Envelope.D, e.EnvelopeCursor.EndWorld, PointCheck.Default);
    }

    [Fact]
    public void CursorBottomCorrectZ()
    {
        var box = SimpleBox();

        Assert.Equal(-0.4, box.Bottom.Bottom.SharedCursor.StartWorld.Z, 1e-6);
        Assert.Equal(-0.4, box.Bottom.Right.SharedCursor.StartWorld.Z, 1e-6);
        Assert.Equal(-0.4, box.Bottom.Top.SharedCursor.StartWorld.Z, 1e-6);
        Assert.Equal(-0.4, box.Bottom.Left.SharedCursor.StartWorld.Z, 1e-6);

    }

    [Fact]
    public void SharedCursorWorldEnds()
    {
        var box = SimpleBox();

        var e0 = box.Front.Bottom;
        var e1 = box.Bottom.Top;

        // Check that the neighbors are correct
        Assert.Equal(e0, e1.Neighbor);
        Assert.Equal(e1, e0.Neighbor);

        // Check that the shared cursors have the same length
        Assert.Equal(e0.SharedCursor.Length, e1.SharedCursor.Length, 1e-6);

        // Check that the end of one shared cursor is the start of the other
        Assert.Equal(box.Thickness, e0.SharedCursor.StartWorld.DistanceTo(e1.SharedCursor.EndWorld), 1e-6);
        Assert.Equal(box.Thickness, e0.SharedCursor.EndWorld.DistanceTo(e1.SharedCursor.StartWorld), 1e-6);
    }

    [Fact]
    public void CursorSharedBottomConstructs()
    {
        var box = SimpleBox();

        var e0 = box.Front.Bottom;
        var e1 = box.Bottom.Top;

        // Check that the neighbors are correct
        Assert.Equal(e0, e1.Neighbor);
        Assert.Equal(e1, e0.Neighbor);

        // Check that the shared cursors have the same length
        Assert.Equal(e0.SharedCursor.Length, e1.SharedCursor.Length, 1e-6);

        // Check that the end of one shared cursor is the start of the other
        Assert.Equal(0, e0.SharedY(e1.SharedCursor.Length));
        Assert.Equal(e0.SharedCursor.Length, e1.SharedY(0));
    }

    [Fact]
    public void CheckAllSharedYEndpoints()
    {
        var box = SimpleBox();
        foreach (var edge in box.AllFaces.SelectMany(x => x.AllEdges))
        {
            var other = edge.Neighbor;
            Assert.Equal(0, edge.SharedY(other.SharedCursor.Length), 1e-12);
            Assert.Equal(edge.SharedCursor.Length, other.SharedY(0), 1e-12);
        }
    }

    private BoxModel SimpleBox()
    {
        var p = new BoxParams()
        {
            BaseInset = 0.1,
            Length = 3,
            Width = 2,
            Height = 1,
            Thickness = 0.05
        };

        return BoxModel.Create(p, true);
    }
}
