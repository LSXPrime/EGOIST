namespace EGOIST.Application.Interfaces.Text
{
    /// <summary>
    /// Defines the interface for a text service, providing methods for text generation, creation, and deletion.
    /// </summary>
    public interface ITextService
    {
        /// <summary>
        /// Creates a new text item.
        /// </summary>
        /// <param name="parameter">Optional parameter for text creation.</param>
        /// <returns>A task that completes when the text item is created.</returns>
        Task Create(string parameter = "");

        /// <summary>
        /// Deletes an existing text item.
        /// </summary>
        /// <param name="parameter">Optional parameter for text deletion.</param>
        /// <returns>A task that completes when the text item is deleted.</returns>
        Task Delete(string parameter = "");

        /// <summary>
        /// Generates text based on a given prompt.
        /// </summary>
        /// <param name="prompt">The prompt for text generation.</param>
        /// <returns>A task that completes when the text is generated.</returns>
        Task Generate(string prompt);
    }
}