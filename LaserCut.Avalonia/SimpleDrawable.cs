using System.Reactive.Linq;
using System.Reactive.Subjects;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia;

/// <summary>
/// This is the simplest implementation of an `IDrawable` which is basically just a passthrough for a list of
/// `IDrawViewModel` objects without any complex additional capabilities.
/// </summary>
public class SimpleDrawable : IDrawable
{
    private readonly List<IDrawViewModel> _geometries = new();
    private readonly Subject<IDrawViewModel> _added = new();
    private readonly Subject<IDrawViewModel> _removed = new();
    private readonly Dictionary<int, Aabb2> _bounds = new();
    private readonly Dictionary<int, IDrawViewModel> _geometriesById = new();

    public Guid Id { get; } = Guid.NewGuid();

    public IReadOnlyList<IDrawViewModel> Geometries => _geometries;
    
    public Aabb2 Bounds { get; private set; }

    public bool UseBounds => true;

    public IObservable<IDrawViewModel> Added => _added.AsObservable();
    
    public IObservable<IDrawViewModel> Removed => _removed.AsObservable();
    
    /// <summary>
    /// Add a new geometry to the drawable, getting back an integer ID which can be used to remove the geometry later.
    /// </summary>
    /// <param name="geometry"></param>
    /// <param name="bounds"></param>
    public void Add(IDrawViewModel geometry, Aabb2 bounds)
    {
        int nextId = _geometries.Count;
        _geometriesById[nextId] = geometry;
        _bounds[nextId] = bounds;
        
        _geometries.Add(geometry);
        _added.OnNext(geometry);

        Bounds = _bounds.Values.CombinedBounds();
    }
    
    public void Remove(int id)
    {
        var geometry = _geometriesById[id];
        _geometries.Remove(geometry);
        _removed.OnNext(geometry);
        _geometriesById.Remove(id);
        _bounds.Remove(id);
        Bounds = _bounds.Values.CombinedBounds();
    }
    
}