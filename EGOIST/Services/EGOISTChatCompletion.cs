using LLama;
using LLama.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using static LLama.LLamaTransforms;

namespace EGOIST.Services;

public sealed class EGOISTChatCompletion : IAIService
{
    private readonly InteractiveExecutor _model;
    private ChatRequestSettings defaultRequestSettings;
    private readonly ITextStreamTransform outputTransform;

    private readonly Dictionary<string, object?> _attributes = new();

    public IReadOnlyDictionary<string, object?> Attributes => this._attributes;

    static ChatRequestSettings GetDefaultSettings()
    {
        return new ChatRequestSettings
        {
            MaxTokens = 256,
            Temperature = 0,
            TopP = 0,
            StopSequences = new List<string>()
        };
    }

    public EGOISTChatCompletion(InteractiveExecutor model,
        ChatRequestSettings? defaultRequestSettings = default,
        ITextStreamTransform? outputTransform = null)
    {
        this._model = model;
        this.defaultRequestSettings = defaultRequestSettings ?? GetDefaultSettings();
        this.outputTransform = outputTransform ?? new KeywordTextOutputStreamTransform(new[] { $"{LLama.Common.AuthorRole.User}:",
                                                                                            $"{LLama.Common.AuthorRole.Assistant}:",
                                                                                            $"{LLama.Common.AuthorRole.System}:"});
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        /*
        var settings = executionSettings != null
           ? ChatRequestSettings.FromRequestSettings(executionSettings)
           : defaultRequestSettings;
        var prompt = historyTransform.HistoryToText(chatHistory.ToLLamaSharpChatHistory());

        var result = _model.InferAsync(prompt, settings.ToLLamaSharpInferenceParams(), cancellationToken);

        var output = outputTransform.TransformAsync(result);

        var sb = new StringBuilder();
        await foreach (var token in output)
        {
            sb.Append(token);
        }

        return new List<ChatMessageContent> { new(AuthorRole.Assistant, sb.ToString()) }.AsReadOnly();
        */
        
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var settings = executionSettings != null
          ? ChatRequestSettings.FromRequestSettings(executionSettings)
          : defaultRequestSettings;

        var result = _model.InferAsync(prompt, ToLLamaSharpInferenceParams(settings), cancellationToken);

        var output = outputTransform.TransformAsync(result);

        await foreach (var token in output)
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, token);
        }
    }

    private static LLama.Common.InferenceParams ToLLamaSharpInferenceParams(ChatRequestSettings requestSettings)
    {
        if (requestSettings is null)
        {
            throw new ArgumentNullException(nameof(requestSettings));
        }

        var antiPrompts = new List<string>(requestSettings.StopSequences)
                                  { LLama.Common.AuthorRole.User.ToString() + ":" ,
                                    LLama.Common.AuthorRole.Assistant.ToString() + ":",
                                    LLama.Common.AuthorRole.System.ToString() + ":"};

        return new LLama.Common.InferenceParams
        {
            Temperature = (float)requestSettings.Temperature,
            TopP = (float)requestSettings.TopP,
            PresencePenalty = (float)requestSettings.PresencePenalty,
            FrequencyPenalty = (float)requestSettings.FrequencyPenalty,
            AntiPrompts = antiPrompts,
            MaxTokens = requestSettings.MaxTokens ?? -1
        };
    }
}
