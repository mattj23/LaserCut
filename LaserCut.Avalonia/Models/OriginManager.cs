using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Binding;

namespace LaserCut.Avalonia.Models;

public class OriginManager
{
    private readonly ObservableCollection<IEntityWithOrigin> _options = new();
    public OriginManager()
    {
        // Options = new ReadOnlyObservableCollection<IEntityWithOrigin>(_options);
        
        _options.Add(new WorkspaceOrigin());
    }
    
    public ObservableCollection<IEntityWithOrigin> Options => _options;
    
    public void Add(IEntityWithOrigin entity)
    {
        _options.Add(entity);
    }
    
    public void Remove(IEntityWithOrigin entity)
    {
        _options.Remove(entity);
    }

    public IObservableList<IEntityWithOrigin> Filtered(Guid excludeId)
    {
        return Options.ToObservableChangeSet().Filter(x => x.Origin.Id != excludeId).AsObservableList();
    }
}