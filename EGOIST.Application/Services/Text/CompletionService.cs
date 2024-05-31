using System.Collections.ObjectModel;
using System.Threading.Tasks.Dataflow;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using LLama;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class CompletionService(ILogger<CompletionService> _logger) : ITextService
{
    public ObservableCollection<CompletionSession> CompletionSessions { get; } = [];
    public CompletionSession? SelectedCompletionSession { get; set; }
    public TextPromptParameters CompletionPrompt { get; set; } = new() { SystemPrompt = "I want you to act as a storyteller. You will come up with entertaining stories that are engaging, imaginative and captivating for the audience." };

    private readonly GenerationService _generation = GenerationService.Instance;

    public Task Create(string sessionName)
    {
        if (_generation.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.CompletedTask;
        }

        var newSession = new CompletionSession
        {
            SessionName = sessionName,
            Executor = new InferenceService(new StatelessExecutor(_generation.Model, _generation.ModelParameters)),
        };
        CompletionSessions.Add(newSession);
        SelectedCompletionSession = newSession;
        return Task.CompletedTask;
    }

    public Task Delete(string parameter)
    {
        _logger.LogInformation($"Session {SelectedCompletionSession.SessionName} Deleted");

        SelectedCompletionSession.Executor?.Dispose();
        CompletionSessions.Remove(SelectedCompletionSession);
        SelectedCompletionSession = null;

        return Task.CompletedTask;
    }

    public async Task Generate(string userInput)
    {
        if (_generation.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return;
        }
        if (_generation.State == GenerationState.Started)
        {
            _generation.CancelToken.Cancel();
            return;
        }
        if (SelectedCompletionSession == null)
            await Create($"Completion {DateTime.Now}");

        SelectedCompletionSession.Content = userInput;
        if (SelectedCompletionSession?.Executor == null)
            SelectedCompletionSession.Executor = new InferenceService(new StatelessExecutor(_generation.Model, _generation.ModelParameters));

        _generation.State = GenerationState.Started;
        var Prompt = _generation.SelectedGenerationModel.TextConfig.Prompt(userInput, CompletionPrompt);

        _generation.CancelToken = new();
        var tokens = SelectedCompletionSession.Executor.InferenceConcurrent(Prompt, _generation.SelectedGenerationModel.TextConfig.BlackList, _generation.Parameters, _generation.CancelToken.Token);
        await foreach (var token in tokens)
        {
            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                SelectedCompletionSession.Content = token;
                break;
            }
            SelectedCompletionSession.Content += token + " ";
        }

        _generation.State = GenerationState.Finished;
        _generation.CancelToken.Dispose();
    }
}
