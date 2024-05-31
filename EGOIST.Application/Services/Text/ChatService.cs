using System.Collections.ObjectModel;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Enums;
using LLama;
using Microsoft.Extensions.Logging;
using ChatSession = EGOIST.Domain.Entities.ChatSession;

namespace EGOIST.Application.Services.Text;

public class ChatService(ILogger<ChatService> logger) : ITextService
{
    public ObservableCollection<ChatSession> ChatSessions { get; } = [];
    public ChatSession? SelectedChatSession { get; set; }

    private readonly ILogger<ChatService> _logger = logger;
    private readonly GenerationService _generation = GenerationService.Instance;

    public Task Create(string sessionName)
    {
        if (_generation.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.CompletedTask;
        }

        // TODO: Support Function Calling whenever Semantic Kernel support it for LLamaSharp

        // Create a new chat session and add it to ChatSessions
        var newSession = new ChatSession
        {
            SessionName = sessionName,
            Executor = new InferenceService(new InteractiveExecutor(_generation.Model.CreateContext(_generation.ModelParameters)))
        };
        ChatSessions.Add(newSession);
        SelectedChatSession = newSession;

        return Task.CompletedTask;
    }


    public Task Delete(string parameter)
    {
        _logger.LogInformation($"Chat {SelectedChatSession.SessionName} Deleted");

        SelectedChatSession.Messages.Clear();
        ChatSessions.Remove(SelectedChatSession);
        SelectedChatSession = null;

        return Task.CompletedTask;
    }


    public async Task Generate(string userInput)
    {
        if (_generation.State == GenerationState.Started)
        {
            _generation.CancelToken?.Cancel();
            return;
        }

        if (string.IsNullOrEmpty(userInput))
        {
            _logger.LogWarning("ChatUserInput is empty.");
            return;
        }

        if (_generation.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return;
        }

        if (SelectedChatSession == null)
            await Create($"Chat {DateTime.Now}");

        try
        {
            _generation.State = GenerationState.Started;

            var Prompt = string.Empty;
            _generation.CancelToken = new CancellationTokenSource();

            if (SelectedChatSession?.Executor == null || SelectedChatSession.Executor.IsFirstRun(nameof(StatefulExecutorBase)).Result)
            {
                if (SelectedChatSession?.Executor == null)
                    SelectedChatSession.Executor = new InferenceService(new InteractiveExecutor(_generation.Model.CreateContext(_generation.ModelParameters)));
                Prompt = $"{(SelectedChatSession.Messages.Count > 0 ? SelectedChatSession.ToString() : string.Empty)} \nUser: {userInput}";
            }
            else
            {
                Prompt = _generation.SelectedGenerationModel.TextConfig.Prompt(userInput, SelectedChatSession?.Messages.Count <= 2);
            }

            var userMessage = SelectedChatSession?.AddMessage("User", userInput);
            var aiMessage = SelectedChatSession?.AddMessage("Assistant", string.Empty);

            var tokens = SelectedChatSession?.Executor.InferenceConcurrent(Prompt, _generation.SelectedGenerationModel.TextConfig.BlackList, _generation.Parameters, _generation.CancelToken.Token);
            await foreach (var token in tokens)
            {
                if (token == "FILTERING MECHANISM TRIGGERED")
                {
                    aiMessage.Message = token;
                    break;
                }
                aiMessage.Message += token + " ";
            }

            _generation.State = GenerationState.Finished;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating chat message.");
        }
        finally
        {
            _generation.CancelToken?.Dispose();
        }
    }
}
