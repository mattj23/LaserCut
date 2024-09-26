using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Media;
using Wacton.Unicolour;

namespace LaserCut.Avalonia.Utility;

public class ColorSpace
{
    private readonly Unicolour _start;
    private readonly Unicolour _mid;
    private readonly Unicolour _end;
    private int _count = 2;
    private readonly Subject<Unit> _rangeUpdate = new();

    public ColorSpace()
    {
        _start = new Unicolour(ColourSpace.Rgb255, 255, 215, 0);
        _mid = new Unicolour(ColourSpace.Rgb255, 255, 61, 117);
        _end = new Unicolour(ColourSpace.Rgb255, 102, 51, 153);

        _count = 35 / 5;
    }

    public int Count => _count;

    public IObservable<Unit> RangeUpdate => _rangeUpdate.AsObservable();

    public Unicolour GetColor(int index)
    {
        if (index + 1 > _count)
        {
            _count = index + 1;
            Console.WriteLine($"Updating color count to {_count}");
            _rangeUpdate.Next();
        }

        var fraction = index / (_count - 1.0);
        if (fraction < 0.5) return _start.Mix(_mid, ColourSpace.Rgb255, fraction * 2);

        return _mid.Mix(_end, ColourSpace.Rgb255, (fraction - 0.5) * 2);
    }

}

public static class ColorExtensions
{
    public static IBrush ToAvaloniaBrush(this Unicolour unicolour)
    {
        var (r, g, b) = unicolour.GetRepresentation(ColourSpace.Rgb255).Triplet;
        return new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    public static System.Drawing.Color ToDrawingColor(this Unicolour unicolour)
    {
        var (r, g, b) = unicolour.GetRepresentation(ColourSpace.Rgb255).Triplet;
        return System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
    }

    public static string ToHexString(this Unicolour unicolour)
    {
        var (r, g, b) = unicolour.GetRepresentation(ColourSpace.Rgb255).Triplet;
        return $"#ff{(int)r:x2}{(int)g:x2}{(int)b:x2}";
    }
}
