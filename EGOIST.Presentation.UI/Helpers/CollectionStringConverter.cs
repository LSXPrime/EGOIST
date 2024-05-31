using System;
using Avalonia.Data.Converters;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace EGOIST.Presentation.UI.Helpers;

public class CollectionStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<string> list)
            return string.Join(", ", list);
        
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return null;
        var items = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .ToList();
        return new ObservableCollection<string>(items);
    }
}