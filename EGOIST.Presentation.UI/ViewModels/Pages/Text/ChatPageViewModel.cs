using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Text;

public partial class ChatPageViewModel([FromKeyedServices("ChatService")] ITextService ChatService) : ViewModelBase, INavigationAware
{
    [ObservableProperty]
    private GenerationState _state = GenerationState.None;

    public string Title => "Chat";
    
    #region ChatVariables
    [ObservableProperty]
    private ObservableCollection<ISession<ChatMessage>> _chatSessions = [];
    [ObservableProperty]
    private ISession<ChatMessage>? _selectedChatSession;
    [ObservableProperty]
    private string _chatUserInput = string.Empty;
    #endregion

    #region GenerationVariables

    [ObservableProperty]
    private TextGenerationParameters _generationParameters = new();
    [ObservableProperty]
    private TextPromptParameters _promptParameters = new();
    [ObservableProperty]
    private TextModelParameters _modelParameters = new();

    #endregion


    #region Navigation

    public void Initialize(Dictionary<string, object>? parameters) { }

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() { }

    #endregion
    
    
    #region ChatMethods

    [RelayCommand]
    private void ChatCreate()
    {
        // Create a new chat session and add it to ChatSessions
    }

    [RelayCommand]
    private void ChatDelete()
    {
    }

    [RelayCommand]
    private async void MessageSend()
    {
        // Send the message to the chat session and add it to the chat session's messages
    }

    #endregion

}
