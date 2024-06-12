using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;
using ReactiveUI;

namespace LaserCut.Avalonia.Sample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DrawableEntities _entities = new();

    public MainWindowViewModel()
    {
        var drawable = new Drawable();
        Entities.Register(drawable);
    }
    
    public DrawableEntities Entities
    {
        get => _entities;
        set => this.RaiseAndSetIfChanged(ref _entities, value);
    }
}