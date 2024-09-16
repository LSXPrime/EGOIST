using System.Collections.ObjectModel;
using System.Globalization;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Management.Loaders;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using LLama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChatSession = EGOIST.Domain.Entities.ChatSession;

namespace EGOIST.Application.Services.Text;

public class ChatService : EntityBase, ITextService<ChatSession>
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; } = [];

    public ObservableCollection<ChatSession> Sessions { get; set; } = [];

    public ChatSession? SelectedSession
    {
        get => _selectedSession;
        set => Notify(ref _selectedSession, value);
    }

    private ChatSession? _selectedSession;

    private readonly TextModelCoreService _modelCore;
    private readonly TextDataLoader _dataLoader;
    private readonly MemoryService _memoryService;
    private readonly IPromptRepository<TextPromptParameters> _promptRepository;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ILogger<ChatService> logger,
        IPromptRepository<TextPromptParameters> promptRepository,
        [FromKeyedServices("TextModelCoreService")]
        IModelCoreService modelCore, TextDataLoader dataLoader, MemoryService memoryService)
    {
        _logger = logger;
        _promptRepository = promptRepository;
        _dataLoader = dataLoader;
        _memoryService = memoryService;
        _modelCore = (TextModelCoreService)modelCore;

        _ = Initialize();
    }

    private async Task Initialize()
    {
        PromptTemplates = new ObservableCollection<TextPromptParameters>(await _promptRepository.GetAllTemplates(null));
        Sessions = new ObservableCollection<ChatSession>(await _dataLoader.LoadAllSessions<ChatSession>("Chat"));
    }

    public async Task<bool> LoadSession(string sessionName, bool loadCache = true,
        Dictionary<string, object>? parameter = null)
    {
        if (string.IsNullOrEmpty(sessionName) || _modelCore.SelectedGenerationModel == null ||
            _modelCore.ModelParameters == null || _modelCore.Model == null)
            return false;

        var existingSession = Sessions.FirstOrDefault(x => x.Name == sessionName);
        var session = existingSession != null && loadCache
            ? await _dataLoader.LoadSession(existingSession)
            : await _dataLoader.LoadSession<ChatSession>(sessionName, "Chat", loadCache);
        if (session == null)
            return false;

        if (existingSession == null)
            Sessions.Add(session);


        SelectedSession = session;
        return true;
    }

    public Task<bool> Create(Dictionary<string, object>? parameter = null)
    {
        if (_modelCore.SelectedGenerationModel == null || _modelCore.ModelParameters == null ||
            _modelCore.Model == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.FromResult(false);
        }

        // TODO: Support Function Calling

        // Create a new chat session and add it to Sessions
        var newSession = new ChatSession
        {
            Name = parameter?["Name"].ToString() ??
                   $"Chat {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}",
            Executor = new InferenceService(
                new InteractiveExecutor(_modelCore.Model.CreateContext(_modelCore.ModelParameters))),
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
            session.Executor?.Dispose();
            Sessions.Remove(session);
            if (session == SelectedSession)
                SelectedSession = null;

            await _dataLoader.DeleteSession(parameter);
            return true;
        }

        if (SelectedSession == null)
            return false;

        SelectedSession.Executor?.Dispose();
        await _dataLoader.DeleteSession(SelectedSession.Name);
        _logger.LogInformation("Chat {SelectedSession.Name} Deleted", SelectedSession.Name);
        Sessions.Remove(SelectedSession);
        SelectedSession = null;
        return true;
    }


    public async Task<T?> Generate<T>(string userInput, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null, Citation[]? citations = null) where T : class
    {
        if (_modelCore.State == GenerationState.Started)
        {
            await _modelCore.CancelToken?.CancelAsync()!;
            return null;
        }

        if (_modelCore.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return null;
        }

        if (SelectedSession == null)
        {
            _logger.LogWarning("Session isn't selected yet.");
            return null;
        }


        _modelCore.State = GenerationState.Started;
        var prompt = userInput;
        citations ??= [];
        if (AppConfig.Instance.Parameters.ChatMemory)
            citations = citations.Concat(await _memoryService.Generate(userInput, [], isGlobalMemory: true)).ToArray();

        if (citations.Length > 0)
        {
            var memories = citations.Where(x => x.Collection == Constants.GlobalMessagesMemory).ToArray();
            var extraCitations = citations.Where(x => x.Collection != Constants.GlobalMessagesMemory).ToArray();
            if (memories.Length > 0)
            {
                prompt += $"\n\n{AppConfig.Instance.Parameters.GlobalMemoryPrompt}";
                foreach (var citation in memories)
                    prompt += $"\n{citation.Content}";
            }
            if (extraCitations.Length > 0)
            {
                prompt += $"\n\n{AppConfig.Instance.Parameters.MemoryPrompt}";
                foreach (var citation in extraCitations)
                    prompt += $"\n{citation.Content}";
            }
        }


        _modelCore.CancelToken = new CancellationTokenSource();

        if (SelectedSession.Executor == null ||
            await SelectedSession.Executor.IsFirstRun(nameof(StatefulExecutorBase)))
        {
            if (SelectedSession.Executor == null)
            {
                SelectedSession.Executor =
                    new InferenceService(
                        new InteractiveExecutor(_modelCore.Model!.CreateContext(_modelCore.ModelParameters!)));
                SelectedSession.IsLoaded = true;
            }

            prompt =
                $"{(SelectedSession.Messages.Count > 0 ? SelectedSession.ToString() : string.Empty)} \nUser: {prompt}";
            prompt = promptParameters?.Prompt(prompt, true)!;
        }
        else
        {
            prompt = promptParameters?.Prompt(prompt)!;
        }

        ChatMessage? userMessage = null;
        if (!string.IsNullOrEmpty(userInput))
            userMessage = SelectedSession.AddMessage("User", userInput,
                citations.Where(x => x.Collection == "User Attachments").ToArray());

        var aiMessage = SelectedSession.AddMessage("Assistant", string.Empty,
            citations.Where(x => x.Collection != "User Attachments").ToArray());

        var tokens = SelectedSession.Executor.Inference(prompt, promptParameters?.BlackList ?? [],
            generationParameters ?? new TextGenerationParameters(true), _modelCore.CancelToken.Token);

        await foreach (var token in tokens.WithCancellation(_modelCore.CancelToken.Token))
        {
            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                await _modelCore.CancelToken.CancelAsync();
                break;
            }

            aiMessage.Message += token;
        }

        if (!string.IsNullOrEmpty(userInput) && userMessage != null)
            await _memoryService.Feed(userMessage.ToString());
        await _memoryService.Feed(aiMessage.ToString());
        
        _modelCore.CancelToken?.Dispose();
        _modelCore.State = GenerationState.Finished;
        return aiMessage as T;
    }

    public async Task Dispose()
    {
        if (SelectedSession != null)
        {
            SelectedSession.IsLoaded = false;
            await _dataLoader.SaveSession(SelectedSession, "Chat");
            SelectedSession.Executor?.Dispose();
        }
    }
}