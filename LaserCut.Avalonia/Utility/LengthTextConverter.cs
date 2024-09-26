using System.Globalization;
using Avalonia.Data.Converters;
using LaserCut.Avalonia.ViewModels;

namespace LaserCut.Avalonia.Utility;

public class LengthTextConverter : IValueConverter
{
    private readonly UnitViewModel _units;

    public LengthTextConverter(UnitViewModel units)
    {
        _units = units;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
