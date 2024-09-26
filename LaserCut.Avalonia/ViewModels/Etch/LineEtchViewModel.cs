using Avalonia;
using Avalonia.Media;
using LaserCut.Avalonia.HitTesting;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class LineEtchViewModel : EtchEntityViewModelBase
{
    private Point _start;
    private Point _end;
    private Segment? _segment;

    public LineEtchViewModel(UnitViewModel units) : this(Guid.NewGuid(), units) { }

    public LineEtchViewModel(Guid id, UnitViewModel units) : base(id)
    {
        EditX0 = new LengthEditViewModel(units).WithDefaultSettings();
        EditY0 = new LengthEditViewModel(units).WithDefaultSettings();
        EditX1 = new LengthEditViewModel(units).WithDefaultSettings();
        EditY1 = new LengthEditViewModel(units).WithDefaultSettings();
        EditStrokeThickness = new LengthEditViewModel(units);

        EditX0.ValueChanged.Subscribe(_ => OnPointsEdited());
        EditY0.ValueChanged.Subscribe(_ => OnPointsEdited());
        EditX1.ValueChanged.Subscribe(_ => OnPointsEdited());
        EditY1.ValueChanged.Subscribe(_ => OnPointsEdited());
        EditStrokeThickness.ValueChanged.Subscribe(_ => StrokeThickness = EditStrokeThickness.GetValueMm());
    }

    public LengthEditViewModel EditX0 { get; }
    public LengthEditViewModel EditY0 { get; }
    public LengthEditViewModel EditX1 { get; }
    public LengthEditViewModel EditY1 { get; }
    public LengthEditViewModel EditStrokeThickness { get; }

    public override Aabb2 Bounds => _segment?.Bounds ?? Aabb2.Empty;

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
        UpdateGeometry();
    }

    public override void UpdateZoom(double zoom) { }

    private void OnPointsEdited()
    {
        var p0 = new Point2D(EditX0.GetValueMm(), EditY0.GetValueMm());
        var p1 = new Point2D(EditX1.GetValueMm(), EditY1.GetValueMm());
        if (p0.DistanceTo(p1) < GeometryConstants.DistEquals) p1 = p0 + new Vector2D(1e-4, 1e-4);

        _segment = new Segment(p0, p1, 0);
        NotifyChange();
        UpdateGeometry();
    }

    private void UpdateGeometry()
    {
        if (_segment is null) return;

        var s = _segment.Transformed(ParentXyr.AsMatrix());
        Start = s.Start.ToAvalonia();
        End = s.End.ToAvalonia();
    }

    public override void UpdateHitGeometry()
    {
        OnPointsEdited();
    }

    public override bool Hit(Point2D point)
    {
        if (_segment is null) return false;

        return _segment.Closest(point).Surface.Point.DistanceTo(point) < EditStrokeThickness.GetValueMm() * 4;
    }
}
