using System.Collections;
using System.Net.Sockets;
using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

/// <summary>
/// Represents an internally managed wound series of points. 
/// </summary>
public class PointWinding : IReadOnlyList<Point2D>
{
    private readonly List<Point2D> _points;
    
    private double _area = double.NaN;
    private Aabb2 _bounds = Aabb2.Empty;
    
    private List<Segment>? _segments;
    private BvhNode? _bvh;
    
    public PointWinding(List<Point2D> points)
    {
        _points = points;
    }
    
    public PointWinding()
    {
        _points = new List<Point2D>();
    }
    
    public Point2D First => _points.First();
    
    public Point2D Last => _points.Last();
    
    public bool IsClosed => _points.Count > 3 && First == Last;
    
    public double Area => _area = double.IsNaN(_area) ? CalculateArea() : _area;
    
    public Aabb2 Bounds => _bounds = _bounds.IsEmpty ? Aabb2.FromPoints(_points): _bounds;
    
    public BvhNode Bvh => _bvh ??= BuildBvh();

    public void Add(double x, double y)
    {
        Add(new Point2D(x, y));
    }
    
    public void Add(Point2D point)
    {
        if (_points.Count > 0 && Last.DistanceTo(point) > 1e-6)
        {
            _points.Add(point);
            ResetCachedValues();
        }
    }
    
    public void AddRange(IEnumerable<Point2D> points)
    {
        foreach (var point in points)
        {
            Add(point);
        }
    }

    public IEnumerator<Point2D> GetEnumerator()
    {
        return _points.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_points).GetEnumerator();
    }

    public int Count => _points.Count;

    public Point2D this[int index] => _points[index];
    
    private void ResetCachedValues()
    {
        _area = double.NaN;
        _bounds = Aabb2.Empty;
        _bvh = null;
        _segments = null;
    }
    
    private BvhNode BuildBvh()
    {
        _segments = new List<Segment>();
        for (var i = 0; i < _points.Count - 1; i++)
        {
            _segments.Add(new Segment(_points[i], _points[i+1], i));
        }

        var root = new BvhNode(_segments);
        root.Split();

        return root;
    }
    
    private double CalculateArea()
    {
        var area = 0.0;
        for (var i = 0; i < _points.Count; i++)
        {
            var j = (i + 1) % _points.Count;
            area += _points[i].X * _points[j].Y;
            area -= _points[j].X * _points[i].Y;
        }

        return area / 2;
    }
}