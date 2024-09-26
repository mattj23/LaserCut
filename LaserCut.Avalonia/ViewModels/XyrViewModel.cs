using LaserCut.Geometry;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class XyrViewModel : ReactiveObject
{
    private double _rRad;
    private double _r;

    public XyrViewModel(UnitViewModel units, bool hasR)
    {
        HasR = hasR;

        XEdit = new LengthEditViewModel(units).WithDefaultSettings();
        YEdit = new LengthEditViewModel(units).WithDefaultSettings();

        XEdit.ValueChanged.Subscribe(_ => OnNewEditValues(XEdit.GetValueMm(), YEdit.GetValueMm(), _rRad));
        YEdit.ValueChanged.Subscribe(_ => OnNewEditValues(XEdit.GetValueMm(), YEdit.GetValueMm(), _rRad));
    }

    public Action<double, double, double>? OnEditedValuesAction { get; set; }

    public LengthEditViewModel XEdit { get; }
    public LengthEditViewModel YEdit { get; }

    public bool HasR { get; }

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
            OnNewEditValues(XEdit.GetValueMm(), YEdit.GetValueMm(), _rRad);
            this.RaisePropertyChanged();
        }
    }

    public Xyr CurrentXyr => new(XEdit.GetValueMm(), YEdit.GetValueMm(), _rRad);

    /// <summary>
    /// Set the values of the X, Y, and R properties, where X and Y are in millimeters and R is in radians.
    /// </summary>
    /// <param name="xMm"></param>
    /// <param name="yMm"></param>
    /// <param name="r"></param>
    public void SetValues(double xMm, double yMm, double r)
    {
        XEdit.SetValue(xMm, false);
        YEdit.SetValue(yMm, false);
        _rRad = r;
        _r = double.RadiansToDegrees(r);
        this.RaisePropertyChanged(nameof(R));
        OnNewEditValues(XEdit.GetValueMm(), YEdit.GetValueMm(), _rRad);
    }

    protected virtual void OnNewEditValues(double xMm, double yMm, double r)
    {
        OnEditedValuesAction?.Invoke(xMm, yMm, r);
    }

}
