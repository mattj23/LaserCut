using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Media;
using LaserCut.Avalonia.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class FontItem : ReactiveObject
{
    private FontFamily _family;
    private double _size;
    private readonly FontRegistry _registry;

    public FontItem(int id, FontFamily family, double size, FontRegistry registry)
    {
        Id = id;
        _family = family;
        _size = size;
        _registry = registry;
        
        _registry.WhenAnyValue(x => x.Count) 
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRemove)));
        
        RemoveCommand = ReactiveCommand.CreateFromTask(Remove, _registry.WhenAnyValue(x => x.Count, x => x > 1));
    }

    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
    
    public bool CanRemove => _registry.Count > 1;

    public int Id { get; }
    
    public string Name => $"{Family.Name} {Size}pt";
    
    public IList<FontFamily> SystemOptions => SystemFonts.Instance.Fonts;

    public FontFamily Family
    {
        get => _family;
        set
        {
            this.RaiseAndSetIfChanged(ref _family, value);
            this.RaisePropertyChanged(nameof(Name));
        }
    }

    public double Size
    {
        get => _size;
        set
        {
            this.RaiseAndSetIfChanged(ref _size, value);
            this.RaisePropertyChanged(nameof(Name));
        }
    }
    
    private async Task Remove()
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Remove Font", $"Are you sure you want to remove the font {Name}?", ButtonEnum.YesNo);
        if (await box.ShowAsync() == ButtonResult.Yes) _registry.Remove(this);
    }
}

public class FontRegistry : ReactiveObject
{
    private readonly ObservableCollection<FontItem> _fonts = new();
    
    public FontRegistry()
    {
        Registered = new ReadOnlyObservableCollection<FontItem>(_fonts);
    }
    
    public int Count => _fonts.Count;

    public IList<FontFamily> SystemOptions => SystemFonts.Instance.Fonts;
    
    public ReadOnlyObservableCollection<FontItem> Registered { get; }

    public void Add(int id, string family, double size)
    {
        if (_fonts.Any(x => x.Id == id)) throw new ArgumentException($"There is already a font with the key {id}");
        
        var fontFamily = SystemOptions.FirstOrDefault(x => x.Name == family);
        if (fontFamily == null) throw new ArgumentException($"The font family {family} is not available");
        
        _fonts.Add(new FontItem(id, fontFamily, size, this));
        this.RaisePropertyChanged(nameof(Count));
    }
    
    public void AddNew(FontFamily family, double size)
    {
        var id = NextId();
        var item = new FontItem(id, family, size, this);
        _fonts.Add(item);
        this.RaisePropertyChanged(nameof(Count));
    }

    public void AddNew(string family, double size)
    {
        AddNew(new FontFamily(family), size);
    }
    
    public void Remove(FontItem item)
    {
        if (Count == 1) return;
        
        _fonts.Remove(item);
        this.RaisePropertyChanged(nameof(Count));
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