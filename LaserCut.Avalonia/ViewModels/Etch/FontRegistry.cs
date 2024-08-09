using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Subjects;
using Avalonia.Media;
using LaserCut.Avalonia.Models;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class FontRegistry : ReactiveObject
{
    private readonly ObservableCollection<FontItem> _fonts = new();
    private readonly Subject<Unit> _removedSubject = new();
    private readonly Subject<Unit> _stateChanged = new();
    private readonly Dictionary<int, IDisposable> _subscriptions = new();
    
    public FontRegistry()
    {
        Registered = new ReadOnlyObservableCollection<FontItem>(_fonts);
    }
    
    public IObservable<Unit> StateChanged => _stateChanged;
    
    public IObservable<Unit> Removed => _removedSubject;
    
    public int Count => _fonts.Count;

    public IList<FontFamily> SystemOptions => SystemFonts.Instance.Fonts;
    
    public ReadOnlyObservableCollection<FontItem> Registered { get; }

    public void Add(int id, string family, double size)
    {
        if (_fonts.Any(x => x.Id == id)) throw new ArgumentException($"There is already a font with the key {id}");
        
        var fontFamily = SystemOptions.FirstOrDefault(x => x.Name == family);
        if (fontFamily == null) throw new ArgumentException($"The font family {family} is not available");
        
        AddItem(new FontItem(id, fontFamily, size, this));
    }
    
    public void AddNew(FontFamily family, double size)
    {
        var id = NextId();
        var item = new FontItem(id, family, size, this);
        AddItem(item);
    }

    public void AddNew(string family, double size)
    {
        AddNew(new FontFamily(family), size);
    }
    
    public void Remove(FontItem item)
    {
        if (Count == 1) return;

        if (_subscriptions.Remove(item.Id, out var sub))
        {
            sub.Dispose();
        }
        
        _fonts.Remove(item);
        _removedSubject.OnNext(default);
        this.RaisePropertyChanged(nameof(Count));
        _stateChanged.OnNext(default);
    }

    public void AddNew()
    {
        if (_fonts.Count == 0)
        {
            AddNew(SystemFonts.Instance.Default, 12);
        }
        else
        {
            AddNew(_fonts.Last().Family, _fonts.Last().Size);
        }
    }

    private void AddItem(FontItem item)
    {
        _subscriptions[item.Id] = item.WhenAnyValue(x => x.Family, x => x.Size)
            .Subscribe(_ => _stateChanged.OnNext(default));
        
        _fonts.Add(item);
        this.RaisePropertyChanged(nameof(Count));
        
        _stateChanged.OnNext(default);
    }

    private int NextId()
    {
        if (_fonts.Count == 0) return 1;
        
        return _fonts.Select(x => x.Id).Max() + 1;
    }
    
    public static FontRegistry New()
    {
        var registry = new FontRegistry();
        registry.AddNew(SystemFonts.Instance.Default, 12);
        return registry;
    }
}