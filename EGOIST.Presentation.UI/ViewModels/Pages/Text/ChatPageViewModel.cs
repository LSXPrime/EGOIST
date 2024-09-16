using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Text;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Interactions;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class ChatPageViewModel([FromKeyedServices("ChatService")] ITextService<ChatSession> service, INetworkService networkService, MemoryService memoryService)
    : ViewModelBase, INavigationAware, ITextViewModel
{
    /*
    public ObservableCollection<ChatMessage> Messages { get; } =
    [
        new ChatMessage { Sender = "User", Message = "Hello!" },
        new ChatMessage { Sender = "Assistant", Message = "Hi there!" },
        new ChatMessage { Sender = "User", Message = "How are you?" },
        new ChatMessage { Sender = "Assistant", Message = "I'm doing well, thanks for asking." },
        new ChatMessage
        {
            Sender = "User", Message = "What's your favorite color?",
            Citations =
            [
                new Citation
                {
                    Title = "Blue", Path = "https://en.wikipedia.org/wiki/Blue",
                    Content = "Blue is a calming and peaceful color."
                },
                new Citation
                {
                    Title = "EGOIST CONFIG", Path = @"C:\External\Models\Text\Octopus-v2-2B\egoist_config.json",
                    Content = "Config file for the Octopus model."
                },
                new Citation
                {
                    Title = "Octopus", Path = @"C:\External\Models\Text\Octopus-v2-2B",
                    Content = "Octopus is a 2B parameters language model."
                }
            ]
        },
        new ChatMessage { Sender = "Assistant", Message = "My favorite color is blue." },
        new ChatMessage { Sender = "User", Message = "Why blue?" },
        new ChatMessage { Sender = "Assistant", Message = "It's a calming and peaceful color." },
        new ChatMessage { Sender = "User", Message = "Interesting! What's your favorite food?" },
        new ChatMessage
        {
            Sender = "Assistant",
            Message = "As an AI, I don't have a favorite food. But I can provide you with recipes if you'd like."
        },
        new ChatMessage { Sender = "User", Message = "That's cool!  Maybe a pizza recipe?" },
        new ChatMessage { Sender = "Assistant", Message = "Here's a recipe for a delicious Margherita pizza:" },
        new ChatMessage { Sender = "User", Message = "Thanks!" },
        new ChatMessage { Sender = "Assistant", Message = "You're welcome! Enjoy your pizza." },
        new ChatMessage { Sender = "User", Message = "I will!  Thanks for the chat." },
        new ChatMessage { Sender = "Assistant", Message = "It was nice chatting with you too! Have a great day." }
    ];
    */


    [ObservableProperty] private GenerationState _state = GenerationState.None;


    public override string Title => "Chat";

    #region ChatVariables

    [ObservableProperty] private string _userInput = string.Empty;
    [ObservableProperty] private bool _webSearchEnabled;
    [ObservableProperty] private int _webSearchResultsCount = 3;
    public ObservableCollection<Citation> Citations { get; } = [];

    [ObservableProperty] private bool _memoryEnabled;
    [ObservableProperty] private int _memoryChunksCount = 3;
    [ObservableProperty] private double _memorySimilarity = 0.5;
    public ObservableCollection<MemorySource> Sources { get; } = [];

    #endregion

    #region GenerationVariables

    [ObservableProperty] private TextGenerationParameters _generationParameters = new(true);
    [ObservableProperty] private TextPromptParameters _promptParameters = new();
    [ObservableProperty] private TextModelParameters _modelParameters = new();

    public ITextService<ChatSession> Service { get; } = service;

    public MemoryService MemoryService { get; } = memoryService;

    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters)
    {
        PromptParameters = Service.PromptTemplates.FirstOrDefault() ?? new TextPromptParameters();
        return Task.CompletedTask;
    }

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;

    #endregion
    
    #region SessionMethods

    [RelayCommand]
    public async Task Create()
    {
        // Create a new chat session and add it to Sessions
        await Service.Create();
    }

    [RelayCommand]
    public async Task Delete()
    {
        // Delete the selected chat session
        await Service.Delete();
    }

    [RelayCommand]
    public Task Select(ISession? session)
    {
        Service.SelectedSession = session as ChatSession;
        return Task.CompletedTask;
    }

    public async Task Select(MemorySource[] session)
    {
        Sources.Clear();
        Sources.AddRange(session);
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task Load(ISession? session)
    {
        if (session == null && Service.SelectedSession == null)
            return;

        await Service.LoadSession(session?.Name ?? Service.SelectedSession!.Name);
    }

    [RelayCommand]
    public async Task Unload()
    {
        // Unload and save the selected chat session
        await Service.Dispose();
    }

    [RelayCommand]
    public async Task Generate()
    {
        State = GenerationState.Started;
        var userInput = UserInput;
        var citations = Citations.ToArray();
        UserInput = string.Empty;
        Citations.Clear();
        
        if (WebSearchEnabled)
        {
            var results = await networkService.SearchAsync(userInput, WebSearchResultsCount, false);
            var resultsText = string.Empty;
            foreach (var result in results)
                resultsText += $"{result.Title}\n{result.Content}\n\n";

            await MemoryService.Feed(resultsText, Constants.TemporaryMemory);
            var searchCitations = await MemoryService.Generate(userInput, [], WebSearchResultsCount, 0.1f);
            foreach (var citation in searchCitations)
                citation.Collection = "Web Search";
            citations = citations.Concat(searchCitations).ToArray();
            await MemoryService.Delete(Constants.TemporaryMemory);
        }

        if (MemoryEnabled)
            citations = citations.Concat(await MemoryService.Generate(userInput, Sources.ToArray(), MemoryChunksCount, MemorySimilarity)).ToArray();

        await Task.Run(async () =>
                await Service.Generate<ChatMessage>(userInput, GenerationParameters, PromptParameters,
                    citations))
            .ConfigureAwait(false);
        State = GenerationState.Finished;
    }

    #endregion

    #region Attachments

    [RelayCommand]
    public async Task LoadAttachment()
    {
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
                Collection = "User Attachments",
                Title = Path.GetFileName(path),
                Path = path,
                Content = FileTextExtractor.ExtractText(path)
            });
        }
    }

    [RelayCommand]
    public void RemoveAttachment(Citation citation) => Citations.Remove(citation);

    [RelayCommand]
    public async Task OpenAttachment(Citation citation) => await DialogService.LaunchUriAsync(citation.Path);
    
    #endregion
}