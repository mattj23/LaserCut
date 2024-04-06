using System.Collections;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

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
public abstract class Loop<T>
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
    private readonly Dictionary<int, LoopNode> _nodes = new();
    
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
    public int Count => _nodes.Count;
    
    public T Head => _nodes[_headId].Item;
    
    public T Tail => _nodes[GetTailId()].Item;
     
    
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
    public ILoopCursor<T> GetCursor(int? id = null)
    {
        var nodeId = id ?? GetTailId();
        return new LoopCursor(this, nodeId);
    }
    
    public IEnumerable<T> GetItems(int? startId = null)
    {
        return new NodeEnumerable(this, startId ?? _headId);
    }
    
    public int? FindId(Func<T, bool> predicate)
    {
        if (Count == 0) return null;
        
        var startId = _headId;
        var currentId = startId;
        do
        {
            if (predicate(_nodes[currentId].Item))
            {
                return currentId;
            }
            currentId = _nodes[currentId].NextId;
        } while (currentId != startId);

        return null;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool TryRemove(int id)
    {
        if (!_nodes.TryGetValue(id, out var node)) return false;
        var beforeNode = _nodes[node.PreviousId];
        var afterNode = _nodes[node.NextId];
        beforeNode.NextId = node.NextId;
        afterNode.PreviousId = node.PreviousId;
        _nodes.Remove(id);
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
        
        _nodes[id] = node;
        OnItemChanged(node.Item);
        return id;
    }
    
    private int InsertAfter(T item, int idOfPrevious)
    {
        if (_nodes.Count == 0)
        {
            return InsertBetween(item, null, null);
        }
        
        if (_nodes.TryGetValue(idOfPrevious, out var beforeNode))
        {
            return InsertBetween(item, beforeNode, _nodes[beforeNode.NextId]);
        }
        
        throw new KeyNotFoundException("Invalid id to insert after");
    }
    

    private int InsertBefore(T item, int idOfNext)
    {
        if (_nodes.Count == 0)
        {
            return InsertBetween(item, null, null);
        }

        // Get the node which will come after the one we're inserting
        if (_nodes.TryGetValue(idOfNext, out var afterNode))
        {
            return InsertBetween(item, _nodes[afterNode.PreviousId], afterNode);
        }

        throw new KeyNotFoundException("Invalid id to insert before");
    }

    private int GetTailId()
    {
        if (Count == 0) return -1;
        return _nodes[_headId].PreviousId;
    }
    
    
    private class LoopNode
    {
        public int Id { get; }
        public int NextId { get; set; }
        public int PreviousId { get; set; }
        public T Item { get; }
        
        public LoopNode(int id, T item, int previousId, int nextId)
        {
            Id = id;
            Item = item;
            PreviousId = previousId;
            NextId = nextId;
        }
    }

    private class NodeEnumerator : IEnumerator<T>
    {
        private readonly Loop<T> _loop;
        private readonly int _startId;
        private int _currentId;
        
        public NodeEnumerator(Loop<T> loop, int startId)
        {
            _loop = loop;
            _startId = startId;
            _currentId = -1;
            Current = _loop._nodes[_startId].Item;
        }

        public bool MoveNext()
        {
            if (_currentId == -1)
            {
                _currentId = _startId;
            }
            else
            {
                var nextId = _loop._nodes[_currentId].NextId;
                if (nextId == _startId)
                {
                    return false;
                }
                _currentId = nextId;
            }
            
            Current = _loop._nodes[_currentId].Item;
            return true;
        }

        public void Reset()
        {
            _currentId = -1;
            Current = _loop._nodes[_startId].Item;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            
        }
    }

    private class NodeEnumerable : IEnumerable<T>
    {
        private readonly NodeEnumerator _enumerator;
        
        public NodeEnumerable(Loop<T> loop, int startId)
        {
            _enumerator = new NodeEnumerator(loop, startId);
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    private class LoopCursor : ILoopCursor<T>
    {
        private readonly Loop<T> _loop;
        private int _nodeId;

        public LoopCursor(Loop<T> loop, int nodeId)
        {
            _loop = loop;
            _nodeId = nodeId;
        }

        public T Current => _loop._nodes[_nodeId].Item;
        
        public int CurrentId => _nodeId;
        
        private LoopNode Node => _loop._nodes[_nodeId];
        
        private LoopNode NextNode => _loop._nodes[Node.NextId];
        
        private LoopNode PreviousNode => _loop._nodes[Node.PreviousId];
        
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
            var id = _loop.InsertBefore(item, _nodeId);
            if (moveCursor)
            {
                _nodeId = id;
            }
            
            return id;
        }
        
        public int InsertAfter(T item, bool moveCursor = true)
        {
            var id = _loop.InsertAfter(item, _nodeId);
            if (moveCursor)
            {
                _nodeId = id;
            }

            return id;
        }
        
        public bool SeekNext(Func<T, bool> predicate)
        {
            var nextId = Node.NextId;
            while (nextId != _nodeId)
            {
                if (predicate(_loop._nodes[nextId].Item))
                {
                    _nodeId = nextId;
                    return true;
                }
                nextId = _loop._nodes[nextId].NextId;
            }

            return false;
        }
        
        public bool SeekPrevious(Func<T, bool> predicate)
        {
            var previousId = Node.PreviousId;
            while (previousId != _nodeId)
            {
                if (predicate(_loop._nodes[previousId].Item))
                {
                    _nodeId = previousId;
                    return true;
                }
                previousId = _loop._nodes[previousId].PreviousId;
            }

            return false;
        }
        
        public void Remove(bool moveForward = true)
        {
            if (_loop.Count == 0) return;
            
            var nextId = moveForward ? Node.NextId : Node.PreviousId;
            _loop.TryRemove(_nodeId);
            _nodeId = nextId;
        }
        
        public void MoveForward()
        {
            if (_loop.Count > 0)
            {
                _nodeId = Node.NextId;
            }
        }
        
        public void MoveBackward()
        {
            if (_loop.Count > 0)
            {
                _nodeId = Node.PreviousId;
            }
        }
        
        public void MoveTo(int id)
        {
            if (_loop._nodes.ContainsKey(id))
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
            if (_loop.Count > 0)
            {
                _nodeId = _loop._headId;
            }
        }
        
        public void MoveToTail()
        {
            if (_loop.Count > 0)
            {
                _nodeId = _loop.GetTailId();
            }
        }
        
        // what happens if we remove the node at the cursor position?
    }
    
}