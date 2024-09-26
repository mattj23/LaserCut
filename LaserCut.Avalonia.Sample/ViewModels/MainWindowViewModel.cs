using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using LaserCut.Avalonia.Models;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;
using ReactiveUI;

namespace LaserCut.Avalonia.Sample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DrawableEntities _entities = new();
    private readonly UnitViewModel _unit = new();

    public MainWindowViewModel()
    {
        Length = new LengthEditViewModel(_unit);
        // Observable.Timer(TimeSpan.FromMilliseconds(500))
        //     .ObserveOn(SynchronizationContext.Current!)
        //     .Subscribe(_ => TestRun());
    }

    public LengthEditViewModel Length { get; }

    public Interaction<Unit, ImportedGeometry?> ImportMeshInteraction { get; } = new();

    public DrawableEntities Entities
    {
        get => _entities;
        set => this.RaiseAndSetIfChanged(ref _entities, value);
    }


    private async void TestRun()
    {
        var result = await ImportMeshInteraction.Handle(Unit.Default);
        Debug.WriteLine(result);
    }
}
