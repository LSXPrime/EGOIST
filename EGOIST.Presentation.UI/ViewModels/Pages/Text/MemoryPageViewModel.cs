using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Services.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Interactions;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YoutubeExplode;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class MemoryPageViewModel : ViewModelBase, INavigationAware, ITextViewModel
{
    [ObservableProperty] private GenerationState _state = GenerationState.None;

    public override string Title => "Memory";

    #region GenerationVariables

    [ObservableProperty] private MemorySource? _selectedMemory;
    /*
    {
        Name = "Gowther: Between The Arms of Sinners",
        IsLoaded = true,
        Documents = new ObservableCollection<Citation>
        {
            new()
            {
                Title = "Chapter 1: The Beginning",
                Path = "https://www.gutenberg.org/files/1661/1661-0.txt",
                Collection = "Gowther: Between The Arms of Sinners"
            },
            new()
            {
                Title = "Chapter 2: The Middle",
                Path = "https://www.gutenberg.org/files/1662/1662-0.txt",
                Collection = "Gowther: Between The Arms of Sinners"
            },
            new()
            {
                Title = "Chapter 3: The End",
                Path = "https://www.gutenberg.org/files/1663/1663-0.txt",
                Collection = "Gowther: Between The Arms of Sinners"
            },
            new()
            {
                Title = "El-Joker: 12 Years Rap",
                Path = "https://www.youtube.com/watch?v=XYT1j0ykq_U",
                Collection = "Gowther: Between The Arms of Sinners"
            },
            new()
            {
                Title = "Image Prompt",
                Path = @"C:\External\Results\Image\20240817_17-55-10.json",
                Collection = "Gowther: Between The Arms of Sinners"
            }
        }
    };
    */

    public string UserInput { get; set; } = string.Empty;

    public bool WebSearchEnabled { get; set; }
    public int WebSearchResultsCount { get; set; }
    public ObservableCollection<Citation> Citations => SelectedMemory?.Documents ?? [];

    [ObservableProperty] private Dictionary<string, string[]>? _transcriptionsLanguages;
    [ObservableProperty] private bool _memoryEnabled;
    [ObservableProperty] private int _memoryChunksCount = 256;
    [ObservableProperty] private double _memorySimilarity = 0.1f;
    public ObservableCollection<MemorySource> Sources { get; } = [];
    public TextPromptParameters PromptParameters { get; set; } = new();
    [ObservableProperty] private TextGenerationParameters _generationParameters = new(true);
    [ObservableProperty] private TextModelParameters _modelParameters = new();


    [ObservableProperty] private int _chunkSize = 512;
    [ObservableProperty] private int _chunkOverlap = 64;
    [ObservableProperty] private string _chunkSeparator = string.Empty;

    public ObservableCollection<ModelInfo> Models { get; set; } = [];
    public MemoryService Service { get; }

    private readonly IModelsRepository _localRepository;
    private readonly ILogger<MemoryPageViewModel> _logger;
    private readonly Dictionary<string, string> _transcriptions = new();


    public MemoryPageViewModel(MemoryService memoryService, ILogger<MemoryPageViewModel> logger,
        [FromKeyedServices("LocalModelsRepository")]
        IModelsRepository localRepository)
    {
        _logger = logger;
        Service = memoryService;
        _localRepository = localRepository;
        if (!Design.IsDesignMode)
            RefreshModels().Wait();
    }

    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;

    #endregion

    #region SessionMethods

    [RelayCommand]
    private void Add()
    {
        SelectedMemory = new MemorySource
        {
            Name = $"Memory {DateTime.Now:dd.MM.yyyy_HH-mm-ss}",
        };
        Service.Sessions.Add(SelectedMemory);
    }

    [RelayCommand]
    public async Task Create()
    {
        if (SelectedMemory == null || State == GenerationState.Started)
            return;

        State = GenerationState.Started;
        await Service.Create(SelectedMemory.Name,
            Citations.Select(c => _transcriptions.TryGetValue(c.Path, out var lang) && !string.IsNullOrEmpty(lang) ? $"{c.Path}:::{lang}" : c.Path)
                .ToArray(), ChunkSize, ChunkOverlap, ChunkSeparator);
        State = GenerationState.Finished;
    }

    public Task Select(ISession? session)
    {
        if (session is not MemorySource sessionSource)
            return Task.CompletedTask;

        SelectedMemory = sessionSource;
        OnPropertyChanged(nameof(SelectedMemory));
        OnPropertyChanged(nameof(Citations));
        return Task.CompletedTask;
    }

    public async Task Select(MemorySource[] session) => await Task.CompletedTask;

    public Task Load(ISession? session) => Task.CompletedTask;

    [RelayCommand]
    public async Task Delete() => await Service.Delete(SelectedMemory?.Name!);

    [RelayCommand]
    public async Task Unload() => await Service.Dispose();

    public Task Generate() => Task.CompletedTask;

    #endregion

    #region AttachmentMethods

    [RelayCommand]
    private void AddWebEntry()
    {
        Citations.Add(new Citation { Path = "https://www.link-to-your-website.com" });
        OnPropertyChanged(nameof(Citations));
    }

    [RelayCommand]
    private void AddYoutubeEntry()
    {
       Citations.Add(new Citation { Path = "https://www.youtube.com/" });
        OnPropertyChanged(nameof(Citations));
    }

    [RelayCommand]
    private async Task FetchYoutubeLanguages(IEnumerable<Citation> citations)
    {
        try
        {
            var youtube = new YoutubeClient();
            var dictionary = new Dictionary<string, string[]>();
            foreach (var citation in citations)
            {
                var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(citation.Path);
                dictionary[citation.Path] = trackManifest.Tracks.Select(track => track.Language.Code.ToUpperInvariant())
                    .Distinct().ToArray();
            }

            TranscriptionsLanguages = dictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching youtube languages");
        }
    }

    [RelayCommand]
    private void SetYoutubeLanguage(IList<object> bindings)
    {
        if (bindings is not [Citation citation, string language])
            return;

        _transcriptions[citation.Path] = language;
    }

    [RelayCommand]
    public async Task LoadAttachment()
    {
        if (SelectedMemory == null)
            return;

        var filters = new List<FilePickerFileType>
        {
            new("Supported Documents")
            {
                Patterns = new List<string>
                {
                    "*.pdf", "*.csv", "*.doc", "*.docx", "*.epub", "*.txt", "*.md", "*.cs", "*.js", "*.py",
                    "*.java", "*.cpp", "*.h", "*.hpp", "*.c", "*.cc", "*.cxx", "*.go", "*.rb", "*.php",
                    "*.swift", "*.ts", "*.rs", "*.kt", "*.scala", "*.vb", "*.fs", "*.lua", "*.pl", "*.r",
                    "*.m", "*.sql", "*.sh", "*.bat", "*.ps1", "*.yml", "*.yaml", "*.json", "*.xml",
                    "*.html", "*.css", "*.less", "*.sass", "*.scss"
                },
                AppleUniformTypeIdentifiers = new List<string> { "public.data", "public.text", "public.item" },
                MimeTypes = new List<string> { "text/*", "application/pdf", "application/json", "text/html" }
            }
        };

        var paths = (await DialogService.OpenFileDialogAsync(true, filters)).ToArray();
        if (paths.Length == 0)
            return;

        foreach (var path in paths)
        {
            Citations.Add(new Citation
            {
                Title = Path.GetFileName(path),
                Path = path,
                Content = FileTextExtractor.ExtractText(path)
            });
        }

        OnPropertyChanged(nameof(Citations));
    }

    [RelayCommand]
    public void RemoveAttachment(Citation citation)
    {
        Citations.Remove(citation);
        OnPropertyChanged(nameof(Citations));
    }

    [RelayCommand]
    public async Task OpenAttachment(Citation citation) => await DialogService.LaunchUriAsync(citation.Path);

    #endregion

    #region ModelMethods

    [RelayCommand]
    private async Task RefreshModels() => Models =
        new ObservableCollection<ModelInfo>(await _localRepository.GetAllModels(new Dictionary<string, string>
            { { "Type", "Text" }, { "Task", "Embeddings" }, { "WeightExtension", ".gguf" } }));

    [RelayCommand]
    private async Task SwitchModel((ModelInfo GenerationModel, ModelInfoWeight GenerationWeight) model) =>
        await Service.ModelCore.Switch(model.GenerationModel, model.GenerationWeight,
            new Dictionary<string, object?> { { "ModelParameters", ModelParameters } });

    [RelayCommand]
    private async Task Dispose() => await Service.Dispose();

    #endregion
}