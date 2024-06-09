namespace EGOIST.Domain.Interfaces;

/// <summary>
/// Defines the interface for a repository that manages prompt templates.
/// </summary>
public interface IPromptRepository<TPromptTemplate> where TPromptTemplate : IPromptTemplate
{
    /// <summary>
    /// Gets all available prompt templates.
    /// </summary>
    /// <param name="parameters">A dictionary of optional parameters to filter the results.</param>
    /// <returns>An enumerable collection of <see cref="IPromptTemplate"/> objects representing all prompt templates.</returns>
    Task<IEnumerable<TPromptTemplate>> GetAllTemplates(Dictionary<string, string>? parameters = null);

    /// <summary>
    /// Gets all available prompt templates with optional query and model count.
    /// </summary>
    /// <param name="query">An optional search query to filter the results.</param>
    /// <param name="templatesCount">The maximum number of templates to retrieve.</param>
    /// <returns>An enumerable collection of <see cref="IPromptTemplate"/> objects representing the retrieved prompt templates.</returns>
    Task<IEnumerable<TPromptTemplate>> GetAllTemplates(string query = "", int templatesCount = 10);

    /// <summary>
    /// Gets a specific prompt template by its name.
    /// </summary>
    /// <param name="template">The name of the prompt template to retrieve.</param>
    /// <returns>The <see cref="IPromptTemplate"/> object representing the requested template, or null if not found.</returns>
    Task<TPromptTemplate?> GetTemplate(string template);
}