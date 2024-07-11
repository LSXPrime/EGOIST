using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Management;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class RoleplayPageViewModel(
    [FromKeyedServices("RoleplayService")] ITextService service,
    CharacterService characterService) : ViewModelBase, INavigationAware
{
    public override string Title => "Roleplay";

    /*
    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages =
    [
        new ChatMessage { Sender = "User", Message = "Hello!" },
        new ChatMessage { Sender = "Assistant", Message = "Hi there!" },
        new ChatMessage { Sender = "User", Message = "How are you?" },
        new ChatMessage { Sender = "Assistant", Message = "I'm doing well, thanks for asking." },
        new ChatMessage { Sender = "User", Message = "What's your favorite color?" },
        new ChatMessage { Sender = "Assistant", Message = "My favorite color is blue." },
        new ChatMessage { Sender = "User", Message = "Why blue?" },
        new ChatMessage { Sender = "Assistant", Message = "It's a calming and peaceful color." },
        new ChatMessage { Sender = "User", Message = "Interesting! What's your favorite food?" },
        new ChatMessage { Sender = "Assistant", Message = "As an AI, I don't have a favorite food. But I can provide you with recipes if you'd like." },
        new ChatMessage { Sender = "User", Message = "That's cool!  Maybe a pizza recipe?" },
        new ChatMessage { Sender = "Assistant", Message = "Here's a recipe for a delicious Margherita pizza:" },
        new ChatMessage { Sender = "User", Message = "Thanks!" },
        new ChatMessage { Sender = "Assistant", Message = "You're welcome! Enjoy your pizza." },
        new ChatMessage { Sender = "User", Message = "I will!  Thanks for the chat." },
        new ChatMessage { Sender = "Assistant", Message = "It was nice chatting with you too! Have a great day." }
    ];
    */

    [ObservableProperty] private GenerationState _state = GenerationState.None;


    #region ChatVariables

    [ObservableProperty] private string _userInput = string.Empty;

    [ObservableProperty] private string[] _characterReceivers = ["Auto"];

    [ObservableProperty] private RoleplayWorld? _selectedWorld;
    #endregion

    #region GenerationVariables

    [ObservableProperty] private TextGenerationParameters _generationParameters = new();
    [ObservableProperty] private TextPromptParameters _promptParameters = new();
    [ObservableProperty] private TextModelParameters _modelParameters = new();

    public ITextService Service { get; } = service;
    public CharacterService CharacterService { get; } = characterService;

    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;
    

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;

    #endregion


    #region ChatMethods

    [RelayCommand]
    private async Task Create()
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
    private void ChatDelete()
    {
        Service.Delete();
    }

    [RelayCommand]
    private async Task MessageSend()
    {
        var userInput = UserInput;
        UserInput = string.Empty;

        await Task.Run(async () =>
                await Service.Generate<RoleplayMessage>(userInput, GenerationParameters, PromptParameters))
            .ConfigureAwait(false);
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
}