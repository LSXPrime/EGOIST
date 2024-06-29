using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
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
        var dialog = new ContentDialog()
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = cancelButtonText
        };


        return await dialog.ShowAsync();
    }

    public static async Task<T?> CreateDialogAsync<T>(string? title = null, string primaryButtonText = "Create",
        string cancelButtonText = "Cancel") where T : ViewModelBase
    {
        if (Locator == null)
            return null;
        
        var vm = Ioc.Default.GetRequiredService<T>();
        var dialog = new ContentDialog()
        {
            Title = title ?? vm.Title,
            Content = new ViewLocator().Build(vm),
            DataContext = vm,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = cancelButtonText
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary ? vm : null;
    }

    private static Window GetMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow!;
        }

        return null!;
    }

    public static async Task<IEnumerable<string?>> OpenFileDialogAsync(bool allowMultiple = false,
        IReadOnlyList<FilePickerFileType>? fileTypes = null)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        };

        var files = await GetMainWindow().StorageProvider.OpenFilePickerAsync(options);
        var filePaths = files.Select(file => file.TryGetLocalPath());

        return filePaths;
    }
}