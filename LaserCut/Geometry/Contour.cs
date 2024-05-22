using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;

namespace LaserCut.Geometry;

public class Contour : Loop<IContourElement>
{
    private Bvh? _bvh = null;
    
    public Contour(Guid id)
    {
        Id = id;
    }
    
    public Contour() : this(Guid.NewGuid())
    {
    }
        
    
    public Guid Id { get; }
    
    public bool IsClosed => !IsEmpty && Head.PrecededBy(Tail);
    
    public Bvh Bvh => _bvh ??= new Bvh(IterItems().Select(x => x.Item));

    public override IContourCursor GetCursor(int? id = null)
    {
        return new ContourCursor(this, id ?? GetTailId());
    }

    public override void OnItemChanged(IContourElement item)
    {
        base.OnItemChanged(item);
    }

    private class ContourCursor : LoopCursor, IContourCursor
    {
        public ContourCursor(Loop<IContourElement> loop, int nodeId) : base(loop, nodeId)
        {
        }
        
        
    }
}