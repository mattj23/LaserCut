using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class BoundaryLoopViewModel : DrawViewModelBase
{
    private PathGeometry _geometry = new();
    
    public BoundaryLoopViewModel() : base()
    {

    }
    
    public PathGeometry Geometry
    {
        get => _geometry;
        set => this.RaiseAndSetIfChanged(ref _geometry, value);
    }
}