using ReactiveUI;

namespace LaserCut.Avalonia.Models;

public class WorkspaceOrigin : ReactiveObject, IEntityWithOrigin
{
    public string Name => "Workspace Origin";
    public IOrigin Origin { get; } = new Origin(Guid.Empty, 0, 0, 0);
}