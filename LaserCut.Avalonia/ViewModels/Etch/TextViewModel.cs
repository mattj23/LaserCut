using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LaserCut.Avalonia.Models;
using LaserCut.Geometry;
using LaserCut.Text;
using ReactiveUI;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class TextViewModel : EtchEntityViewModelBase
{
    private string _text = string.Empty;
    private TextBlock? _block;
    private EtchAlign _horizontal;
    private EtchAlign _vertical;
    private Xyr _fullXyr;
    private double _fontSize = 12;
    private Rect _bounds;

    private FontFamily _font;

    public TextViewModel(Guid id, UnitViewModel unit) : base(id)
    {
        Font = FontManager.Current.DefaultFontFamily;
        
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
    
    public double FontSize
    {
        get => _fontSize;
        set => this.RaiseAndSetIfChanged(ref _fontSize, value);
    }
    
    public Xyr FullXyr => _fullXyr;
    
    public override void UpdateZoom(double zoom) { }


    public override void OnParentXyrChanged()
    {
        UpdateTransform();
    }
    
    private void UpdateTransform()
    {
        if (Block is null) return;
        
        var align = AlignmentTransform();

        var fullMatrix = (Matrix)(ParentXyr.AsMatrix() * XyrVm.CurrentXyr.AsMatrix());
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