using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Interfaces;

public interface IInference
{
    Task<bool> IsFirstRun(string type);
    IAsyncEnumerable<T> Inference<T>(string prompt, IEnumerable<T> blacklist, TextGenerationParameters parameters, CancellationToken cancellationToken);
    IAsyncEnumerable<T> InferenceConcurrent<T>(string prompt, IEnumerable<T> blacklist, TextGenerationParameters parameters, CancellationToken cancellationToken);
    void Dispose();
}
