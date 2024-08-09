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
    
    public string IdDisplay => $"Font ID: {Id}";
    
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
        if (await box.ShowAsync() == ButtonResult.Yes)
        {
            _registry.Remove(this);
        }
    }
}