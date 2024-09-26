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
        LengthFormat = GetLengthFormat();
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

    public string LengthFormat
    {
        get => _lengthFormat;
        set => this.RaiseAndSetIfChanged(ref _lengthFormat, value);
    }

    public double GetValueMm()
    {
        return _valueMm;
    }

    private void OnUnitChange()
    {
        LengthFormat = GetLengthFormat();
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

    private string GetLengthFormat()
    {
        return Units.Unit switch
        {
            LengthUnit.Millimeter => "0.00 mm",
            LengthUnit.Inch => "0.000 in",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

}
