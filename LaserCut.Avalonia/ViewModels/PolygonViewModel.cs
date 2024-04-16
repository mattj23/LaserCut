using Avalonia;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class PolygonViewModel : DrawViewModelBase
{
    private IList<Point> _points;

    public PolygonViewModel() : base()
    {
        _points = new List<Point>();
    }

    public IList<Point> Points
    {
        get => _points;
        set => this.RaiseAndSetIfChanged(ref _points, value);
    }
}