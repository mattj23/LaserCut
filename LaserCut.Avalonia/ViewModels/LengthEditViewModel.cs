using System.Reactive.Subjects;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class LengthEditViewModel : ReactiveObject
{
    private double _valueMm;
    private double _value;
    private string _lengthFormat;
    private Subject<double> _valueChanged = new();

    public LengthEditViewModel(UnitViewModel units)
    {
        Units = units;
        Units.WhenAnyValue(x => x.Unit)
            .Subscribe(_ => OnUnitChange());
    }

    public IObservable<double> ValueChanged => _valueChanged;

    public UnitViewModel Units { get; }

    public double Value
    {
        get => _value;
        set
        {
            if (Math.Abs(value - _value) < 1e-12) return;
            _value = value;
            _valueMm = Units.UnitToMm(value);
            _valueChanged.OnNext(_valueMm);
            this.RaisePropertyChanged();
        }
    }

    public LengthEditViewModel WithDefaultSettings()
    {
        Increment ??= new Dictionary<LengthUnit, double>
        {
            {LengthUnit.Millimeter, 1},
            {LengthUnit.Inch, 0.1}
        };

        DecimalPlaces ??= new Dictionary<LengthUnit, int>
        {
            {LengthUnit.Millimeter, 2},
            {LengthUnit.Inch, 3}
        };

        return this;
    }

    public IDictionary<LengthUnit, double>? Increment { get; set; }

    public IDictionary<LengthUnit, int>? DecimalPlaces { get; set; }

    /// <summary>
    /// Get the number of decimal places for the editor based on the current unit.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int GetDecimalPlaces()
    {
        if (DecimalPlaces is {} dec) return dec[Units.Unit];
        return Units.Unit switch
        {
            LengthUnit.Millimeter => 2,
            LengthUnit.Inch => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Get the increment for the editor based on the current unit, returning a value which is *in the current unit*.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public double GetIncrement()
    {
        if (Increment is {} inc) return inc[Units.Unit];
        return Units.Unit switch
        {
            LengthUnit.Millimeter => 1,
            LengthUnit.Inch => 0.1,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    public double GetValueMm()
    {
        return _valueMm;
    }

    private void OnUnitChange()
    {
        _value = Units.MmToUnit(_valueMm);
        this.RaisePropertyChanged(nameof(Value));
    }

    public void SetValue(double valueMm)
    {
        _valueMm = valueMm;
        _value = Units.MmToUnit(valueMm);
        _valueChanged.OnNext(_valueMm);
        this.RaisePropertyChanged(nameof(Value));
    }

}
