using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class CompletionPageViewModel([FromKeyedServices("CompletionService")] ITextService completionService) : ViewModelBase, INavigationAware
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

    public ITextService Service { get; } = completionService;



    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;

    #endregion
    
    
    #region CompletionMethods
    
    
    [RelayCommand]
    private void SessionCreate()
    {
        // Create a new chat session and add it to ChatSessions
        Service.Create();
    }

    [RelayCommand]
    private void SessionDelete()
    {
        // Delete the selected chat session
        Service.Delete();
    }

    [RelayCommand]
    private async Task Generate() 
    {
        State = GenerationState.Started;
        await Task.Run(async () =>  await Service.Generate<string>(string.Empty, GenerationParameters, PromptParameters));
        State = GenerationState.Finished;
    }

    #endregion
}
