using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using LaserCut.Avalonia.Models;
using LaserCut.Geometry;
using LaserCut.Text;
using ReactiveUI;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;

namespace LaserCut.Avalonia.ViewModels;

public class TextViewModel : ReactiveObject, IDrawViewModel
{
    private bool _isVisible;
    private string _text = string.Empty;
    private TextBlock? _block;
    private EtchAlign _horizontal;
    private EtchAlign _vertical;
    private Xyr _parentXyr;
    private Xyr _fullXyr;
    private double _fontSize = 12;
    private Rect _bounds;

    private FontFamily _font;

    public TextViewModel(Guid id, UnitViewModel unit)
    {
        Font = FontManager.Current.DefaultFontFamily;
        
        Id = id;
        XyrVm = new XyrViewModel(unit, true)
        {
            OnEditedValuesAction = (_, _, _) => UpdateTransform()
        };

        this.WhenAnyValue(x => x.Text, x => x.FontSize, x => x.Font)
            .Subscribe(_ => SetBlockProperties());
        
        this.WhenAnyValue(x => x.Horizontal, x => x.Vertical)
            .Subscribe(_ => UpdateTransform());
    }

    public List<EnumOption<EtchAlign>> AlignOptions { get; } = EnumSelector.Get<EtchAlign>();

    public Guid Id { get; }
    public IBrush? Stroke => null;
    public IBrush? Fill => null;
    public double StrokeThickness { get; set; }

    public IList<FontFamily> FontOptions => SystemFonts.Instance.Fonts;
    
    public FontFamily Font
    {
        get => _font;
        set => this.RaiseAndSetIfChanged(ref _font, value);
    }
    
    public XyrViewModel XyrVm { get; }

    public EtchAlign Horizontal
    {
        get => _horizontal;
        set => this.RaiseAndSetIfChanged(ref _horizontal, value);
    }

    public EtchAlign Vertical
    {
        get => _vertical;
        set => this.RaiseAndSetIfChanged(ref _vertical, value);
    }
    
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
    
    public double FontSize
    {
        get => _fontSize;
        set => this.RaiseAndSetIfChanged(ref _fontSize, value);
    }
    
    public void UpdateZoom(double zoom) { }

    public void UpdateParentXyr(Xyr xyr)
    {
        _parentXyr = xyr;
        UpdateTransform();
    }
    
    public Xyr FullXyr => _fullXyr;

    private void UpdateTransform()
    {
        if (Block is null) return;
        
        var align = AlignmentTransform();

        var fullMatrix = (Matrix)(_parentXyr.AsMatrix() * XyrVm.CurrentXyr.AsMatrix());
        _fullXyr = Xyr.FromMatrix(fullMatrix);
        
        var full = new MatrixTransform { Matrix = fullMatrix.ToAvalonia() };
        var transform = new TransformGroup { Children = [ align, full, ] };

        Block.RenderTransform = transform;
    }

    private TranslateTransform AlignmentTransform()
    {
        var sx = Horizontal switch
        {
            EtchAlign.Near => 0,
            EtchAlign.Center => -_bounds.Width/2,
            EtchAlign.Far => -_bounds.Width,
            _ => 0
        };
        
        var sy = Vertical switch
        {
            EtchAlign.Near => 0,
            EtchAlign.Center => -_bounds.Height/2,
            EtchAlign.Far => -_bounds.Height,
            _ => 0
        };

        return new TranslateTransform(sx, sy);
    }

    private void SetBlockProperties()
    {
        if (Block is null)
        {
            Block = new TextBlock
            {
                RenderTransformOrigin = RelativePoint.TopLeft,
                Foreground = Brushes.DodgerBlue,
            };
            Block.WhenAnyValue(x => x.Bounds)
                .Subscribe(OnBoundsUpdate);
        }
        
        Block.Text = Text;
        Block.FontFamily = Font;
        Block.FontSize = FontSize * 0.264583;
    }

    private void OnBoundsUpdate(Rect b)
    {
        _bounds = b;
        UpdateTransform();
    }

}