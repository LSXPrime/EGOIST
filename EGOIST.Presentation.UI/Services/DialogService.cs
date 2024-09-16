using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.ViewModels;
using FluentAvalonia.UI.Controls;

namespace EGOIST.Presentation.UI.Services;

public static class DialogService
{
    private static ViewLocator? Locator { get; set; }

    public static void Initialize(ViewLocator? locator)
    {
        Locator = locator;
    }

    public static async Task<ContentDialogResult?> CreateDialogAsync(string? title = null, string content = "",
        string primaryButtonText = "Create", string cancelButtonText = "Cancel")
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = cancelButtonText
        };


        return await dialog.ShowAsync();
    }

    public static async Task<string> CreateInputDialogAsync(string? title = null, string content = "",
        string primaryButtonText = "Create", string cancelButtonText = "Cancel")
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = content },
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = cancelButtonText
        };


        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return string.Empty;

        return (dialog.Content as TextBlock)?.Text ?? string.Empty;
    }

    public static async Task<T?> CreateDialogAsync<T>(string? title = null, string primaryButtonText = "Create",
        string cancelButtonText = "Cancel") where T : ViewModelBase
    {
        if (Locator == null)
            throw new InvalidOperationException("Locator is not initialized.");

        var vm = Ioc.Default.GetRequiredService<T>();
        var isNavigationAware = vm is INavigationAware;
        var navAware = vm as INavigationAware;

        if (isNavigationAware && navAware != null)
            await navAware.Initialize(null);

        var dialog = new ContentDialog
        {
            Title = title ?? vm.Title,
            Content = Locator.Build(vm),
            DataContext = vm,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = cancelButtonText,
        };

        if (isNavigationAware && navAware != null)
            await navAware.OnNavigatedTo();

        var result = await dialog.ShowAsync();

        if (isNavigationAware && navAware != null)
            await navAware.OnNavigatedFrom();

        return result == ContentDialogResult.Primary ? vm : null;
    }

    private static Window GetMainWindow()
    {
        return Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow!
            : null!;
    }

    public static async Task<IEnumerable<string>> OpenFileDialogAsync(bool allowMultiple = false,
        IReadOnlyList<FilePickerFileType>? fileTypes = null)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = allowMultiple,
            FileTypeFilter = fileTypes
        };

        var files = await GetMainWindow().StorageProvider.OpenFilePickerAsync(options);
        var filePaths = files.Select(file => file.TryGetLocalPath()!);

        return filePaths;
    }

    public static async Task<string?> OpenFolderDialogAsync()
    {
        var path = await GetMainWindow().StorageProvider
            .OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });

        return path.FirstOrDefault()?.Path.AbsolutePath;
    }

    public static async Task LaunchUriAsync(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            await GetMainWindow().Launcher.LaunchUriAsync(uri);
    }
}