using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.VisualBasic;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class TextViewModel : ReactiveObject, IDrawViewModel
{
    private bool _isVisible;
    private ITransform _transform;
    private string _text = string.Empty;
    private TextBlock? _block;

    public TextViewModel(Guid id)
    {
        Id = id;

        this.WhenAnyValue(x => x.Text)
            .Subscribe(_ => UpdateBlock());
    }

    public Guid Id { get; }
    public IBrush? Stroke => null;
    public IBrush? Fill => null;
    public double StrokeThickness { get; set; }


    public TextBlock? Block
    {
        get => _block;
        set => this.RaiseAndSetIfChanged(ref _block, value);
    }
    
    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    public bool IsVisible 
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public double DisplayThickness => 0;
    
    public ITransform Transform
    {
        get => _transform;
        set => this.RaiseAndSetIfChanged(ref _transform, value);
    }
    
    public void UpdateZoom(double zoom) { }

    private void UpdateBlock()
    {
        if (Block is { } block)
        {
            block.Text = Text;
        }
        else
        {
            Block = new TextBlock
            {
                Text = Text,
            };
            
            Block.LayoutUpdated += BlockOnLayoutUpdated;
            
        }
        
    }

    private void BlockOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (Block is { } block)
        {
            Block.RenderTransform = new TranslateTransform(-block.Bounds.Width / 2, -block.Bounds.Height / 2);
        }
    }
}