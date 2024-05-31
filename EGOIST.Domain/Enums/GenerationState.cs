namespace EGOIST.Domain.Enums;

/// <summary>
/// Represents the different states a generation process can be in.
/// </summary>
public enum GenerationState : short
{
    /// <summary>
    /// The generation process is not active.
    /// </summary>
    None,

    /// <summary>
    /// The generation process has started.
    /// </summary>
    Started,

    /// <summary>
    /// The generation process has finished.
    /// </summary>
    Finished
}