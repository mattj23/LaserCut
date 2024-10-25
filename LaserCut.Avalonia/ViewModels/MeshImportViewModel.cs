﻿using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using LaserCut.Algorithms;
using LaserCut.Avalonia.Models;
using LaserCut.Data;
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

    // Set when the first run has completed
    private bool _firstRun;

    private CoordinateSystem _lastCs = Isometry3.Default;
    private readonly List<Body> _bodies = new();
    private readonly List<FlatPatch> _flat = new();

    private readonly MeshImportConfig _config;

    public MeshImportViewModel(string filePath, MeshImportConfig config)
    {
        _filePath = filePath;
        _config = config;
        _isNotValid = false;
        _loading = true;
        _replaceWithArcs = true;

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
            var result = new ImportedGeometry(_filePath, _bodies.ToArray(), _flat.ToArray());
            await Confirm.Handle(result);

        }, this.WhenAnyValue(x => x.IsNotValid, x => !x));

        ArcPointTol = new LengthEditViewModel(_config.Units)
        {
            DecimalPlaces = new Dictionary<LengthUnit, int>{ { LengthUnit.Millimeter, 5 }, { LengthUnit.Inch, 6} },
            Increment = new Dictionary<LengthUnit, double>{ { LengthUnit.Millimeter, 0.001 }, { LengthUnit.Inch, 0.0001} }
        };

        ArcBodyTol = new LengthEditViewModel(_config.Units)
        {
            DecimalPlaces = new Dictionary<LengthUnit, int>{ { LengthUnit.Millimeter, 5 }, { LengthUnit.Inch, 6} },
            Increment = new Dictionary<LengthUnit, double>{ { LengthUnit.Millimeter, 0.01 }, { LengthUnit.Inch, 0.001} }
        };

        ArcPointTol.SetValue(1e-3);
        ArcBodyTol.SetValue(1e-2);

        var arcThrottle = TimeSpan.FromMilliseconds(500);

        ArcPointTol.ValueChanged
            .Throttle(arcThrottle)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => InvalidateGeometry());

        ArcBodyTol.ValueChanged
            .Throttle(arcThrottle)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => InvalidateGeometry());

        this.WhenAnyValue(x => x.ReplaceWithArcs)
            .Throttle(arcThrottle)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => InvalidateGeometry());

        Observable.Timer(TimeSpan.FromMilliseconds(500))
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => UpdateGeometry(Isometry3.Default));
    }

    public MeshImportViewModel(MeshImportConfig config) : this("", config) { }

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

    public LengthEditViewModel ArcPointTol { get; }
    public LengthEditViewModel ArcBodyTol { get; }

    public bool ReplaceWithArcs
    {
        get => _replaceWithArcs;
        set => this.RaiseAndSetIfChanged(ref _replaceWithArcs, value);
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
            copy.ReplaceLinesWithArcs(ArcPointTol.GetValueMm(), ArcBodyTol.GetValueMm());
            return copy;
        }).ToArray();
    }

    private void InvalidateGeometry()
    {
        if (_firstRun) UpdateGeometry(_lastCs);
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
            var (tempBodies, tempPatches) = await Task.Run(() => LoadMeshData(_filePath, cs));
            var resultBodies = tempBodies.Select(x => x.MirroredY()).ToArray();
            var resultPatches = tempPatches.Select(x => x.MirrorY()).ToArray();
            _flat.Clear();
            _flat.AddRange(resultPatches);

            var bounds = resultBodies.CombinedBounds();

            var sx = bounds.Width / 10 - bounds.MinX;
            var sy = bounds.Height / 10 - bounds.MinY;

            // Draw the flat patches, if there are any
            if (resultBodies.Length > 0)
            {
                var flatBodies = resultPatches.Select(x => x.Body).ToArray();
                ToDrawable(flatBodies, sx, sy, Brushes.SlateGray, Brushes.SlateGray);
            }

            // If we're going to replace arc-like sections with true arc elements, the overall plan will be to preserve
            // and draw the original bodies a light gray color, but then draw the new bodies in a darker color.  If
            // we aren't, we'll just draw the original bodies in black.
            if (ReplaceWithArcs)
            {
                ToDrawable(resultBodies, sx, sy, Brushes.DarkGray, Brushes.DarkGray);
                _bodies.AddRange(ToArcs(resultBodies));
                ToDrawable(_bodies, sx, sy, Brushes.Black, Brushes.MediumBlue, AddArcs);
            }
            else
            {
                _bodies.AddRange(resultBodies);
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

    private (Body[], FlatPatch[]) LoadMeshData(string filePath, CoordinateSystem cs)
    {
        var m = Mesh3.ReadStl(filePath, true);
        var bodies = m.ExtractSilhouetteBodies(cs, 0.5);

        if (_config.FlatPatches)
        {
            var flat = m.ExtractFlatPatches(cs);
            return (bodies, flat);
        }

        return (bodies, []);
    }

}
