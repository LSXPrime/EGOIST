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
using EGOIST.Application.Services.Management;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Interactions;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.Services.Utilities;
using EGOIST.Presentation.UI.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class RoleplayPageViewModel(INetworkService networkService,
    [FromKeyedServices("RoleplayService")] ITextService<RoleplaySession> service,
    CharacterService characterService) : ViewModelBase, INavigationAware, ITextViewModel
{
    public override string Title => "Roleplay";
    
    [ObservableProperty] private GenerationState _state = GenerationState.None;


    #region ChatVariables

    [ObservableProperty] private string _userInput = string.Empty;

    [ObservableProperty] private string[] _characterReceivers = ["Auto"];

    [ObservableProperty] private RoleplayWorld? _selectedWorld;
    
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

    public ITextService<RoleplaySession> Service { get; } = service;
    public CharacterService CharacterService { get; } = characterService;

    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;
    

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;

    #endregion


    #region SessionMethods

    [RelayCommand]
    public async Task Create()
    {
        var result = await DialogService.CreateDialogAsync<TextRoleplayCreateViewModel>();
        if (result == null)
            return;

        var parameters = new Dictionary<string, object>
        {
            { "Name", result.SessionName! },
            { "UserRoleName", result.UserCharacterName! },
            { "Characters", result.SelectedCharacters },
            { "PersonalityApproach", result.PersonalityApproach },
            { "WorldMemory", SelectedWorld! }
        };

        if (await Service.Create(parameters))
            CharacterReceivers = result.SelectedCharacters.Select(x => x.Name).Prepend("Auto").ToArray();
    }

    [RelayCommand]
    public async Task Delete()
    {
        await Service.Delete();
    }
    
    [RelayCommand]
    public Task Select(ISession? session)
    {
        Service.SelectedSession = session as RoleplaySession;
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
            citations = citations.Concat(await networkService.SearchAsync(userInput, WebSearchResultsCount)).ToArray();

        await Task.Run(async () =>
                await Service.Generate<RoleplayMessage>(userInput, GenerationParameters, PromptParameters, citations))
            .ConfigureAwait(false);
        
        State = GenerationState.Finished;
    }

    [RelayCommand]
    private async Task WorldManage()
    {
        var result = await DialogService.CreateDialogAsync<TextRoleplayWorldMemoryViewModel>(primaryButtonText: "Close", cancelButtonText: "");
        if (result == null)
            return;

        SelectedWorld = result.SelectedWorld;
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