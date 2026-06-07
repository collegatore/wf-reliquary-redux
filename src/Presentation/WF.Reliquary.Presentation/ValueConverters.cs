using System.Globalization;
using Avalonia.Data.Converters;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Converts an enum value to bool by comparing it with <c>ConverterParameter</c>.
/// Supports two-way binding: converting back returns the enum value parsed from
/// the parameter (only when the incoming value is <c>true</c>).
/// </summary>
public sealed class EnumToBoolConverter : IValueConverter
{
    public static readonly EnumToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string s && targetType.IsEnum)
        {
            return Enum.Parse(targetType, s);
        }

        return Avalonia.Data.BindingOperations.DoNothing;
    }
}

/// <summary>
/// Returns <c>true</c> when the bound string is non-null and non-empty.
/// </summary>
public sealed class NonEmptyStringToBoolConverter : IValueConverter
{
    public static readonly NonEmptyStringToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
