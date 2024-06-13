using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;
using ReactiveUI;

namespace LaserCut.Avalonia.Sample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DrawableEntities _entities = new();

    public MainWindowViewModel()
    {
        Observable.Timer(TimeSpan.FromMilliseconds(500))
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => TestRun());
    }

    public Interaction<Unit, Unit> ImportMeshInteraction { get; } = new();
    
    public DrawableEntities Entities
    {
        get => _entities;
        set => this.RaiseAndSetIfChanged(ref _entities, value);
    }


    private async void TestRun()
    {
        await ImportMeshInteraction.Handle(Unit.Default);
    }
}