using System.Collections;

namespace LaserCut.Algorithms.Loop;

/// <summary>
/// A `Loop` is a double linked circular list holding onto items of type `T`.  It is not a lightweight data
/// structure, but has the advantage of being able to easily insert and remove items at any position in the list while
/// also being easy to traverse and conceptualize as a ring.
///
/// Rather than use references to the nodes themselves, internally this class uses integer ids and a dictionary to
/// store items, while hiding the node structure from the user.  This has two implications: first it provides a
/// simplified ownership model in which orphaned nodes cannot be held indefinitely by client code, and second it allows
/// for nodes to be found by id in constant time.
/// </summary>
public class Loop<T>
{
    /// <summary>
    /// This is the next unique id that will be assigned to a node.  It is incremented each time a new node is created.
    /// </summary>
    private int _nextUniqueId = 0;

    /// <summary>
    /// This is the id of the "first" node in the loop, which may or may not be a meaningful concept depending on the
    /// application. It starts as -1 when the loop is empty.
    /// </summary>
    private int _headId = -1;
    
    /// <summary>
    /// Nodes are stored in a dictionary with the integer id as the key. 
    /// </summary>
    protected readonly Dictionary<int, LoopNode> Nodes = new();
    
    /// <summary>
    /// Create a new loop with no items.
    /// </summary>
    public Loop() {}

    /// <summary>
    /// Create a new loop with the given items in the order they are provided. The last item in the list will link to
    /// the first item.
    /// </summary>
    /// <param name="items"></param>
    public Loop(IEnumerable<T> items)
    {
        var cursor = GetCursor();
        foreach (var item in items)
        {
            cursor.InsertAfter(item);
        }
    }
    
    /// <summary>
    /// The total number of items in the loop.
    /// </summary>
    public int Count => Nodes.Count;
    
    public bool IsEmpty => Count == 0;
    
    public T Head => Nodes[_headId].Item;
    
    public T Tail => Nodes[GetTailId()].Item;
     
    public int HeadId => _headId;
    
    public int TailId => GetTailId();
    
    /// <summary>
    /// This method is called whenever an item is inserted or removed from the loop.  It is intended to be overridden
    /// in the case that a subclass needs to perform some action when the loop changes.
    /// </summary>
    /// <param name="item"></param>
    public virtual void OnItemChanged(T item) {}
    
    /// <summary>
    /// Retrieves a cursor to edit the loop at the given id.  If no id is provided, the cursor will be created at the
    /// tail of the loop.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual ILoopCursor<T> GetCursor(int? id = null)
    {
        var nodeId = id ?? GetTailId();
        return new LoopCursor(this, nodeId);
    }
    
    /// <summary>
    /// Returns an enumerable with all items in the loop.  If a startId is provided, the enumeration will start at that
    /// item, otherwise it will start at the head of the loop.
    /// </summary>
    /// <param name="startId"></param>
    /// <returns></returns>
    public IEnumerable<LoopItem<T>> IterItems(int? startId = null)
    {
        return new NodeEnumerable(this, startId ?? _headId);
    }
    
    /// <summary>
    /// Returns an enumerable with all edges (pairs of consecutive items) in the loop.  If a startId is provided, the
    /// enumeration will start at that item, otherwise it will start at the head of the loop.
    /// </summary>
    /// <param name="startId"></param>
    /// <returns></returns>
    public IEnumerable<(LoopItem<T>, LoopItem<T>)> IterEdges(int? startId = null)
    {
        return new EdgeEnumerable(this, startId ?? _headId);
    }
    
    /// <summary>
    /// Creates a new loop with the same items in the same order.  This is not a deep copy, so if the items themselves
    /// are reference types, they will be shared between the two loops.
    /// </summary>
    /// <returns></returns>
    public virtual Loop<T> Copy()
    {
        var loop = new Loop<T>();
        foreach (var item in IterItems())
        {
            loop.GetCursor().InsertAfter(item.Item);
        }
        return loop;
    }
    
    /// <summary>
    /// Reverses the order of the items in the loop in place.  The head of the loop will become the tail, and the tail
    /// will become the head
    /// </summary>
    public void Reverse()
    {
        _headId = GetTailId();
        foreach (var node in Nodes)
        {
            var p = node.Value.PreviousId;
            var n = node.Value.NextId;
            node.Value.PreviousId = n;
            node.Value.NextId = p;
        }
    }
    
    public int NextId(int id)
    {
        return Nodes[id].NextId;
    }
    
    public int PreviousId(int id)
    {
        return Nodes[id].PreviousId;
    }
    
    public T Next(int id)
    {
        return Nodes[NextId(id)].Item;
    }
    
    public T Previous(int id)
    {
        return Nodes[PreviousId(id)].Item;
    }
    
