namespace EGOIST.Domain.Enums;

/// <summary>
/// Represents the role of an author in a conversation or interaction.
/// </summary>
public enum AuthorRole : short
{
    /// <summary>
    /// Represents a user, typically the human interacting with the system.
    /// </summary>
    User,

    /// <summary>
    /// Represents an assistant, typically an AI model responding to user input.
    /// </summary>
    Assistant,

    /// <summary>
    /// Represents a system, often used for initial instructions or context setting.
    /// </summary>
    System,

    /// <summary>
    /// Represents a tool, indicating a specific tool or functionality used within the interaction.
    /// </summary>
    Tool
}