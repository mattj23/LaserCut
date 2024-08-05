using LaserCut.Geometry;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class XyrViewModel : ReactiveObject
{
    private readonly UnitViewModel _units;
    private double _xMm;
    private double _yMm;
    private double _rRad;
    private double _r;
    private double _x;
    private double _y;

    private string _lengthFormat;

    public XyrViewModel(UnitViewModel units, bool hasR)
    {
        _units = units;
        HasR = hasR;
        _units.WhenAnyValue(x => x.Unit)
            .Subscribe(_ => OnUnitChange());
        LengthFormat = GetLengthFormat();
    }
    
    public Action<double, double, double>? OnEditedValuesAction { get; set; }
    
    public bool HasR { get; }
    
    public double XMm => _xMm;
    
    public double YMm => _yMm;
    
    public double RRad => _rRad;
    
    /// <summary>
    /// Gets or sets the X coordinate in the currently active length unit.
    /// </summary>
    public double X
    {
        get => _x;
        set
        {
            if (Math.Abs(value - _x) < 1e-12) return;
            _x = value;
            _xMm = _units.UnitToMm(value);
            OnNewEditValues(_xMm, _yMm, _rRad);
            this.RaisePropertyChanged();
        }
    }
    
    /// <summary>
    /// Gets or sets the Y coordinate in the currently active length unit.
    /// </summary>
    public double Y
    {
        get => _y;
        set
        {
            if (Math.Abs(value - _y) < 1e-12) return;
            _y = value;
            _yMm = _units.UnitToMm(value);
            OnNewEditValues(_xMm, _yMm, _rRad);
            this.RaisePropertyChanged();
        }
    }
    
    /// <summary>
    /// Gets or sets the rotation in degrees.
    /// </summary>
    public double R
    {
        get => _r;
        set
        {
            if (Math.Abs(value - _r) < 1e-12) return;
            _r = value;
            _rRad = double.DegreesToRadians(value);
            OnNewEditValues(_xMm, _yMm, _rRad);
            this.RaisePropertyChanged();
        }
    }
    
    public Xyr CurrentXyr => new(_xMm, _yMm, _rRad);
    
    public string LengthFormat
    {
        get => _lengthFormat;
        set => this.RaiseAndSetIfChanged(ref _lengthFormat, value);
    }

    private void OnUnitChange()
    {
        LengthFormat = GetLengthFormat();
        _x = _units.MmToUnit(_xMm);
        _y = _units.MmToUnit(_yMm);
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }
    
    private string GetLengthFormat()
    {
        return _units.Unit switch
        {
            LengthUnit.Millimeter => "0.00 mm",
            LengthUnit.Inch => "0.000 in",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public void SetValues(double xMm, double yMm, double r)
    {
        _xMm = xMm;
        _yMm = yMm;
        _rRad = r;
        _r = double.RadiansToDegrees(r);
        _x = _units.MmToUnit(xMm);
        _y = _units.MmToUnit(yMm);
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(R));
    }
    
    protected virtual void OnNewEditValues(double xMm, double yMm, double r)
    {
        OnEditedValuesAction?.Invoke(xMm, yMm, r);
    }
    
}