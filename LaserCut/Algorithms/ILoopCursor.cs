namespace LaserCut.Algorithms;

public interface ILoopCursor<T>
{
    T Current { get; }
    int CurrentId { get; }
    
    T PeekNext();
    
    T PeekPrevious();
    
    int InsertBefore(T item, bool moveCursor = true);
    
    int InsertAfter(T item, bool moveCursor = true);
    
}