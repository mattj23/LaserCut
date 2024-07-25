namespace LaserCut.Algorithms.Loop;

public interface ILoopCursor<T>
{
    T Current { get; }
    int CurrentId { get; }
    
    int NextId { get; }

    int GetIdFwd(int n);
    
    int PreviousId { get; }
    
    int GetIdBack(int n);
    
    T PeekNext();
    
    T PeekPrevious();
    
    int InsertBefore(T item, bool moveCursor = true);
    
    int InsertAfter(T item, bool moveCursor = true);

    bool SeekNext(Func<T, bool> predicate);
    
    bool SeekPrevious(Func<T, bool> predicate);
    
    void Remove(bool moveForward = true);

    void MoveForward();
    
    void MoveBackward();
    
    void MoveTo(int id);
    
    void MoveToHead();
    
    void MoveToTail();
}