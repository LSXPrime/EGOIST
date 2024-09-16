using System;
using Avalonia;
using CommunityToolkit.Mvvm.DependencyInjection;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Image;
using EGOIST.Application.Services.Management;
using EGOIST.Application.Services.Management.Loaders;
using EGOIST.Application.Services.Text;
using EGOIST.Application.Services.Text.Roleplay;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Services.Voice;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using EGOIST.Infrastructure.Repositories;
using EGOIST.Infrastructure.Services.FileTypes;
using EGOIST.Infrastructure.Services.Network;
using EGOIST.Infrastructure.Services.RAG;
using EGOIST.Infrastructure.Services.Storage;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.Services.Utilities;
using EGOIST.Presentation.UI.ViewModels;
using EGOIST.Presentation.UI.ViewModels.Dialogs;
using EGOIST.Presentation.UI.ViewModels.Pages;
using EGOIST.Presentation.UI.ViewModels.Pages.Image;
using EGOIST.Presentation.UI.ViewModels.Pages.Management;
using EGOIST.Presentation.UI.ViewModels.Pages.Management.Characters;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;
using EGOIST.Presentation.UI.Views;
using EGOIST.Presentation.UI.Views.Dialogs;
using EGOIST.Presentation.UI.Views.Pages;
using EGOIST.Presentation.UI.Views.Pages.Image;
using EGOIST.Presentation.UI.Views.Pages.Management;
using EGOIST.Presentation.UI.Views.Pages.Management.Characters;
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
            .AddKeyedScoped<IModelCoreService,
                TextModelCoreService>("TextModelCoreScopedService") // for independent models services eg. memory
            .AddKeyedSingleton<IModelCoreService, TextModelCoreService>("TextModelCoreService")
            .AddKeyedSingleton<IModelCoreService, VoiceModelCoreService>("VoiceModelCoreService")
            .AddKeyedSingleton<IModelCoreService, ImageModelCoreService>("ImageModelCoreService")
            .AddKeyedSingleton<IImageService, ImagineService>("ImagineService")
            .AddKeyedSingleton<IImageService, TransformService>("TransformService")
            .AddKeyedSingleton<IImageService, UpscaleService>("UpscaleService")
            .AddKeyedSingleton<ITextService<ChatSession>, ChatService>("ChatService")
            .AddKeyedSingleton<ITextService<CompletionSession>, CompletionService>("CompletionService")
            .AddKeyedSingleton<ITextService<RoleplaySession>, RoleplayService>("RoleplayService")
            .AddKeyedSingleton<IModelsRepository, LocalModelsRepository>("LocalModelsRepository")
            .AddKeyedSingleton<IModelsRepository, HuggingFaceModelsRepository>("HuggingFaceModelsRepository")
            .AddKeyedSingleton<IModelsRepository, CivitAiModelsRepository>("CivitAIModelsRepository")
            .AddScoped<INetworkService, NetworkService>()
            .AddScoped<IImageHelper, ImageHelper>()
            .AddSingleton<IImageMetadataService, ImageMetadataService>()
            .AddSingleton<IFileSystemService, FileSystemService>()
            .AddSingleton<IPromptRepository<TextPromptParameters>, LocalPromptRepository<TextPromptParameters>>()
            .AddSingleton<IHistoryRepository, LocalHistoryRepository>()
            .AddSingleton<ICharacterRepository, LocalCharacterRepository>()
            .AddSingleton<IWorldMemoryRepository, LocalWorldMemoryRepository>()
            .AddSingleton<WorldMemoryService>()
            .AddSingleton<CharacterService>()
            .AddSingleton<IRagMemory, KernelRagMemory>()
            .AddSingleton<MemoryService>()
            .AddSingleton<TextDataLoader>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<MainWindow>()
            .AddSingleton<HomePageViewModel>()
            .AddTransient<HomePageView>()
            .AddSingleton<ImagePageViewModel>()
            .AddTransient<ImagePageView>()
            .AddSingleton<ImaginePageViewModel>()
            .AddTransient<ImaginePageView>()
            .AddSingleton<TransformPageViewModel>()
            .AddTransient<TransformPageView>()
            .AddSingleton<UpscalePageViewModel>()
            .AddTransient<UpscalePageView>()
            .AddSingleton<TextPageViewModel>()
            .AddTransient<TextPageView>()
            .AddSingleton<ChatPageViewModel>()
            .AddTransient<ChatPageView>()
            .AddSingleton<CompletionPageViewModel>()
            .AddTransient<CompletionPageView>()
            .AddSingleton<MemoryPageViewModel>()
            .AddTransient<MemoryPageView>()
            .AddSingleton<RoleplayPageViewModel>()
            .AddTransient<RoleplayPageView>()
            .AddSingleton<SettingsPageViewModel>()
            .AddTransient<SettingsPageView>()
            .AddSingleton<CharactersPageViewModel>()
            .AddTransient<CharactersPageView>()
            .AddTransient<PreviewPageViewModel>()
            .AddTransient<PreviewPageView>()
            .AddTransient<CreatePageViewModel>()
            .AddTransient<CreatePageView>()
            .AddTransient<TextMemoryCreateViewModel>()
            .AddTransient<TextMemoryCreateView>()
            .AddTransient<TextRoleplayCreateViewModel>()
            .AddTransient<TextRoleplayCreateView>()
            .AddTransient<TextRoleplayWorldMemoryViewModel>()
            .AddTransient<TextRoleplayWorldMemoryView>()
            .BuildServiceProvider();
        Ioc.Default.ConfigureServices(provider);
        AppConfig.Instance.Load();
        BackendService.Instance.Initialize();
        DialogService.Initialize(new ViewLocator());

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .With(new Win32PlatformOptions
            {
                RenderingMode = AppConfig.Instance.Parameters.HardwareAcceleration
                    ? [Win32RenderingMode.AngleEgl, Win32RenderingMode.Vulkan, Win32RenderingMode.Software]
                    : [Win32RenderingMode.Software]
            })
            .With(new AvaloniaNativePlatformOptions
            {
                RenderingMode = AppConfig.Instance.Parameters.HardwareAcceleration
                    ? [AvaloniaNativeRenderingMode.OpenGl, AvaloniaNativeRenderingMode.Software]
                    : [AvaloniaNativeRenderingMode.Software]
            })
            .LogToTrace();
}