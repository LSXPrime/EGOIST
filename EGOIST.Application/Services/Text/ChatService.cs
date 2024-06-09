using System.Collections.ObjectModel;
using System.Diagnostics;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using LLama;
using Microsoft.Extensions.Logging;
using ChatSession = EGOIST.Domain.Entities.ChatSession;

namespace EGOIST.Application.Services.Text;

public class ChatService(ILogger<ChatService> logger, IPromptRepository<TextPromptParameters> promptRepository)
    : EntityBase, ITextService
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; } = new(promptRepository.GetAllTemplates(null).Result);

    public ObservableCollection<ChatSession> ChatSessions { get; } = [];

    public ChatSession? SelectedChatSession
    {
        get => _selectedChatSession;
        set => Notify(ref _selectedChatSession, value);
    }

    private readonly GenerationService _generation = GenerationService.Instance;
    private ChatSession? _selectedChatSession;

    public Task Create(string sessionName = "")
    {
        if (_generation.SelectedGenerationModel == null || _generation.ModelParameters == null ||
            _generation.Model == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.CompletedTask;
        }

        // TODO: Support Function Calling

        // Create a new chat session and add it to ChatSessions
        var newSession = new ChatSession
        {
            SessionName = string.IsNullOrEmpty(sessionName) ? $"Chat {DateTime.Now}" : sessionName,
            Executor = new InferenceService(
                new InteractiveExecutor(_generation.Model.CreateContext(_generation.ModelParameters)))
        };
        ChatSessions.Add(newSession);
        SelectedChatSession = newSession;

        return Task.CompletedTask;
    }


    public Task Delete(string parameter = "")
    {
        if (!string.IsNullOrEmpty(parameter))
        {
            var session = ChatSessions.First(x => x.SessionName == parameter);
            session.Executor?.Dispose();
            ChatSessions.Remove(session);
            return Task.CompletedTask;
        }

        logger.LogInformation($"Chat {SelectedChatSession!.SessionName} Deleted");

        SelectedChatSession.Messages.Clear();
        ChatSessions.Remove(SelectedChatSession);
        SelectedChatSession = null;

        return Task.CompletedTask;
    }


    public async Task Generate(string userInput, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null)
    {
        if (_generation.State == GenerationState.Started)
        {
            await _generation.CancelToken?.CancelAsync()!;
            return;
        }

        if (string.IsNullOrEmpty(userInput))
        {
            logger.LogWarning("ChatUserInput is empty.");
            return;
        }

        if (_generation.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return;
        }

        if (SelectedChatSession == null)
            await Create($"Chat {DateTime.Now}");


        _generation.State = GenerationState.Started;

        string prompt;
        _generation.CancelToken = new CancellationTokenSource();

        if (SelectedChatSession?.Executor == null ||
            await SelectedChatSession.Executor.IsFirstRun(nameof(StatefulExecutorBase)))
        {
            if (SelectedChatSession?.Executor == null)
                SelectedChatSession!.Executor =
                    new InferenceService(
                        new InteractiveExecutor(_generation.Model!.CreateContext(_generation.ModelParameters!)));

            prompt =
                $"{(SelectedChatSession.Messages.Count > 0 ? SelectedChatSession.ToString() : string.Empty)} \nUser: {userInput}";
            prompt = promptParameters?.Prompt(prompt, true)!;
        }
        else
        {
            prompt = promptParameters?.Prompt(userInput)!;
        }
        
        Debug.WriteLine($"User Prompt: {prompt}");

        SelectedChatSession?.AddMessage("User", userInput);
        var aiMessage = SelectedChatSession?.AddMessage("Assistant", string.Empty);

        var tokens = SelectedChatSession?.Executor.Inference(prompt, promptParameters?.BlackList ?? [],
            generationParameters ?? new TextGenerationParameters(true), _generation.CancelToken.Token);

        /*
        var engine = ((InferenceService)SelectedChatSession.Executor)._inference;
        var tokens = engine.InferAsync(prompt, token: _generation.CancelToken.Token);
        */
        await foreach (var token in tokens!.WithCancellation(_generation.CancelToken.Token))
        {
            Debug.WriteLine($"RECEIVED TOKEN: {token}");

            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                aiMessage!.Message = token;
                break;
            }

            aiMessage!.Message += token;
        }

        _generation.CancelToken?.Dispose();
        _generation.State = GenerationState.Finished;
    }
}