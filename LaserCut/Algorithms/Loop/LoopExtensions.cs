namespace LaserCut.Algorithms.Loop;

public static class LoopExtensions
{
    public static T[] ToItemArray<T>(this Loop<T> loop, int? startId = null)
    {
        return loop.IterItems(startId).Select(x => x.Item).ToArray();
    }
    
    public static (T, T)[] ToEdgeArray<T>(this Loop<T> loop, int? startId = null)
    {
        return loop.IterEdges(startId).Select(x => (x.Item1.Item, x.Item2.Item)).ToArray();
    }
    
}