using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using LaserCut.Algorithms;
using LaserCut.Avalonia.Models;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
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
    private bool _firstRun = false;
    private CoordinateSystem _lastCs = Isometry3.Default;
    private readonly List<Body> _bodies = new();

    public MeshImportViewModel(string filePath)
    {
        _filePath = filePath;
        _isNotValid = false;
        _loading = true;
        _replaceWithArcs = true;
        _arcBodyTol = 4e-2;
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
        
        ConfirmCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = new ImportedGeometry(_filePath, _bodies.ToArray(), []);
            await Confirm.Handle(result);

        }, this.WhenAnyValue(x => x.IsNotValid, x => !x));

        Observable.Timer(TimeSpan.FromMilliseconds(500))
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => UpdateGeometry(Isometry3.Default));

        this.WhenAnyValue(x => x.ReplaceWithArcs, x => x.ArcBodyTol, x => x.ArcPointTol)
            .Subscribe(_ =>
            {
                if (_firstRun) UpdateGeometry(_lastCs);
            });
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

    public Interaction<ImportedGeometry, Unit> Confirm { get; } = new();

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

    private Body[] ToArcs(Body[] bodies)
    {
        return bodies.Select(x =>
        {
            var copy = x.Copy();
            copy.ReplaceLinesWithArcs(ArcPointTol, ArcBodyTol);
            return copy;
        }).ToArray();
    }

    private void ToDrawable(IEnumerable<Body> bodies, double sx, double sy, IBrush outer, IBrush inner,
        Action<BoundaryLoop, SimpleDrawable>? action = null)
    {
        var drawable = new SimpleDrawable();
        foreach (var body in bodies)
        {
            var working = body.Copy();
            working.Translate(sx, sy);
            
            drawable.Add(working.Outer.ToViewModel(null, outer, 1.5), working.Outer.Bounds);
            action?.Invoke(working.Outer, drawable);
            foreach (var loop in working.Inners)
            {
                action?.Invoke(loop, drawable);
                drawable.Add(loop.ToViewModel(null, inner, 1.5), loop.Bounds);
            }
        }

        Entities.Register(drawable);
    }

    private async void UpdateGeometry(CoordinateSystem cs)
    {
        _bodies.Clear();
        _lastCs = cs;
        IsLoading = true;
        Entities.Clear();
        try
        {
            // Load the mesh data and flip it to the correct orientation
            var temp = await Task.Run(() => LoadMeshData(_filePath, cs));
            var result = temp.Select(x => x.MirroredY()).ToArray();
            var bounds = result.CombinedBounds();
            
            var sx = bounds.Width / 10 - bounds.MinX;
            var sy = bounds.Height / 10 - bounds.MinY;
            
            // If we're going to replace arc-like sections with true arc elements, the overall plan will be to preserve
            // and draw the original bodies a light gray color, but then draw the new bodies in a darker color.  If 
            // we aren't, we'll just draw the original bodies in black.
            if (ReplaceWithArcs)
            {
                ToDrawable(result, sx, sy, Brushes.DarkGray, Brushes.DarkGray);
                _bodies.AddRange(ToArcs(result));
                ToDrawable(_bodies, sx, sy, Brushes.Black, Brushes.MediumBlue, AddArcs);
            }
            else
            {
                _bodies.AddRange(result);
                ToDrawable(_bodies, sx, sy, Brushes.Black, Brushes.MediumBlue);
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
            _firstRun = true;
            IsLoading = false;
        }
    }

    private void AddArcs(BoundaryLoop loop, SimpleDrawable drawable)
    {
        foreach (var element in loop.Elements)
        {
            if (element is Arc arc)
            {
                drawable.Add(arc.ToViewModel(Brushes.LightBlue, 6), arc.Bounds);
            }
        }
    }
    
    private Body[] LoadMeshData(string filePath, CoordinateSystem cs)
    {
        var m = Mesh3.ReadStl(filePath, true);
        return m.ExtractSilhouetteBodies(cs);
    }
    
}