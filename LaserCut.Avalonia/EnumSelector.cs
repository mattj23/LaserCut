using System.ComponentModel;
using System.Reflection;

namespace LaserCut.Avalonia;

public class EnumOption<T> where T : Enum
{
    public EnumOption(string name, T value)
    {
        Name = name;
        Value = value;
    }
    
    public string Name { get; }
    
    public T Value { get; }
}

public static class EnumSelector
{
    public static List<EnumOption<T>> Get<T>() where T : Enum
    {
        var options = new List<EnumOption<T>>();
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            options.Add(new EnumOption<T>(GetEnumDescription((Enum)value), (T)value));
        }

        return options;
    }
    
    public static string GetEnumDescription(Enum value)
    {
        var info = value.GetType().GetField(value.ToString());
        var custom = info?.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (custom is DescriptionAttribute[] attributes && attributes.Length != 0)
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}