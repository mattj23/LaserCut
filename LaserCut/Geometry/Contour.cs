using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public abstract record ContourPoint(Point2D Point);

public record ContourLine(Point2D Point) : ContourPoint(Point);

public record ContourArc(Point2D Point, Point2D Center, bool Clockwise) : ContourPoint(Point);

public class Contour : Loop<ContourPoint>
{
    private Bvh? _bvh = null;
    private List<IContourElement>? _elements = null;
    private double _area = double.NaN;
    
    public Contour(Guid id)
    {
        Id = id;
    }
    
    public Contour() : this(Guid.NewGuid())
    {
    }
        
    
    public Guid Id { get; }

    public IReadOnlyList<IContourElement> Elements => _elements ??= BuildElements();
    
    public Bvh Bvh => _bvh ??= new Bvh(Elements);
    
    public double Area => _area = double.IsNaN(_area) ? CalculateArea() : _area;

    public override IContourCursor GetCursor(int? id = null)
    {
        return new ContourCursor(this, id ?? GetTailId());
    }

    public override void OnItemChanged(ContourPoint item)
    {
        ResetCachedValues();
        base.OnItemChanged(item);
    }

    private void ResetCachedValues()
    {
        _area = double.NaN;
        _bvh = null;
        _elements = null;
    }

    private double CalculateArea()
    {
        double area = 0.0;
        foreach (var e in Elements)
        {
            area += e.CrossProductWedge;
        }
        
        return area / 2.0;
    }

    private List<IContourElement> BuildElements()
    {
        var elements = new List<IContourElement>();
        foreach (var (a, b) in IterEdges())
        {
            IContourElement e = a.Item switch
            {
                ContourLine line => new Segment(line.Point, b.Item.Point, a.Id),
                ContourArc arc => Arc.FromEnds(arc.Point, b.Item.Point, arc.Center, arc.Clockwise, a.Id),
                _ => throw new NotImplementedException()
            };
            elements.Add(e);
        }

        return elements;
    }

    private class ContourCursor : LoopCursor, IContourCursor
    {
        public ContourCursor(Loop<ContourPoint> loop, int nodeId) : base(loop, nodeId) { }

        protected override void OnItemAdded(ContourPoint item, int id)
        {
            base.OnItemAdded(item, id);
        }
    }
}