using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// A `PointLoop` is a sequence of points that form a closed loop, used to represent a single exterior or interior
/// boundary. A `PointLoop` needs at least three points to be valid.
///
/// Internally, a `PointLoop` is a mutable ring stored as nodes in a Dictionary.  Nodes can easily be inserted or
/// removed from the loop.
///
/// Winding order used to determine if the loop is an exterior or interior boundary, such that the area calculation will
/// yield a positive value for exterior boundaries and a negative value for interior boundaries.
/// </summary>
public abstract class Loop<T>
{
    private int _nextId = 0;
    private int _lastCreatedId = -1;
    private readonly Dictionary<int, LoopNode> _nodes = new();

    public Loop(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            _lastCreatedId = InsertAfter(item, _lastCreatedId);
        }
    }
    
    public abstract void OnItemChanged(T item);
    
    public ILoopCursor<T> GetCursor(int? id = null)
    {
        return new LoopCursor(this, id ?? _lastCreatedId);
    }

    private int InsertBefore(T item, int idOfNext)
    {
        if (_nodes.Count == 0)
        {
            var id = NewId();
            var node = new LoopNode(id, item, id, id);
            _nodes[id] = node;
            OnItemChanged(node.Item);
            return id;
        }

        // Get the node which will come after the one we're inserting
        if (_nodes.TryGetValue(idOfNext, out var afterNode))
        {
            var id = NewId();
            var node = new LoopNode(id, item, afterNode.PreviousId, afterNode.Id);
            var beforeNode = _nodes[afterNode.PreviousId];
            beforeNode.NextId = id;
            afterNode.PreviousId = id;
            _nodes[id] = node;
            OnItemChanged(node.Item);
            return id;
        }

        throw new KeyNotFoundException("Invalid id to insert before");
        
    }
    
    private int InsertAfter(T item, int idOfPrevious)
    {
        if (_nodes.Count == 0)
        {
            var id = NewId();
            var node = new LoopNode(id, item, id, id);
            _nodes[id] = node;
            OnItemChanged(node.Item);
            return id;
        }

        // Get the node that comes before the one we're inserting
        if (_nodes.TryGetValue(idOfPrevious, out var beforeNode))
        {
            var id = NewId();
            var node = new LoopNode(id, item, beforeNode.Id, beforeNode.NextId);
            var afterNode = _nodes[beforeNode.NextId];
            beforeNode.NextId = id;
            afterNode.PreviousId = id;
            _nodes[id] = node;
            OnItemChanged(node.Item);
            return id;
        }

        throw new KeyNotFoundException("Invalid id to insert after");
    }
    
    private void Remove(int id)
    {
        if (_nodes.TryGetValue(id, out var node))
        {
            var beforeNode = _nodes[node.PreviousId];
            var afterNode = _nodes[node.NextId];
            beforeNode.NextId = node.NextId;
            afterNode.PreviousId = node.PreviousId;
            _nodes.Remove(id);
            OnItemChanged(node.Item);
            
            
        }
    }

    private int NewId()
    {
        return _nextId++;
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
            var id = _loop.InsertBefore(item, Node.Id);
            if (moveCursor)
            {
                _nodeId = id;
            }
            
            return id;
        }
        
        public int InsertAfter(T item, bool moveCursor = true)
        {
            var id = _loop.InsertAfter(item, Node.Id);
            if (moveCursor)
            {
                _nodeId = id;
            }

            return id;
        }
        
        // what happens if we remove the node at the cursor position?
    }
    
}