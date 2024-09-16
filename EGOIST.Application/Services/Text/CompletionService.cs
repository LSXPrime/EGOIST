using System.Collections.ObjectModel;
using System.Globalization;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Management.Loaders;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using LLama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class CompletionService : EntityBase, ITextService<CompletionSession>
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; } = [];

    public ObservableCollection<CompletionSession> Sessions { get; set; } = [];

    public CompletionSession? SelectedSession
    {
        get => _selectedSession;
        set => Notify(ref _selectedSession, value);
    }

    private CompletionSession? _selectedSession;

    private readonly TextModelCoreService _modelCore;
    private readonly TextDataLoader _dataLoader;
    private readonly IPromptRepository<TextPromptParameters> _promptRepository;
    private readonly ILogger<CompletionService> _logger;

    public CompletionService(ILogger<CompletionService> logger,
        IPromptRepository<TextPromptParameters> promptRepository,
        [FromKeyedServices("TextModelCoreService")]
        IModelCoreService modelCore, TextDataLoader dataLoader)
    {
        _logger = logger;
        _promptRepository = promptRepository;
        _dataLoader = dataLoader;
        _modelCore = (TextModelCoreService)modelCore;

        _ = Initialize();
    }

    private async Task Initialize()
    {
        PromptTemplates = new ObservableCollection<TextPromptParameters>(await _promptRepository.GetAllTemplates(null));
        Sessions =
            new ObservableCollection<CompletionSession>(await _dataLoader.LoadAllSessions<CompletionSession>("Completion"));
    }

    public async Task<bool> LoadSession(string sessionName, bool loadCache = false,
        Dictionary<string, object>? parameter = null)
    {
        if (string.IsNullOrEmpty(sessionName) || _modelCore.SelectedGenerationModel == null ||
            _modelCore.ModelParameters == null || _modelCore.Model == null)
            return false;

        var existingSession = Sessions.FirstOrDefault(x => x.Name == sessionName);
        if (existingSession != null)
            return true;

        var session = await _dataLoader.LoadSession<CompletionSession>(sessionName, "Completion",loadCache);
        if (session == null)
            return false;

        Sessions.Add(session);

        SelectedSession = session;
        return true;
    }

    public Task<bool> Create(Dictionary<string, object>? parameter = null)
    {
        if (_modelCore.SelectedGenerationModel == null || _modelCore.ModelParameters == null ||
            _modelCore.Model == null || _modelCore.State == GenerationState.Started)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.FromResult(false);
        }

        var newSession = new CompletionSession
        {
            Name = parameter?["Name"].ToString() ??
                   $"Completion {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}",
            Executor = new InferenceService(new StatelessExecutor(_modelCore.Model!, _modelCore.ModelParameters!)),
            IsLoaded = true
        };
        Sessions.Add(newSession);
//        SelectedSession = newSession;
        return Task.FromResult(true);
    }

    public async Task<bool> Delete(string parameter = "")
    {
        if (!string.IsNullOrEmpty(parameter))
        {
            var session = Sessions.FirstOrDefault(x => x.Name == parameter);
            if (session == null)
                return false;

            if (session == SelectedSession)
                SelectedSession = null;

            await _modelCore.CancelToken?.CancelAsync()!;
            await _dataLoader.DeleteSession(session.Name);
            session.Executor?.Dispose();
            Sessions.Remove(session);
            return true;
        }

        if (SelectedSession == null)
            return false;

        await _modelCore.CancelToken?.CancelAsync()!;
        await _dataLoader.DeleteSession(SelectedSession.Name);
        Sessions.Remove(SelectedSession);

        _logger.LogInformation($"Session {SelectedSession.Name} Deleted");
        SelectedSession = null;

        return true;
    }

    public async Task<T?> Generate<T>(string prompt, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null, Citation[]? citations = null) where T : class
    {
        if (_modelCore.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return null;
        }

        if (_modelCore.State == GenerationState.Started)
        {
            await _modelCore.CancelToken?.CancelAsync()!;
            return null;
        }

        if (SelectedSession == null)
        {
            _logger.LogWarning("Session isn't selected yet.");
            return null;
        }


        if (string.IsNullOrEmpty(prompt))
            prompt = SelectedSession!.Content;

        SelectedSession!.Content = prompt;
        SelectedSession.Executor ??=
            new InferenceService(new StatelessExecutor(_modelCore.Model!, _modelCore.ModelParameters!));

        _modelCore.State = GenerationState.Started;
        prompt = promptParameters?.Prompt(prompt, true)!;

        _modelCore.CancelToken = new CancellationTokenSource();
        var tokens = SelectedSession.Executor.Inference(prompt, promptParameters?.BlackList ?? [],
            generationParameters ?? new TextGenerationParameters(true), _modelCore.CancelToken.Token);
        await foreach (var token in tokens)
        {
            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                await _modelCore.CancelToken.CancelAsync();
                break;
            }

            SelectedSession.Content += token;
        }

        _modelCore.State = GenerationState.Finished;
        _modelCore.CancelToken.Dispose();

        return SelectedSession.Content as T ?? null;
    }

    public async Task Dispose()
    {
        if (SelectedSession != null)
        {
            SelectedSession.IsLoaded = false;
            await _dataLoader.SaveSession(SelectedSession, "Completion",false);
            SelectedSession?.Executor?.Dispose();
        }
    }
}