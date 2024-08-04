using Avalonia;
using Avalonia.Media;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class LineEtchViewModel : ReactiveObject, IDrawViewModel
{
    private UnitViewModel _units;
    private bool _isVisible;
    private double _strokeThickness;

    private Point _start;
    private Point _end;
    private Xyr _parentXyr;

    public LineEtchViewModel(UnitViewModel units)
    {
        _units = units;
        Id = Guid.NewGuid();
        Stroke = new SolidColorBrush(Colors.DodgerBlue);
        
        EditX0 = new LengthEditViewModel(units);
        EditY0 = new LengthEditViewModel(units);
        EditX1 = new LengthEditViewModel(units);
        EditY1 = new LengthEditViewModel(units);
        EditStrokeThickness = new LengthEditViewModel(units);
        
        EditX0.ValueChanged.Subscribe(_ => UpdatePoints());
        EditY0.ValueChanged.Subscribe(_ => UpdatePoints());
        EditX1.ValueChanged.Subscribe(_ => UpdatePoints());
        EditY1.ValueChanged.Subscribe(_ => UpdatePoints());
        EditStrokeThickness.ValueChanged.Subscribe(_ => StrokeThickness = EditStrokeThickness.GetValueMm());
    }

    public Guid Id { get; }
    
    public IBrush? Stroke { get; }
    
    public IBrush? Fill => null;
    
    public LengthEditViewModel EditX0 { get; } 
    public LengthEditViewModel EditY0 { get; }
    public LengthEditViewModel EditX1 { get; }
    public LengthEditViewModel EditY1 { get; }
    public LengthEditViewModel EditStrokeThickness { get; }
    
    
    public Point Start
    {
        get => _start;
        set => this.RaiseAndSetIfChanged(ref _start, value);
    }
    
    public Point End
    {
        get => _end;
        set => this.RaiseAndSetIfChanged(ref _end, value);
    }

    public double StrokeThickness
    {
        get => _strokeThickness;
        set => this.RaiseAndSetIfChanged(ref _strokeThickness, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public double DisplayThickness => StrokeThickness;
    
    public void UpdateParentXyr(Xyr xyr)
    {
        _parentXyr = xyr;
        UpdatePoints();
    }
    
    public void UpdateZoom(double zoom)
    {
    }
    
    private void UpdatePoints()
    {
        var p0 = new Point2D(EditX0.GetValueMm(), EditY0.GetValueMm());
        var p1 = new Point2D(EditX1.GetValueMm(), EditY1.GetValueMm());
        var t = _parentXyr.AsMatrix();
        Start = p0.Transformed(t).ToAvalonia();
        End = p1.Transformed(t).ToAvalonia();
    }
}