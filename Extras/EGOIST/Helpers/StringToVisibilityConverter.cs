using System.Globalization;
using System.Windows.Data;

namespace EGOIST.Helpers;
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is string stringValue))
        {
            return DependencyProperty.UnsetValue;
        }

        return stringValue.ToLower() == "visible" ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}