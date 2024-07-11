using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Presentation.UI.Services;

namespace EGOIST.Presentation.UI.Helpers;

public class NavigationHeaderConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var header = NavigationService.Current.Main.Title;
        if (NavigationService.Current.Sub != null)
            header += $" - {NavigationService.Current.Sub.Title}";
        if (NavigationService.Current.Nested != null)
            header += $" - {NavigationService.Current.Nested.Peek().Title}";
        

        return header;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
