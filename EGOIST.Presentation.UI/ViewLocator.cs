using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.DependencyInjection;
using EGOIST.Presentation.UI.ViewModels;

namespace EGOIST.Presentation.UI
{
    public class ViewLocator : IDataTemplate
    {

        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type == null) return new TextBlock { Text = "View Not Found : " + name };
            var control = (Control)(Design.IsDesignMode ? Activator.CreateInstance(type) : Ioc.Default.GetService(type)!)!;
            control.DataContext = data;
            return control;
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