    /// <summary>
    /// Finds the first item in the loop that satisfies the given predicate.  If a startId is provided, the search will
    /// begin at that id, otherwise the search will begin at the head. If no item is found, the method will return null.
    ///
    /// The search will be in the forward direction.
    /// </summary>
    /// <param name="predicate">A predicate which should return true on the object where the search should stop</param>
    /// <param name="startId">An optional id of the item to begin the search at</param>
    /// <returns>The integer id of the first item which matches the predicate, or null if no item was found</returns>
    public int? FirstId(Func<T, bool> predicate, int? startId = null)
    {
        if (Count == 0) return null;
        
        var firstId = startId ?? _headId;
        var currentId = firstId;
        do
        {
            if (predicate(Nodes[currentId].Item))
            {
                return currentId;
            }
            currentId = Nodes[currentId].NextId;
        } while (currentId != firstId);

        return null;
    }
    
    /// <summary>
    /// Returns an array of items which are between the startId and endId.  The startId is included but the
    /// endId is not.
    /// </summary>
    /// <param name="startId"></param>
    /// <param name="endId"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    protected virtual T[] SliceItems(int startId, int endId)
    {
        if (!Nodes.ContainsKey(startId) || !Nodes.ContainsKey(endId))
        {
            throw new KeyNotFoundException("Invalid start or end id");
        }
        
        var items = new List<T>();
        var cursor = GetCursor(startId);
        while (cursor.CurrentId != endId)
        {
            items.Add(cursor.Current);
            cursor.MoveForward();
        }
        return items.ToArray();
    }
    
