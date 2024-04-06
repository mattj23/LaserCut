using LaserCut.Algorithms.Loop;

namespace LaserCut.Tests;

public class LoopTests
{
    [Fact]
    public void LoopStartsEmpty()
    {
        var loop = new StringLoop();
        Assert.Equal(0, loop.Count);
    }

    [Fact]
    public void LoopStartsWithItems()
    {
        var expected = new[] { "a", "b", "c" };
        var loop = new StringLoop(expected);

        var values = loop.ToItemArray();

        Assert.Equal(expected, values);
    }

    [Fact]
    public void AddItemsAfter()
    {
        var loop = new StringLoop();
        var cursor = loop.GetCursor();

        cursor.InsertAfter("a");
        cursor.InsertAfter("b");
        cursor.InsertAfter("c");
        var values = loop.ToItemArray();

        var expected = new[] { "a", "b", "c" };
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void FindIdOfItem()
    {
        var loop = new StringLoop();
        var cursor = loop.GetCursor();

        cursor.InsertAfter("a");
        var x = cursor.InsertAfter("b");
        cursor.InsertAfter("c");
        cursor.InsertAfter("d");
        
        Assert.Equal(x, loop.FirstId(s => s == "b"));
    }
    
    [Fact]
    public void FindIdOfItemNotFound()
    {
        var loop = new StringLoop();
        var cursor = loop.GetCursor();

        cursor.InsertAfter("a");
        cursor.InsertAfter("b");
        cursor.InsertAfter("c");
        cursor.InsertAfter("d");
        
        Assert.Null(loop.FirstId(s => s == "e"));
    }

    [Fact]
    public void AddItemsBefore()
    {
        var loop = new StringLoop();
        var cursor = loop.GetCursor();

        cursor.InsertAfter("a");
        cursor.InsertAfter("b");
        cursor.InsertBefore("c");
        var values = loop.ToItemArray();

        var expected = new[] { "a", "c", "b" };
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CursorAtHandle()
    {
        var loop = new StringLoop();
        var cursor = loop.GetCursor();

        cursor.InsertAfter("a");
        var center = cursor.InsertAfter("b");
        cursor.InsertAfter("c");

        var c2 = loop.GetCursor(center);
        c2.InsertBefore("d");
        var values = loop.ToItemArray();

        var expected = new[] { "a", "d", "b", "c" };
        Assert.Equal(expected, values);
    }

    [Fact]
    public void CheckFixtureDefault()
    {
        var loop = StringLoop.Abcde();
        var expected = new[] { "a", "b", "c", "d", "e" };
        var values = loop.ToItemArray();
        Assert.Equal(expected, values);
    }

    [Fact]
    public void SeekNext()
    {
        var loop = StringLoop.Abcde();
        var cursor = loop.GetCursor();
        Assert.True(cursor.SeekNext(x => x == "c" || x == "d"));
        Assert.Equal("c", cursor.Current);
    }

    [Fact]
    public void SeekPrevious()
    {
        var loop = StringLoop.Abcde();
        var cursor = loop.GetCursor();
        Assert.True(cursor.SeekPrevious(x => x == "c" || x == "d"));
        Assert.Equal("d", cursor.Current);
    }

    [Fact]
    public void SeekMissing()
    {
        var loop = StringLoop.Abcde();
        var cursor = loop.GetCursor();
        Assert.False(cursor.SeekNext(x => x == "f"));
        Assert.Equal("e", cursor.Current);
    }

    [Fact]
    public void RemoveItemMoveForward()
    {
        var loop = StringLoop.Abcde();
        var cursor = loop.GetCursor();
        cursor.SeekNext(x => x == "c");
        cursor.Remove();
        var values = loop.ToItemArray();
        var expected = new[] { "a", "b", "d", "e" };
        Assert.Equal(expected, values);
        Assert.Equal("d", cursor.Current);
    }

    [Fact]
    public void RemoveItemMoveBackwards()
    {
        var loop = StringLoop.Abcde();
        var cursor = loop.GetCursor();
        cursor.SeekNext(x => x == "c");
        cursor.Remove(false);
        var values = loop.ToItemArray();
        var expected = new[] { "a", "b", "d", "e" };
        Assert.Equal(expected, values);
        Assert.Equal("b", cursor.Current);
    }

    [Fact]
    public void RemoveItemAtHead()
    {
        var loop = StringLoop.Abcde();
        var cursor = loop.GetCursor();
        cursor.MoveToHead();
        cursor.Remove();
        var values = loop.ToItemArray();
        var expected = new[] { "b", "c", "d", "e" };
        Assert.Equal(expected, values);
        Assert.Equal("b", cursor.Current);
    }

    [Fact]
    public void IterateFromMiddle()
    {
        var loop = StringLoop.Abcde();
        var xid = loop.FirstId(s => s == "c");
        var values = loop.ToItemArray(xid);
        var expected = new[] { "c", "d", "e", "a", "b" };
        Assert.Equal(expected, values);
    }

    [Fact]
    public void IterateEdges()
    {
        var loop = StringLoop.Abcde();
        var values = loop.ToEdgeArray();
        var expected = new[] { ("a", "b"), ("b", "c"), ("c", "d"), ("d", "e"), ("e", "a") };
        Assert.Equal(expected, values);
    }
    
    [Fact]
    public void ReverseDirection()
    {
        var loop = StringLoop.Abcde();
        loop.Reverse();
        var values = loop.ToItemArray();
        var expected = new[] { "e", "d", "c", "b", "a" };
        Assert.Equal(expected, values);
    }

    private class StringLoop : Loop<string>
    {
        public StringLoop()
        {
        }

        public StringLoop(IEnumerable<string> items) : base(items)
        {
        }

        public static StringLoop Abcde()
        {
            return new StringLoop(new[] { "a", "b", "c", "d", "e" });
        }
    }
}