using System.Threading.Tasks.Dataflow;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using static LLama.InteractiveExecutor;

namespace EGOIST.Application.Inference.Text;

public class InferenceService(ILLamaExecutor _inference) : IInference
{
    public void Dispose()
    {
        switch (_inference)
        {
            case StatefulExecutorBase statefulExecutor:
                statefulExecutor.Context.Dispose();
                statefulExecutor.ClipModel?.Dispose();
                break;
            case StatelessExecutor statelessExecutor:
                statelessExecutor.Context.Dispose();
                statelessExecutor.ClipModel?.Dispose();
                break;
            default:
                break;
        }
    }

    public IAsyncEnumerable<T> Inference<T>(string prompt, TextGenerationParameters parameters, CancellationToken cancellationToken)
    {
        var messageParams = new InferenceParams()
        {
            MaxTokens = parameters.MaxTokens,
            Temperature = parameters.Randomness,
            TopP = parameters.RandomnessBooster,
            TopK = (int)(parameters.OptimalProbability * 100),
            FrequencyPenalty = parameters.FrequencyPenalty,
            AntiPrompts = ["User:", "", "<||end_of_turn|>"]
        };

        return (IAsyncEnumerable<T>)_inference.InferAsync(prompt, messageParams, cancellationToken);
    }

    public async IAsyncEnumerable<T> InferenceConcurrent<T>(string prompt, IEnumerable<T> blacklist, TextGenerationParameters parameters, CancellationToken cancellationToken)
    {
        var messageParams = new InferenceParams()
        {
            MaxTokens = parameters.MaxTokens,
            Temperature = parameters.Randomness,
            TopP = parameters.RandomnessBooster,
            TopK = (int)(parameters.OptimalProbability * 100),
            FrequencyPenalty = parameters.FrequencyPenalty,
            AntiPrompts = ["User:", "", "<||end_of_turn|>"]
        };

        var tokens = (IAsyncEnumerable<T>)_inference.InferAsync(prompt, messageParams, cancellationToken);

        // Oh God, I've been using Unity for 6 years and executing everything on the main thread. Wtf am I doing here?

        // Use BufferBlock to hold generated tokens
        var bufferBlock = new BufferBlock<T>();

        // ActionBlock to process and filter tokens
        var actionBlock = new ActionBlock<T>(token =>
        {
            // Apply filtering on the token
            if (blacklist.Any(x => token.ToString().Contains(x.ToString(), StringComparison.CurrentCultureIgnoreCase)))
            {
                // Blacklist match found
                bufferBlock.Post((T)(object)"FILTERING MECHANISM TRIGGERED"); // Post message as T
                bufferBlock.Complete(); // Signal end of processing
                cancellationToken.ThrowIfCancellationRequested(); // Trigger cancellation
            }
            else
            {
                bufferBlock.Post(token); // Post filtered token to bufferBlock
            }
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded, CancellationToken = cancellationToken });

        bufferBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // Asynchronously process tokens from Inference
        await foreach (var token in tokens)
        {
            cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
            await bufferBlock.SendAsync(token);
        }

        bufferBlock.Complete();

        // Stream back the processed tokens
        while (await bufferBlock.OutputAvailableAsync())
        {
            var processedToken = await bufferBlock.ReceiveAsync();
            yield return processedToken;
        }
    }

    public Task<bool> IsFirstRun(string type)
    {
        if (type == nameof(StatefulExecutorBase) && _inference is StatefulExecutorBase executer)
            return Task.FromResult(((InteractiveExecutorState)executer.GetStateData()).IsPromptRun);

        return Task.FromResult(false);
    }
}
