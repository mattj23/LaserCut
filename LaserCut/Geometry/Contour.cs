using System.Diagnostics.Contracts;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

/// <summary>
///     A `Contour` is a sequence of points arranged in the order they should be connected to form a shape.
///     Points should be in the counter-clockwise order to form a shape that encloses an area, and in the
///     clockwise order to form a shape that excludes an area.  This class has methods to aid in the process
///     of building contours.
/// </summary>
public class Contour
{
    private readonly List<Point2D> _points;

    public Contour()
    {
        _points = new List<Point2D>();
    }

    public Contour(IEnumerable<Point2D> points)
    {
        _points = new List<Point2D>(points);
    }

    public IReadOnlyList<Point2D> Points => _points;
    public Point2D Start => _points.First();
    public Point2D End => _points.Last();
    
    public Contour Clone()
    {
        return new Contour(_points);
    }

    public void Extend(Contour other)
    {
        _points.AddRange(other._points);
    }

    public Contour Transformed(Matrix transformation)
    {
        return new Contour(_points.Select(p => p.Transformed(transformation)));
    }

    public Contour Scaled(double scale)
    {
        return new Contour(_points.Select(x => Point2D.OfVector(x.ToVector() * scale)));
    }

    public void AddAbs(Point2D point)
    {
        _points.Add(point);
    }

    public void AddAbs(double x, double y)
    {
        _points.Add(new Point2D(x, y));
    }
    
    public void AddAbsX(double x)
    {
        _points.Add(new Point2D(x, _points.Last().Y));
    }
    
    public void AddAbsY(double y)
    {
        _points.Add(new Point2D(_points.Last().X, y));
    }

    public void AddPoints(IEnumerable<Point2D> points)
    {
        _points.AddRange(points);
    }

    public void AddRel(double x, double y)
    {
        if (_points.Count == 0)
            AddAbs(x, y);
        else
            AddAbs(_points.Last().X + x, _points.Last().Y + y);
    }

    public void AddRelX(double x)
    {
        AddRel(x, 0);
    }

    public void AddRelY(double y)
    {
        AddRel(0, y);
    }

    /// <summary>
    /// Create and return a new contour that is a mirror of this contour around the given X position.
    /// </summary>
    /// <param name="x">The X position about which to mirror the points. Defaults to 0.</param>
    /// <returns>A new contour object</returns>
    [Pure]
    public Contour MirroredX(double x = 0)
    {
        // Mirror the contour around the position given by x
        var mirrored = new Contour();
        // Iterate backwards
        for (var i = _points.Count - 1; i >= 0; i--)
        {
            var dx = _points[i].X - x;
            mirrored.AddAbs(x - dx, _points[i].Y);
        }

        return mirrored;
    }

    /// <summary>
    /// Create and return a new contour that is a mirror of this contour around the given Y position.
    /// </summary>
    /// <param name="y">The y position about which to mirror the points. Defaults to 0.</param>
    /// <returns>A new contour object</returns>
    [Pure]
    public Contour MirroredY(double y = 0)
    {
        // Mirror the contour around the position given by y
        var mirrored = new Contour();
        // Iterate backwards
        for (var i = _points.Count - 1; i >= 0; i--)
        {
            var dy = _points[i].Y - y;
            mirrored.AddAbs(_points[i].X, y - dy);
        }

        return mirrored;
    }

    /// <summary>
    /// If the contour's last point is not equal to the first point, add a new point that is the same as
    /// the first point.  This will essentially 'close' the contour, geometrically.
    /// </summary>
    public void Close()
    {
        if (_points.Count > 0)
            if (_points.First() != _points.Last())
                _points.Add(_points.First());
    }

    /// <summary>
    /// If the contour's last point' X is not equal to the first point's X, add a new point that has
    /// the first point's X and the last point's Y.
    ///
    /// For example, if finishing a CCW contour that's a rectangle where the last point is diagonal from
    /// the first point, you would first call CloseX() to draw the top edge and then CloseY() to draw
    /// the left edge.
    /// </summary>
    public void CloseX()
    {
        if (_points.Count <= 0) 
            throw new InvalidOperationException("Cannot close a contour with no points");
        
        if (Math.Abs(_points.First().X - _points.Last().X) > 1e-6)
            _points.Add(new Point2D(_points.First().X, _points.Last().Y));
    }

    /// <summary>
    /// If the contour's last point' Y is not equal to the first point's Y, add a new point that has
    /// the first point's Y and the last point's X.
    ///
    /// For example, if finishing a CW contour that's a rectangle where the last point is diagonal from
    /// the first point, you would first call CloseY() to draw the right edge and then CloseX() to draw
    /// the bottom edge.
    /// </summary>
    public void CloseY()
    {
        if (_points.Count <= 0) 
            throw new InvalidOperationException("Cannot close a contour with no points");
        
        if (Math.Abs(_points.First().Y - _points.Last().Y) > 1e-6)
            _points.Add(new Point2D(_points.Last().X, _points.First().Y));
    }

    /// <summary>
    /// Create a new contour where the edges are offset by the given amount.  If the points are ordered
    /// in the counter-clockwise direction, a positive offset will be to the outside of the shape.  If
    /// the points are ordered in the clockwise direction, a positive offset will be to the inside of
    /// the shape.
    /// </summary>
    /// <param name="distance">The distance to offset the lines of the contour</param>
    /// <returns>A new contour with the given offset</returns>
    [Pure]
    public Contour Offset(double distance)
    {
        throw new NotImplementedException("Contour offset not implemented yet");
    }
}