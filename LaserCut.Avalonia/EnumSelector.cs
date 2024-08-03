namespace LaserCut.Avalonia;

public record EnumOption<T>(string Name, T Value) where T : Enum;

public static class EnumSelector
{
    public static List<EnumOption<T>> Get<T>() where T : Enum
    {
        var options = new List<EnumOption<T>>();
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            options.Add(new EnumOption<T>(value.ToString(), (T)value));
        }

        return options;
    }
    
}