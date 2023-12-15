using System.Globalization;
using System.Windows.Data;

namespace EGOIST.Helpers;
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string enumString)
            throw new ArgumentException("Exception: Enum parameter isn't string");

        if (value == null || parameter == null || !Enum.IsDefined(value.GetType(), value))
            return DependencyProperty.UnsetValue;

        var enumValue = Enum.Parse(value.GetType(), enumString);
        return enumValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string enumString)
            throw new ArgumentException("Exception: Enum parameter isn't string");

        return Enum.Parse(value.GetType(), enumString);
    }
}
