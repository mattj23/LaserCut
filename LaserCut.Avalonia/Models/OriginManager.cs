using System.Collections.ObjectModel;
using ReactiveUI;

namespace LaserCut.Avalonia.Models;

public class OriginOptions : ReactiveObject
{
    private readonly ObservableCollection<IEntityWithOrigin> _options;

    public OriginOptions(IEnumerable<IEntityWithOrigin> source)
    {
        _options = new ObservableCollection<IEntityWithOrigin>(source);
        Options = new ReadOnlyObservableCollection<IEntityWithOrigin>(_options);
    }
    
    public void SynchronizeOptions(IEnumerable<IEntityWithOrigin> source)
    {
        var target = source.ToArray();
        // Remove items that are no longer in the source, working backwards
        for (int i = _options.Count - 1; i >= 0; i--)
        {
            if (!target.Contains(_options[i]))
            {
                _options.RemoveAt(i);
            }
        }
        
        // Add items that are in the source but not in the target
        foreach (var item in target)
        {
            if (!_options.Contains(item))
            {
                _options.Add(item);
            }
        }
    }

    public ReadOnlyObservableCollection<IEntityWithOrigin> Options { get; }
    
}

public class OriginManager
{
    private readonly ObservableCollection<IEntityWithOrigin> _options = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _dependencies = new();
    private readonly Dictionary<Guid, IOrigin> _origins = new();
    private readonly Dictionary<Guid, OriginOptions> _filtered = new();
    
    public OriginManager()
    {
        Add(new WorkspaceOrigin());
    }
    
    public void Add(IEntityWithOrigin entity)
    {
        _options.Add(entity);
        _origins[entity.Origin.Id] = entity.Origin;
        entity.Origin.ParentChanged
            .Subscribe(_ => ConstructDependencies());
        ConstructDependencies();
    }
    
    public void Remove(IEntityWithOrigin entity)
    {
        _options.Remove(entity);
        _origins.Remove(entity.Origin.Id);
        ConstructDependencies();
    }

    public OriginOptions Filtered(Guid itemId)
    {
        if (_filtered.TryGetValue(itemId, out var filtered))
        {
            return filtered;
        }
        if (_dependencies.TryGetValue(itemId, out var deps))
        {
            _filtered[itemId] = new OriginOptions(_options.Where(o => !deps.Contains(o.Origin.Id)));
        }
        else
        {
            _filtered[itemId] = new OriginOptions([_options[0]]);
        }
        return _filtered[itemId];
    }

    private void ConstructDependencies()
    {
        _dependencies.Clear();
        foreach (var option in _options)
        {
            AddChild(option.Origin.Id, option.Origin.Id);

            var working = option.Origin;
            while (working.ParentId != Guid.Empty)
            {
                AddChild(working.ParentId, option.Origin.Id);
                working = _origins[working.ParentId];
            }
        }

        foreach (var (itemId, options) in _filtered)
        {
            options.SynchronizeOptions(_options.Where(o => !_dependencies[itemId].Contains(o.Origin.Id)));
        }
    }
    
    private void AddChild(Guid parentId, Guid childId)
    {
        // TODO: this is naive and repetitive, fix it later
        if (!_dependencies.ContainsKey(parentId))
        {
            _dependencies[parentId] = new HashSet<Guid>();
        }
        
        _dependencies[parentId].Add(childId);
    }
}