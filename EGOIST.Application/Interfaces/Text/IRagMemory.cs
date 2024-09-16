using EGOIST.Domain.Entities;

namespace EGOIST.Application.Interfaces.Text;

public interface IRagMemory
{
    /// <summary>
    /// Initializes the RAG memory service.
    /// </summary>
    /// <param name="args">The optional arguments for the initialization.</param>
    Task InitializeAsync(params object[] args);
    
    /// <summary>
    /// Disposes the RAG memory service.
    /// </summary>
    /// <param name="args">The optional arguments for the dispose process.</param>
    Task DisposeAsync(params object[] args);

    /// <summary>
    /// Saves a new memory item.
    /// </summary>
    /// <param name="key">The unique key for the memory item.</param>
    /// <param name="paths">The paths of the memory items to save.</param>
    /// <param name="parameters">The optional parameters for the save.</param>
    /// <param name="cancellationToken">The optional cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync(string key, IEnumerable<string> paths, Dictionary<string, string>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a memory item by its key.
    /// </summary>
    /// <param name="query">The question to search for.</param>
    /// <param name="paths">The keys of the memory items to retrieve.</param>
    /// <param name="parameters">The optional parameters for the retrieval.</param>
    /// <param name="cancellationToken">The optional cancellation token for the operation.</param>
    /// <returns>The value associated with the key, or null if the key is not found.</returns>
    Task<Citation[]> GetAsync(string query, IEnumerable<MemorySource> paths, Dictionary<string, string>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all memory items.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token for the operation.</param>
    /// <returns>A dictionary containing all memory items, with their keys as the keys and their values as the values.</returns>
    Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a memory item by its key.
    /// </summary>
    /// <param name="key">The key of the memory item to remove.</param>
    /// <param name="cancellationToken">The optional cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}