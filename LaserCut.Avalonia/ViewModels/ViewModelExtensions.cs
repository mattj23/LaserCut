using Avalonia;
using Avalonia.Media;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia.ViewModels;

public static class ViewModelExtensions
{
    
    public static Point ToAvalonia(this Point2D point) => new Point(point.X, point.Y);
    
    public static Point ToAvalonia(this Point3D point) => new Point(point.X, point.Y);
    
    
    public static PolygonViewModel ToPolygonViewModel(this PointLoop loop, IBrush? fill = null, IBrush? stroke = null,
        double strokeThickness = 1)
    {
        return new PolygonViewModel
        {
            Points = loop.ToItemArray().Select(x => x.ToAvalonia()).ToList(), 
            Fill = fill,
            Stroke = stroke,
            StrokeThickness = strokeThickness,
            Bounds = loop.Bounds
        };
    }
    
}