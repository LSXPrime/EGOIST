using System.Collections.ObjectModel;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Application.Interfaces.Text;

/// <summary>
/// Defines the interface for a text service, providing methods for text generation, creation, and deletion.
/// </summary>
public interface ITextService<TSession> where TSession : ISession
{
    ObservableCollection<TextPromptParameters> PromptTemplates { get; set; }
    ObservableCollection<TSession> Sessions { get; set; }
    TSession? SelectedSession { get; set; } 
    
    /// <summary>
    /// Loads an existing text generation session.
    /// </summary>
    /// <param name="sessionName">The name of the session to load.</param>
    /// <param name="loadCache">Whether to load the saved session and model state.</param>
    /// <param name="parameter">Optional parameter for session loading.</param>
    /// <returns>A task that completes when the text item is loaded.</returns>
    Task<bool> LoadSession(string sessionName, bool loadCache = true, Dictionary<string, object>? parameter = null);
    
    /// <summary>
    /// Creates a new text generation session.
    /// </summary>
    /// <param name="parameter">Optional parameters for text creation.</param>
    /// <returns>A task that completes when the text item is created.</returns>
    Task<bool> Create(Dictionary<string, object>? parameter = null);

    /// <summary>
    /// Deletes an existing text generation session.
    /// </summary>
    /// <param name="parameter">Optional parameter for text deletion.</param>
    /// <returns>A task that completes when the text item is deleted.</returns>
    Task<bool> Delete(string parameter = "");

    /// <summary>
    /// Generates text based on a given prompt.
    /// </summary>
    /// <param name="prompt">The prompt for text generation.</param>
    /// <param name="generationParameters">The response generation parameters for text generation.</param>
    /// <param name="promptParameters">The prompt formatting parameters for text generation.</param>
    /// <param name="citations"></param>
    /// <returns>A task that completes when the text is generated.</returns>
    Task<T?> Generate<T>(string prompt, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null, Citation[]? citations = null) where T : class;
    
    /// <summary>
    /// Disposes the text service.
    /// </summary>
    /// <returns>A task that completes when the text service is disposed.</returns>
    Task Dispose();
}