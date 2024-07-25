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
    private bool _replaceWithArcs;
    private double _arcPointTol;
    private double _arcBodyTol;
    private CoordinateSystem _lastCs = Isometry3.Default;
    private readonly List<Body> _bodies = new();

    public MeshImportViewModel(string filePath)
    {
        _filePath = filePath;
        _isNotValid = false;
        _loading = true;
        _replaceWithArcs = true;
        _arcBodyTol = 1e-2;
        _arcPointTol = 1e-3;
        
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

        this.WhenAnyValue(x => x.ReplaceWithArcs, x => x.ArcBodyTol, x => x.ArcPointTol)
            .Subscribe(_ => UpdateGeometry(_lastCs));
    }

    public MeshImportViewModel() : this("") { }

    public Interaction<Unit, Unit> ZoomToFit { get; } = new();
    public ReactiveCommand<Unit, Unit> SetXPlusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetXMinusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetYPlusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetYMinusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetZPlusCommand { get; }
    public ReactiveCommand<Unit, Unit> SetZMinusCommand { get; }

    public ReactiveCommand<Unit, Unit> ZoomToFitCommand { get; }
    
    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

    public bool ReplaceWithArcs
    {
        get => _replaceWithArcs;
        set => this.RaiseAndSetIfChanged(ref _replaceWithArcs, value);
    }

    public double ArcPointTol
    {
        get => _arcPointTol;
        set => this.RaiseAndSetIfChanged(ref _arcPointTol, value);
    }

    public double ArcBodyTol
    {
        get => _arcBodyTol;
        set => this.RaiseAndSetIfChanged(ref _arcBodyTol, value);
    }

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

    private IBrush BrushOuter(bool force)
    {
        return force || !ReplaceWithArcs ? Brushes.Black : Brushes.LightGray;
    }
    
    private IBrush BrushInner(bool force)
    {
        return force || !ReplaceWithArcs ? Brushes.DarkBlue : Brushes.LightGray;
    }
    
    private async void UpdateGeometry(CoordinateSystem cs)
    {
        _bodies.Clear();
        _lastCs = cs;
        IsLoading = true;
        Entities.Clear();
        try
        {
            var result = await Task.Run(() => LoadMeshData(_filePath, cs));
            var bounds = result.CombinedBounds();
            var sx = bounds.Width / 10 - bounds.MinX;
            var sy = bounds.Height / 10 - bounds.MinY;
            
            // We draw the silhouette bodies, but what color we use depends on whether we are replacing with arcs.
            foreach (var body in result)
            {
                body.Translate(sx, sy);
                var drawable = new SimpleDrawable();
                
                drawable.Add(body.Outer.ToViewModel(null, BrushOuter(false), 1.5), body.Outer.Bounds);
                foreach (var loop in body.Inners)
                {
                    drawable.Add(loop.ToViewModel(null, BrushInner(false), 1.5), loop.Bounds);
                }
                Entities.Register(drawable);
            }
            
            // If we are replacing with arcs, we need to do that now
            if (ReplaceWithArcs)
            {
                foreach (var body in result)
                {
                    var copied = body.Copy();
                    copied.ReplaceLinesWithArcs(ArcPointTol, ArcBodyTol);
                    _bodies.Add(copied);
                    
                    var drawable = new SimpleDrawable();
                    
                    drawable.Add(copied.Outer.ToViewModel(null, BrushOuter(true), 1.5), body.Outer.Bounds);
                    foreach (var loop in copied.Inners)
                    {
                        drawable.Add(loop.ToViewModel(null, BrushInner(true), 1.5), loop.Bounds);
                    }
                    Entities.Register(drawable);
                }
            }
            else
            {
                _bodies.AddRange(result);
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