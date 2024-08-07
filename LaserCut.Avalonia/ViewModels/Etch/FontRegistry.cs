using System.Collections.ObjectModel;
using Avalonia.Media;
using LaserCut.Avalonia.Models;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class FontItem : ReactiveObject
{
    private FontFamily _family;
    private double _size;

    public FontItem(int id, FontFamily family, double size)
    {
        Id = id;
        _family = family;
        _size = size;
    }

    public int Id { get; }
    
    public IList<FontFamily> SystemOptions => SystemFonts.Instance.Fonts;

    public FontFamily Family
    {
        get => _family;
        set => this.RaiseAndSetIfChanged(ref _family, value);
    }
    
    public double Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }
}

public class FontRegistry : ReactiveObject
{
    private ObservableCollection<FontItem> _fonts = new();
    
    public FontRegistry()
    {
        Registered = new ReadOnlyObservableCollection<FontItem>(_fonts);
    }

    public IList<FontFamily> SystemOptions => SystemFonts.Instance.Fonts;
    
    public ReadOnlyObservableCollection<FontItem> Registered { get; } 
    
    public void AddNew(FontFamily family, double size)
    {
        var id = NextId();
        var item = new FontItem(id, family, size);
        _fonts.Add(item);
    }

    public void AddNew(string family, double size)
    {
        AddNew(new FontFamily(family), size);
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