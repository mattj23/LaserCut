using Avalonia.Media;

namespace LaserCut.Avalonia.Models;

public class SystemFonts
{
    private SystemFonts()
    {
        Fonts = FontManager.Current.SystemFonts
            .OrderBy(x => x.Name)
            .ToList();
    }
    
    public IList<FontFamily> Fonts { get; } 
    
    public static SystemFonts Instance { get; } = new();
}