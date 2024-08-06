using Avalonia;
using Avalonia.Media;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class LineEtchViewModel : EtchEntityViewModelBase
{
    private Point _start;
    private Point _end;

    public LineEtchViewModel(UnitViewModel units) : this(Guid.NewGuid(), units) { }
    
    public LineEtchViewModel(Guid id, UnitViewModel units) : base(id)
    {
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

    public override void OnParentXyrChanged()
    {
        UpdatePoints();
    }

    public override void UpdateZoom(double zoom) { }
    
    private void UpdatePoints()
    {
        var p0 = new Point2D(EditX0.GetValueMm(), EditY0.GetValueMm());
        var p1 = new Point2D(EditX1.GetValueMm(), EditY1.GetValueMm());
        var t = ParentXyr.AsMatrix();
        Start = p0.Transformed(t).ToAvalonia();
        End = p1.Transformed(t).ToAvalonia();
    }
}