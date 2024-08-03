using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class UnitViewModel : ReactiveObject
{
    private LengthUnit _unit;

    public LengthUnit Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }
    
    public List<EnumOption<LengthUnit>> Options { get; } = EnumSelector.Get<LengthUnit>();

    public double Conversion => Unit switch
    {
        LengthUnit.Millimeter => 1,
        LengthUnit.Inch => 1 / 25.4,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public double MmToUnit(double mm)
    {
        return mm * Conversion;
    }
    
    public double UnitToMm(double unit)
    {
        return unit / Conversion;
    }
}