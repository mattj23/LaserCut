using ReactiveUI;

namespace LaserCut.Avalonia.Models;

public interface IEntityWithOrigin : IReactiveObject
{
    string Name { get; }
    
    IOrigin Origin { get; }
}