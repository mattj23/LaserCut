using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class ContourViewModel : DrawViewModelBase
{
    private PathGeometry _geometry = new();
    
    public ContourViewModel() : base()
    {
        var figure = new PathFigure()
        {
            IsClosed = true , 
            StartPoint = new Point(100, 100),
            Segments = new PathSegments()
        };
        
        figure.Segments.Add(new LineSegment() { Point = new Point(200, 100)});
        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(200, 200), 
            RotationAngle = 0,
            IsLargeArc = false,
            Size = new Size(50, 50),
        });
        figure.Segments.Add(new LineSegment() { Point = new Point(100, 200)});
        
        Geometry = new PathGeometry() { Figures = new PathFigures() { figure } };
    }
    
    public PathGeometry Geometry
    {
        get => _geometry;
        set => this.RaiseAndSetIfChanged(ref _geometry, value);
    }
}