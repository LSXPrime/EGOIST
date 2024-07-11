using System.Collections.ObjectModel;
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

namespace EGOIST.Application.Services.Text;

public class CompletionService(
    ILogger<CompletionService> logger,
    IPromptRepository<TextPromptParameters> promptRepository,
    [FromKeyedServices("TextModelCoreService")] IModelCoreService modelCore) : EntityBase, ITextService
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; } =
        new(promptRepository.GetAllTemplates(null).Result);

    public ObservableCollection<CompletionSession> CompletionSessions { get; } = [];

    public CompletionSession? SelectedCompletionSession
    {
        get => _selectedCompletionSession;
        set => Notify(ref _selectedCompletionSession, value);
    }


    private readonly TextModelCoreService? _modelCore = modelCore as TextModelCoreService;
    private CompletionSession? _selectedCompletionSession;

    public Task<bool> Create(Dictionary<string, object>? parameter = null)
    {
        if (_modelCore?.SelectedGenerationModel == null || _modelCore.ModelParameters == null ||
            _modelCore.Model == null || _modelCore.State == GenerationState.Started)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.FromResult(false);
        }

        var newSession = new CompletionSession
        {
            Name =  parameter?["Name"].ToString() ?? $"Completion {DateTime.Now}",
            Executor = new InferenceService(new StatelessExecutor(_modelCore.Model!, _modelCore.ModelParameters!)),
        };
        CompletionSessions.Add(newSession);
        SelectedCompletionSession = newSession;
        return Task.FromResult(true);
    }

    public Task<bool> Delete(string parameter = "")
    {
        if (!string.IsNullOrEmpty(parameter))
        {
            var session = CompletionSessions.First(x => x.Name == parameter);
            if (session == SelectedCompletionSession)
            {
                _modelCore?.CancelToken?.Cancel();
                SelectedCompletionSession = null;
            }
            session.Executor?.Dispose();
            CompletionSessions.Remove(session);
            return Task.FromResult(true);
        }

        logger.LogInformation($"Session {SelectedCompletionSession!.Name} Deleted");

        _modelCore?.CancelToken?.Cancel();
        CompletionSessions.Remove(SelectedCompletionSession);
        SelectedCompletionSession = null;

        return Task.FromResult(true);
    }

    public async Task<T?> Generate<T>(string prompt, TextGenerationParameters? generationParameters = null, TextPromptParameters? promptParameters = null) where T : class
    {
        if (_modelCore?.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return null;
        }

        if (_modelCore.State == GenerationState.Started)
        {
            await _modelCore.CancelToken?.CancelAsync()!;
            return null;
        }

        if (SelectedCompletionSession == null)
        {
            logger.LogWarning("Session isn't selected yet.");
            return null;
        }


        if (string.IsNullOrEmpty(prompt))
            prompt = SelectedCompletionSession!.Content;
        
        SelectedCompletionSession!.Content = prompt;
        SelectedCompletionSession.Executor ??= new InferenceService(new StatelessExecutor(_modelCore.Model!, _modelCore.ModelParameters!));

        _modelCore.State = GenerationState.Started;
        prompt = promptParameters?.Prompt(prompt, true)!;

        _modelCore.CancelToken = new CancellationTokenSource();
        var tokens = SelectedCompletionSession.Executor.Inference(prompt, promptParameters?.BlackList ?? [],
            generationParameters ?? new TextGenerationParameters(true), _modelCore.CancelToken.Token);
        await foreach (var token in tokens)
        {
            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                SelectedCompletionSession.Content = token;
                break;
            }

            SelectedCompletionSession.Content += token;
        }

        _modelCore.State = GenerationState.Finished;
        _modelCore.CancelToken.Dispose();
        
        return SelectedCompletionSession.Content as T ?? null;
    }
}