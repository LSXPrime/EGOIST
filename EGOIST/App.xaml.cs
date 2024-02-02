using System.IO;
using System.Reflection;
using System.Windows.Threading;
using EGOIST.Data;
using EGOIST.Helpers;
using EGOIST.Services;
using EGOIST.ViewModels.Pages;
using EGOIST.ViewModels.Windows;
using EGOIST.Views.Pages;
using EGOIST.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EGOIST;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();

            services.AddSingleton<HomePage>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<TextPage>();
            services.AddSingleton<TextViewModel>();
            services.AddSingleton<VoicePage>();
            services.AddSingleton<VoiceViewModel>();
            services.AddSingleton<ManagementPage>();
            services.AddSingleton<ManagementViewModel>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<SettingsViewModel>();
        }).Build();

    public static T GetService<T>()
        where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();
        GetService<ManagementViewModel>().LoadData();
        GetService<TextViewModel>().OnStartup();
        GetService<SettingsViewModel>().OnStartup();
        _ = GetService<SettingsViewModel>().CheckForUpdate();
        _ = SystemInfo.Instance.Montitor();
        Extensions.LoadData();

        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("EGOIST_Handler.log")
                .CreateLogger();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        Extensions.SaveData();
        Log.CloseAndFlush();
        await _host.StopAsync();
        _host.Dispose();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Extensions.Notify("Core", $"OnDispatcherUnhandledException: {e.Exception.Message}");
    }
}
