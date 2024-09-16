using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Interactions;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class CompletionPageViewModel([FromKeyedServices("CompletionService")] ITextService<CompletionSession> completionService) : ViewModelBase, INavigationAware, ITextViewModel
{
    [ObservableProperty]
    private GenerationState _state = GenerationState.None;
    
    
    public override string Title => "Completion";
    
    #region CompletionVariables
    [ObservableProperty]
    private string _completionStatics = "L: 00 || W: 00 || C: 00";
    #endregion

    #region GenerationVariables

    [ObservableProperty]
    private TextGenerationParameters _generationParameters = new();
    [ObservableProperty]
    private TextPromptParameters _promptParameters = new();
    [ObservableProperty]
    private TextModelParameters _modelParameters = new();

    public ITextService<CompletionSession> Service { get; } = completionService;



    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;

    #endregion
    
    
    #region CompletionMethods
    
    
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
        Service.SelectedSession = session as CompletionSession;
        return Task.CompletedTask;
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
        await Task.Run(async () =>  await Service.Generate<string>(string.Empty, GenerationParameters, PromptParameters));
        State = GenerationState.Finished;
    }

    #endregion

    #region ITextViewModelUnused

    public string UserInput { get; set; }
    public bool WebSearchEnabled { get; set; }
    public int WebSearchResultsCount { get; set; }
    public ObservableCollection<Citation> Citations { get; set; }
    public bool MemoryEnabled { get; set; }
    public int MemoryChunksCount { get; set; }
    public double MemorySimilarity { get; set; }
    public ObservableCollection<MemorySource> Sources { get; set; }
    

    public Task Select(MemorySource[] session)
    {
        throw new System.NotImplementedException();
    }

    public Task Load(ISession? session)
    {
        throw new System.NotImplementedException();
    }
    public Task LoadAttachment()
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAttachment(Citation citation)
    {
        throw new System.NotImplementedException();
    }

    public Task OpenAttachment(Citation citation)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
