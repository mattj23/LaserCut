using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using LaserCut.Geometry;
using LaserCut.Mesh;
using MathNet.Spatial.Euclidean;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class MeshImportViewModel : ReactiveObject
{
    private string _filePath;
    private DrawableEntities _entities = new();
    private bool _isNotValid;
    private bool _loading;

    public MeshImportViewModel(string filePath)
    {
        _filePath = filePath;
        _isNotValid = false;
        _loading = true;
        
        ZoomToFitCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await ZoomToFit.Handle(Unit.Default);
        });

        SetXPlusCommand = ReactiveCommand.Create(() => UpdateGeometry(Isometry3.XPlusToZ));
        SetXMinusCommand = ReactiveCommand.Create(() => UpdateGeometry(Isometry3.XMinusToZ));
        SetYPlusCommand = ReactiveCommand.Create(() => UpdateGeometry(Isometry3.YPlusToZ));
        SetYMinusCommand = ReactiveCommand.Create(() => UpdateGeometry(Isometry3.YMinusToZ));
        SetZPlusCommand = ReactiveCommand.Create(() => UpdateGeometry(Isometry3.Default));
        SetZMinusCommand = ReactiveCommand.Create(() => UpdateGeometry(Isometry3.ZMinusToZ));
        
        // ConfirmCommand = ReactiveCommand.CreateFromTask(async () =>
        // {
        //     await Confirm.Handle(_piece!);
        // }, this.WhenAnyValue(x => x.IsNotValid, x => !x));

        Observable.Timer(TimeSpan.FromMilliseconds(500))
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => UpdateGeometry(Isometry3.Default));
    }
    
    public Interaction<Unit, Unit> ZoomToFit { get; } = new();
    public ReactiveCommand<Unit, Unit> SetXPlusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetXMinusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetYPlusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetYMinusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetZPlusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetZMinusCommand { get; }

    public ReactiveCommand<Unit, Unit> ZoomToFitCommand { get; }
    
    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
    
    public bool IsNotValid
    {
        get => _isNotValid;
        set => this.RaiseAndSetIfChanged(ref _isNotValid, value);
    }

    public bool IsLoading
    {
        get => _loading;
        set => this.RaiseAndSetIfChanged(ref _loading, value);
    }
    
    public DrawableEntities Entities
    {
        get => _entities;
        set => this.RaiseAndSetIfChanged(ref _entities, value);
    }
    
    private async void UpdateGeometry(CoordinateSystem cs)
    {
        IsLoading = true;
        Entities.Clear();
        try
        {
            var result = await Task.Run(() => LoadMeshData(_filePath, cs));
            var drawable = new SimpleDrawable();
            foreach (var contour in result)
            {
                drawable.Add(contour.ToViewModel(null, Brushes.Black, 1), contour.Bounds);
            }
            Entities.Register(drawable);

            await ZoomToFit.Handle(Unit.Default);
            IsNotValid = false;
        }
        catch (Exception e)
        {
            IsNotValid = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private Contour[] LoadMeshData(string filePath, CoordinateSystem cs)
    {
        var m = Mesh3.ReadStl(filePath, true);
        return m.ExtractSilhouetteContours(cs);
    }
    
}