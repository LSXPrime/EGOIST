using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Text;
using EGOIST.Presentation.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Dialogs;

public partial class TextMemoryCreateViewModel : ViewModelBase
{
    public override string Title => "Create Memory";

    [ObservableProperty] private string? _selectedCollection;

    [ObservableProperty] private string? _documentPath;

    public ObservableCollection<string> MemoriesPaths { get; } = [];

    public TextMemoryCreateViewModel([FromKeyedServices("MemoryService")] ITextService memoryService)
    {
        if (memoryService is MemoryService memory)
            MemoriesPaths = new ObservableCollection<string>(memory.MemoriesPaths.Select(x => x.Name));
    }

    [RelayCommand]
    private async Task OpenFileDialog()
    {
        var filters = new List<FilePickerFileType>
        {
            new("Documents")
            {
                Patterns = new List<string> { "*.pdf", "*.html", "*.htm", "*.md", "*.txt", "*.json" },
                AppleUniformTypeIdentifiers = new List<string>
                    { "public.pdf", "public.html", "public.text", "public.json" },
                MimeTypes = new List<string> { "application/pdf", "text/html", "text/plain", "application/json" }
            },
            new("MS Office Files")
            {
                Patterns = new List<string> { "*.docx", "*.doc", "*.xlsx", "*.xls", "*.pptx", "*.ppt" },
                AppleUniformTypeIdentifiers = new List<string>
                {
                    "com.microsoft.word.doc", "org.openxmlformats.wordprocessingml.document", "com.microsoft.excel.xls",
                    "org.openxmlformats.spreadsheetml.sheet", "com.microsoft.powerpoint.ppt",
                    "org.openxmlformats.presentationml.presentation"
                },
                MimeTypes = new List<string>
                {
                    "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "application/vnd.ms-powerpoint",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                }
            },
            new("All Supported Files")
            {
                Patterns = new List<string>
                {
                    "*.docx", "*.doc", "*.xlsx", "*.xls", "*.pptx", "*.ppt", "*.pdf", "*.html", "*.htm", "*.md",
                    "*.txt", "*.json"
                },
                AppleUniformTypeIdentifiers = new List<string>
                {
                    "public.pdf", "public.html", "public.text", "public.json", "com.microsoft.word.doc",
                    "org.openxmlformats.wordprocessingml.document", "com.microsoft.excel.xls",
                    "org.openxmlformats.spreadsheetml.sheet",
                    "com.microsoft.powerpoint.ppt", "org.openxmlformats.presentationml.presentation"
                },
                MimeTypes = new List<string>
                {
                    "application/pdf", "text/html", "text/plain", "application/json", "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "application/vnd.ms-powerpoint",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                }
            }
        };

        var openFileDialog = await DialogService.OpenFileDialogAsync(false, filters);
        DocumentPath = openFileDialog.FirstOrDefault();
    }
}