using System.Collections.ObjectModel;
using System.Diagnostics;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using LLama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChatSession = EGOIST.Domain.Entities.ChatSession;

namespace EGOIST.Application.Services.Text;

public class ChatService(
    ILogger<ChatService> logger,
    IPromptRepository<TextPromptParameters> promptRepository,
    [FromKeyedServices("TextModelCoreService")] IModelCoreService modelCore)
    : EntityBase, ITextService
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; } =
        new(promptRepository.GetAllTemplates(null).Result);

    public ObservableCollection<ChatSession> ChatSessions { get; } = [];

    public ChatSession? SelectedChatSession
    {
        get => _selectedChatSession;
        set => Notify(ref _selectedChatSession, value);
    }

    private readonly TextModelCoreService? _modelCore = modelCore as TextModelCoreService;
    private ChatSession? _selectedChatSession;

    public Task<bool> Create(Dictionary<string, object>? parameter = null)
    {
        if (_modelCore?.SelectedGenerationModel == null || _modelCore.ModelParameters == null ||
            _modelCore.Model == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.FromResult(false);
        }

        // TODO: Support Function Calling

        // Create a new chat session and add it to ChatSessions
        var newSession = new ChatSession
        {
            Name =  parameter?["Name"].ToString() ?? $"Chat {DateTime.Now}",
            Executor = new InferenceService(
                new InteractiveExecutor(_modelCore.Model.CreateContext(_modelCore.ModelParameters)))
        };
        ChatSessions.Add(newSession);
        SelectedChatSession = newSession;

        return Task.FromResult(true);
    }


    public Task<bool> Delete(string parameter = "")
    {
        if (!string.IsNullOrEmpty(parameter))
        {
            var session = ChatSessions.First(x => x.Name == parameter);
            session.Executor?.Dispose();
            ChatSessions.Remove(session);
            return Task.FromResult(true);
        }

        logger.LogInformation($"Chat {SelectedChatSession!.Name} Deleted");

        SelectedChatSession.Messages.Clear();
        ChatSessions.Remove(SelectedChatSession);
        SelectedChatSession = null;

        return Task.FromResult(true);
    }


    public async Task<T?> Generate<T>(string userInput, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null) where T : class
    {
        if (_modelCore?.State == GenerationState.Started)
        {
            await _modelCore.CancelToken?.CancelAsync()!;
            return null;
        }

        if (string.IsNullOrEmpty(userInput))
        {
            logger.LogWarning("User Input is empty.");
            return null;
        }

        if (_modelCore?.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return null;
        }

        if (SelectedChatSession == null)
        {
            logger.LogWarning("Session isn't selected yet.");
            return null;
        }


        _modelCore.State = GenerationState.Started;

        string prompt;
        _modelCore.CancelToken = new CancellationTokenSource();

        if (SelectedChatSession?.Executor == null ||
            await SelectedChatSession.Executor.IsFirstRun(nameof(StatefulExecutorBase)))
        {
            if (SelectedChatSession?.Executor == null)
                SelectedChatSession!.Executor =
                    new InferenceService(
                        new InteractiveExecutor(_modelCore.Model!.CreateContext(_modelCore.ModelParameters!)));

            prompt =
                $"{(SelectedChatSession.Messages.Count > 0 ? SelectedChatSession.ToString() : string.Empty)} \nUser: {userInput}";
            prompt = promptParameters?.Prompt(prompt, true)!;
        }
        else
        {
            prompt = promptParameters?.Prompt(userInput)!;
        }
        
        SelectedChatSession?.AddMessage("User", userInput);
        var aiMessage = SelectedChatSession?.AddMessage("Assistant", string.Empty);

        var tokens = SelectedChatSession?.Executor.Inference(prompt, promptParameters?.BlackList ?? [],
            generationParameters ?? new TextGenerationParameters(true), _modelCore.CancelToken.Token);

        /*
        var engine = ((InferenceService)SelectedChatSession.Executor)._inference;
        var tokens = engine.InferAsync(prompt, token: _modelCore.CancelToken.Token);
        */
        await foreach (var token in tokens!.WithCancellation(_modelCore.CancelToken.Token))
        {
            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                aiMessage!.Message = token;
                break;
            }

            aiMessage!.Message += token;
        }

        _modelCore.CancelToken?.Dispose();
        _modelCore.State = GenerationState.Finished;
        return aiMessage as T;
    }
}