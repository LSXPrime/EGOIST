namespace EGOIST.Domain.Enums;

/// <summary>
/// Represents the frequency of interaction a roleplay character has with others.
/// </summary>
public enum RpCharacterInteractionFrequency : short
{
    /// <summary>
    /// The character is shy and interacts infrequently.
    /// </summary>
    Shy,

    /// <summary>
    /// The character interacts with others at a normal frequency.
    /// </summary>
    Normal,

    /// <summary>
    /// The character is chatty and interacts frequently.
    /// </summary>
    Chatty
}