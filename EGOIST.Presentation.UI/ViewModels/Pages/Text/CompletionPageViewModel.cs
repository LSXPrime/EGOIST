using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class CompletionPageViewModel([FromKeyedServices("ChatService")] ITextService chatService) : ViewModelBase, INavigationAware
{
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
    
    [ObservableProperty]
    private GenerationState _state = GenerationState.None;
    
    

    public new string Title => "Chat";
    
    #region CompletionVariables
    [ObservableProperty]
    private string _userInput = string.Empty;
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

    public ITextService ChatService { get; } = chatService;

    #endregion

    #region Navigation

    public void Initialize(Dictionary<string, object>? parameters) { }

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() { }

    #endregion
    
    
    #region CompletionMethods
    
    
    [RelayCommand]
    private void SessionCreate()
    {
        // Create a new chat session and add it to ChatSessions
        ChatService.Create();
    }

    [RelayCommand]
    private void SessionDelete()
    {
        // Delete the selected chat session
        ChatService.Delete();
    }

    [RelayCommand]
    private async Task Generate() 
    {
        var userInput = UserInput;
        UserInput = string.Empty;

        State = GenerationState.Started;
        await Task.Run(async () => await ChatService.Generate(userInput, GenerationParameters, PromptParameters));
        State = GenerationState.Finished;
    }

    #endregion

    #region UIMethods

    [RelayCommand]
    private async Task CompletionSearch()
    {
        
    }

    #endregion
}
