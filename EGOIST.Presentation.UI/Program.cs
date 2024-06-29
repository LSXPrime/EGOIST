using System;
using Avalonia;
using CommunityToolkit.Mvvm.DependencyInjection;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Image;
using EGOIST.Application.Services.Text;
using EGOIST.Application.Services.Voice;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using EGOIST.Infrastructure.Repositories;
using EGOIST.Infrastructure.Services.RAG;
using EGOIST.Presentation.UI.ViewModels;
using EGOIST.Presentation.UI.ViewModels.Dialogs;
using EGOIST.Presentation.UI.ViewModels.Pages;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;
using EGOIST.Presentation.UI.Views;
using EGOIST.Presentation.UI.Views.Dialogs;
using EGOIST.Presentation.UI.Views.Pages;
using EGOIST.Presentation.UI.Views.Pages.Text;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EGOIST.Presentation.UI;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File("EGOIST_Handler.log", rollingInterval: RollingInterval.Month)
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();

        var services = new ServiceCollection();
        var provider = services
            .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
            .AddKeyedSingleton<IModelCoreService, TextModelCoreService>("TextModelCoreService")
            .AddKeyedSingleton<IModelCoreService, VoiceModelCoreService>("VoiceModelCoreService")
            .AddKeyedSingleton<IModelCoreService, ImageModelCoreService>("ImageModelCoreService")
            .AddKeyedSingleton<ITextService, ChatService>("ChatService")
            .AddKeyedSingleton<ITextService, CompletionService>("CompletionService")
            .AddKeyedSingleton<ITextService, RoleplayService>("RoleplayService")
            .AddKeyedSingleton<ITextService, MemoryService>("MemoryService")
            .AddKeyedSingleton<IModelsRepository, LocalModelsRepository>("LocalModelsRepository")
            .AddKeyedSingleton<IModelsRepository, HuggingFaceModelsRepository>("HuggingFaceModelsRepository")
            .AddKeyedSingleton<IModelsRepository, CivitAIModelsRepository>("CivitAIModelsRepository")
            .AddSingleton<IPromptRepository<TextPromptParameters>, LocalPromptRepository<TextPromptParameters>>()
            .AddSingleton<IRagMemory, KernelRagMemory>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<MainWindow>()
            .AddTransient<HomePageViewModel>()
            .AddTransient<HomePageView>()
            .AddTransient<TextPageViewModel>()
            .AddTransient<TextPageView>()
            .AddTransient<ChatPageViewModel>()
            .AddTransient<ChatPageView>()
            .AddTransient<CompletionPageViewModel>()
            .AddTransient<CompletionPageView>()
            .AddTransient<MemoryPageViewModel>()
            .AddTransient<MemoryPageView>()
            .AddTransient<TextMemoryCreateViewModel>()
            .AddTransient<TextMemoryCreateView>()
            .BuildServiceProvider();
        Ioc.Default.ConfigureServices(provider);
        Services.DialogService.Initialize(new ViewLocator());
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}