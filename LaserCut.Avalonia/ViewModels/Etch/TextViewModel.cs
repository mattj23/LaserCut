using System.Globalization;
using Avalonia;
using Avalonia.Media;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using LaserCut.Text;
using MathNet.Spatial.Euclidean;
using ReactiveUI;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;
using AvaloniaGeometry = Avalonia.Media.Geometry;

namespace LaserCut.Avalonia.ViewModels.Etch;

public class TextViewModel : EtchEntityViewModelBase
{
    private string _text = string.Empty;
    private EtchAlign _horizontal;
    private EtchAlign _vertical;
    private AvaloniaGeometry? _geometry;
    private ITransform? _transform;
    private FontItem _font;

    private double _sx;
    private double _sy;
    private Xyr _fullXyr;
    private Matrix _localMatrix = Isometry2.Identity();
    private BoundaryLoop? _loop;
    
    public TextViewModel(Guid id, UnitViewModel unit, FontRegistry fonts) : base(id)
    {
        FontOptions = fonts;
        Font = FontOptions.Registered.FirstOrDefault()!;
        
        XyrVm = new XyrViewModel(unit, true)
        {
            OnEditedValuesAction = (_, _, _) => OnGeometryUpdated()
        };

        this.WhenAnyValue(x => x.Text, x => x.Font)
            .Subscribe(_ => SetBlockProperties());
        
        this.WhenAnyValue(x => x.Horizontal, x => x.Vertical)
            .Subscribe(_ => OnGeometryUpdated());
        
        Font.WhenAnyValue(x => x.Family, x => x.Size)
            .Subscribe(_ => SetBlockProperties());
        
        FontOptions.Removed.Subscribe(_ => VerifyFont());
        
        SetBlockProperties();
    }
    
    public FontRegistry FontOptions { get; }

    public List<EnumOption<EtchAlign>> AlignOptions { get; } = EnumSelector.Get<EtchAlign>();

    public FontItem Font
    {
        get => _font;
        set => this.RaiseAndSetIfChanged(ref _font, value);
    }
    
    public RelativePoint TransformOrigin => RelativePoint.TopLeft;
    
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

    public AvaloniaGeometry? Geometry
    {
        get => _geometry;
        set => this.RaiseAndSetIfChanged(ref _geometry, value);
    }
    
    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
    
    public ITransform? Transform
    {
        get => _transform;
        set => this.RaiseAndSetIfChanged(ref _transform, value);
    }
    
    public Xyr FullXyr => _fullXyr;
    
    public override void UpdateZoom(double zoom) { }

    public override Aabb2 Bounds => _loop?.Bounds ?? Aabb2.Empty;

    public override void UpdateHitGeometry()
    {
        SetBlockProperties();
    }

    public override bool Hit(Point2D point)
    {
        if (_loop is null) return false;
        return _loop.Encloses(point);
    }

    public override void OnParentXyrChanged()
    {
        CalculateTransform();
    }

    protected void VerifyFont()
    {
        if (!FontOptions.Registered.Contains(Font))
        {
            Font = FontOptions.Registered.FirstOrDefault()!;
        }
    }
    
    private void OnGeometryUpdated()
    {
        // Calculate the alignment shifts
        var x0 = Geometry?.Bounds.Left ?? 0;
        _sx = Horizontal switch
        {
            EtchAlign.Near => -x0,
            EtchAlign.Center => -x0 - GeomWidth/2,
            EtchAlign.Far => -x0 - GeomWidth,
            _ => 0
        };
        
        var y0 = Geometry?.Bounds.Top ?? 0;
        _sy = Vertical switch
        {
            EtchAlign.Near => -y0,
            EtchAlign.Center => -y0 - GeomHeight/2,
            EtchAlign.Far => -y0 - GeomHeight,
            _ => 0
        };

        _localMatrix = (Matrix)(XyrVm.CurrentXyr.AsMatrix() * Isometry2.Translate(_sx, _sy));
        if (_geometry is not null)
        {
            _loop = new BoundaryLoop();
            var cursor = _loop.GetCursor();
            cursor.SegAbs(_geometry.Bounds.TopRight.ToPoint2D().Transformed(_localMatrix));
            cursor.SegAbs(_geometry.Bounds.TopLeft.ToPoint2D().Transformed(_localMatrix));
            cursor.SegAbs(_geometry.Bounds.BottomLeft.ToPoint2D().Transformed(_localMatrix));
            cursor.SegAbs(_geometry.Bounds.BottomRight.ToPoint2D().Transformed(_localMatrix));
        }
        CalculateTransform();
        
        NotifyChange();
    }
    
    private void CalculateTransform()
    {
        var align = new TranslateTransform(_sx, _sy);

        var fullMatrix = (Matrix)(ParentXyr.AsMatrix() * XyrVm.CurrentXyr.AsMatrix());
        _fullXyr = Xyr.FromMatrix(fullMatrix);
        
        var full = new MatrixTransform { Matrix = fullMatrix.ToAvalonia() };
        var transform = new TransformGroup { Children = [ align, full, ] };

        Transform = transform;
    }
    
    private double GeomWidth => _geometry?.Bounds.Width ?? 0;
    private double GeomHeight => _geometry?.Bounds.Height ?? 0;

    private void SetBlockProperties()
    {
        var fmt = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, 
            new Typeface(Font.Family), Font.Size * 1.33333 * 0.264583, Brushes.Black);
        Geometry = fmt.BuildGeometry(new Point(0, 0));
        OnGeometryUpdated();
    }

}