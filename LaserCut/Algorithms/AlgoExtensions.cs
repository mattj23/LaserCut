using LaserCut.Mesh;

namespace LaserCut.Algorithms;

public static class AlgoExtensions
{
    /// <summary>
    /// Performs a pop operation on a HashSet, removing and returning the first item in the set. If the set is empty,
    /// an error will be thrown.
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Pop<T>(this HashSet<T> items)
    {
        // Get, remove, and return the first item
        var item = items.First();
        items.Remove(item);
        return item;
    }

    public static Edge[] Edges(this Face face)
    {
        return new[] { face.EdgeA, face.EdgeB, face.EdgeC };
    }
    
    public static void TransferTo<T>(this Queue<T> source, Queue<T> target)
    {
        while (source.Count > 0)
            target.Enqueue(source.Dequeue());
    }
    
}