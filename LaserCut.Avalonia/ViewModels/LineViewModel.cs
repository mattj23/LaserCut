using Avalonia;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class LineViewModel : DrawViewModelBase
{
    private Point _start;
    private Point _end;
    
    public LineViewModel() : base() {}
    
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
    
}