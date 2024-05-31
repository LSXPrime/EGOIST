using System.Globalization;
using System.Windows.Data;

namespace EGOIST.Helpers;
public class DynamicVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is string) || !((parameter as object).GetType().Equals(ViewModelType)))
        {
            return Visibility.Hidden;
        }

        string converterParameter = (string)parameter;

        // Cast to specific ViewModel type
        var viewModel = Activator.CreateInstance(ViewModelType);
        var getUIVisibilityMethod = viewModel.GetType().GetMethod("GetUIVisiblity");

        if (getUIVisibilityMethod != null)
        {
            // Invoke the specific ViewModel's GetUIVisibility
            return (Visibility)getUIVisibilityMethod.Invoke(viewModel, new object[] { converterParameter });
        }

        return Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    public Type ViewModelType
    {
        get; set;
    }
}