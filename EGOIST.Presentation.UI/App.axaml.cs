using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EGOIST.Presentation.UI.ViewModels;
using EGOIST.Presentation.UI.Views;
using HotAvalonia;
using AvaloniaApp = Avalonia.Application;

namespace EGOIST.Presentation.UI;

public partial class App : AvaloniaApp
{
    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}