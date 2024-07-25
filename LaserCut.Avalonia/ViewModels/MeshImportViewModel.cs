using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using LaserCut.Algorithms;
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
            var r = await Task.Run(() => LoadMeshData(_filePath, cs));
            var result = new List<Body>(r);
            result.Add(new Body(BoundaryLoop.RoundedRectangle(10, 10, 35, 20, 5)));
            var bounds = result.CombinedBounds();
            var sx = bounds.Width / 10 - bounds.MinX;
            var sy = bounds.Height / 10 - bounds.MinY;

            var flag = true;
            foreach (var body in result)
            {
                body.ReplaceLinesWithArcs(1e-2);
                
                body.Translate(sx, sy);
                var drawable = new SimpleDrawable();
                
                drawable.Add(body.Outer.ToViewModel(null, Brushes.Black, 1), body.Outer.Bounds);
                foreach (var loop in body.Inners)
                {
                    drawable.Add(loop.ToViewModel(null, Brushes.DarkBlue, 1), loop.Bounds);
                }
                
                Entities.Register(drawable);
            }

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
    
    private Body[] LoadMeshData(string filePath, CoordinateSystem cs)
    {
        var m = Mesh3.ReadStl(filePath, true);
        return m.ExtractSilhouetteBodies(cs);
    }
    
}