    public virtual Loop<T> Slice(int startId, int endId)
    {
        return new Loop<T>(SliceItems(startId, endId));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool TryRemove(int id)
    {
        if (!Nodes.TryGetValue(id, out var node)) return false;
        var beforeNode = Nodes[node.PreviousId];
        var afterNode = Nodes[node.NextId];
        beforeNode.NextId = node.NextId;
        afterNode.PreviousId = node.PreviousId;
        Nodes.Remove(id);
        OnItemChanged(node.Item);
            
        if (Count == 0)
        {
            _headId = -1;
        }
        else if (id == _headId)
        {
            _headId = afterNode.Id;
        }

        return true;
    }
    
    /// <summary>
    /// This is an internal method to insert an item between two already-resolved nodes in the loop. It is used to
    /// consolidate insert operations into a single method. 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="previous"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    private int InsertBetween(T item, LoopNode? previous, LoopNode? next)
    {
        var id = _nextUniqueId++;

        if (Count > 0 && (previous == null || next == null))
        {
            throw new InvalidOperationException("Cannot insert between nodes without both nodes");
        }
        
        var node = (Count == 0) ? new LoopNode(id, item, id, id) : new LoopNode(id, item, previous!.Id, next!.Id);

        if (previous != null) previous.NextId = id;
        if (next != null) next.PreviousId = id;

        if (Count == 0)
        {
            _headId = id;
        }
        
        Nodes[id] = node;
        OnItemChanged(node.Item);
        return id;
    }
    
    private int InsertAfter(T item, int idOfPrevious)
    {
        if (Nodes.Count == 0)
        {
            return InsertBetween(item, null, null);
        }
        
        if (Nodes.TryGetValue(idOfPrevious, out var beforeNode))
        {
            return InsertBetween(item, beforeNode, Nodes[beforeNode.NextId]);
        }
        
        throw new KeyNotFoundException("Invalid id to insert after");
    }
    

    private int InsertBefore(T item, int idOfNext)
    {
        if (Nodes.Count == 0)
        {
            return InsertBetween(item, null, null);
        }

        // Get the node which will come after the one we're inserting
        if (Nodes.TryGetValue(idOfNext, out var afterNode))
        {
            return InsertBetween(item, Nodes[afterNode.PreviousId], afterNode);
        }

        throw new KeyNotFoundException("Invalid id to insert before");
    }

    protected int GetTailId()
    {
        if (Count == 0) return -1;
        return Nodes[_headId].PreviousId;
    }
    
    /// <summary>
    /// A `LoopNode` is the internal container for items in the loop.  It holds onto the item itself, as well as the
    /// ids of the next and previous nodes in the loop.  It is not intended to be used directly by client code, and
    /// so should not be exposed outside the class.
    /// </summary>
    protected class LoopNode
    {
        public int Id { get; }
        public int NextId { get; set; }
        public int PreviousId { get; set; }
        public T Item { get; set; }
        
        public LoopNode(int id, T item, int previousId, int nextId)
        {
            Id = id;
            Item = item;
            PreviousId = previousId;
            NextId = nextId;
        }

        public override string ToString()
        {
            return $"[Node {Id} | {Item} | N:{NextId} P:{PreviousId}]";
        }
        
        public void SwapNextAndPrevious()
        {
            (NextId, PreviousId) = (PreviousId, NextId);
        }
    }

    protected class NodeEnumerator : IEnumerator<LoopItem<T>>
    {
        private readonly Loop<T> _loop;
        private readonly int _startId;
        private int _currentId;
        
        public NodeEnumerator(Loop<T> loop, int startId)
        {
            _loop = loop;
            _startId = startId;
            _currentId = -1;
        }

        public bool MoveNext()
        {
            if (_currentId == -1)
            {
                _currentId = _startId;
            }
            else
            {
                var nextId = _loop.Nodes[_currentId].NextId;
                if (nextId == _startId)
                {
                    return false;
                }
                _currentId = nextId;
            }
            
            return true;
        }

        public void Reset() { _currentId = -1; }
        
        private LoopItem<T> ValueAt(int id)
        {
            return new LoopItem<T>(id, _loop.Nodes[id].Item);
        }

        public LoopItem<T> Current => ValueAt(_currentId);
        public LoopItem<T> PeekNext => ValueAt(_loop.Nodes[_currentId].NextId);

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }

    protected class NodeEnumerable(Loop<T> loop, int startId) : IEnumerable<LoopItem<T>>
    {
        private readonly NodeEnumerator _enumerator = new(loop, startId);

        public IEnumerator<LoopItem<T>> GetEnumerator() => _enumerator;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    protected class EdgeEnumerator : IEnumerator<(LoopItem<T>, LoopItem<T>)>
    {
        private readonly NodeEnumerator _enumerator;
        
        public EdgeEnumerator(Loop<T> loop, int startId)
        {
            _enumerator = new NodeEnumerator(loop, startId);
        }

        public bool MoveNext() => _enumerator.MoveNext();

        public void Reset()
        {
            _enumerator.Reset();
        }

        public (LoopItem<T>, LoopItem<T>) Current => (_enumerator.Current, _enumerator.PeekNext);

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
    
    protected class EdgeEnumerable(Loop<T> loop, int startId) : IEnumerable<(LoopItem<T>, LoopItem<T>)>
    {
        private readonly EdgeEnumerator _enumerator = new(loop, startId);

        public IEnumerator<(LoopItem<T>, LoopItem<T>)> GetEnumerator() => _enumerator;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    protected class LoopCursor : ILoopCursor<T>
    {
        protected readonly Loop<T> Loop;
        private int _nodeId;

        public LoopCursor(Loop<T> loop, int nodeId)
        {
            Loop = loop;
            _nodeId = nodeId;
        }

        public T Current => Loop.Nodes[_nodeId].Item;
        
        public int CurrentId => _nodeId;
        
        public int NextId => Loop.Nodes[_nodeId].NextId;
        
        public int PreviousId => Loop.Nodes[_nodeId].PreviousId;
        
        private LoopNode Node => Loop.Nodes[_nodeId];
        
        private LoopNode NextNode => Loop.Nodes[Node.NextId];
        
        private LoopNode PreviousNode => Loop.Nodes[Node.PreviousId];
        
        public T PeekNext()
        {
            return NextNode.Item;
        }
        
        public T PeekPrevious()
        {
            return PreviousNode.Item;
        }
        
        public int InsertBefore(T item, bool moveCursor = true)
        {
            var id = Loop.InsertBefore(item, _nodeId);
            if (moveCursor)
            {
                _nodeId = id;
            }
            
            OnItemAdded(item, id);
            
            return id;
        }
        
        public int InsertAfter(T item, bool moveCursor = true)
        {
            var id = Loop.InsertAfter(item, _nodeId);
            if (moveCursor)
            {
                _nodeId = id;
            }
            
            OnItemAdded(item, id);

            return id;
        }
        
        public bool SeekNext(Func<T, bool> predicate)
        {
            var nextId = Node.NextId;
            while (nextId != _nodeId)
            {
                if (predicate(Loop.Nodes[nextId].Item))
                {
                    _nodeId = nextId;
                    return true;
                }
                nextId = Loop.Nodes[nextId].NextId;
            }

            return false;
        }
        
        public bool SeekPrevious(Func<T, bool> predicate)
        {
            var previousId = Node.PreviousId;
            while (previousId != _nodeId)
            {
                if (predicate(Loop.Nodes[previousId].Item))
                {
                    _nodeId = previousId;
                    return true;
                }
                previousId = Loop.Nodes[previousId].PreviousId;
            }

            return false;
        }
        
        public void Remove(bool moveForward = true)
        {
            if (Loop.Count == 0) return;
            
            var nextId = moveForward ? Node.NextId : Node.PreviousId;
            Loop.TryRemove(_nodeId);
            _nodeId = nextId;
        }
        
        public void MoveForward()
        {
            if (Loop.Count > 0)
            {
                _nodeId = Node.NextId;
            }
        }
        
        public void MoveBackward()
        {
            if (Loop.Count > 0)
            {
                _nodeId = Node.PreviousId;
            }
        }
        
        public void MoveTo(int id)
        {
            if (Loop.Nodes.ContainsKey(id))
            {
                _nodeId = id;
            }
            else
            {
                throw new KeyNotFoundException($"No node with id {id}");
            }
        }
        
        public void MoveToHead()
        {
            if (Loop.Count > 0)
            {
                _nodeId = Loop._headId;
            }
        }
        
        public void MoveToTail()
        {
            if (Loop.Count > 0)
            {
                _nodeId = Loop.GetTailId();
            }
        }
        
        protected virtual void OnItemAdded(T item, int id) {}
        
        // what happens if we remove the node at the cursor position?
    }
    
}