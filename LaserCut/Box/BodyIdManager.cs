namespace LaserCut.Box;

public record GuidResults(Guid[] Used, HashSet<Guid> NotUsed, HashSet<Guid> New);

public class BodyIdManager
{
    private bool _finalized = false;
    private readonly Queue<Guid> _available = new();
    private readonly List<Guid> _used = new();
    private readonly HashSet<Guid> _new = new();

    public BodyIdManager() { }

    public BodyIdManager(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            _available.Enqueue(id);
        }
    }

    public Guid GetNextId()
    {
        if (_finalized) throw new InvalidOperationException("Cannot get an id from a finalized id manager.");

        // If we have any available ids, use one of those
        if (_available.TryDequeue(out var id))
        {
            _used.Add(id);
            return id;
        }

        // Otherwise, generate a new id
        id = Guid.NewGuid();
        _used.Add(id);
        _new.Add(id);
        return id;
    }

    public GuidResults Finish()
    {
        _finalized = true;
        return new GuidResults(_used.ToArray(), _available.ToHashSet(), _new.ToHashSet());
    }
}
