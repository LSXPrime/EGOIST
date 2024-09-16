namespace EGOIST.Domain.Enums;

/// <summary>
/// Represents different types of computing devices.
/// </summary>
public enum Device : short
{
    /// <summary>
    /// Represents a Central Processing Unit (CPU).
    /// </summary>
    Cpu = 0,

    /// <summary>
    /// Represents a Graphics Processing Unit (GPU) of Nvidia type with CUDA support.
    /// </summary>
    Cuda = 1,
    
    /// <summary>
    /// Represents a Graphics Processing Unit (GPU) of with Vulkan support.
    /// </summary>
    Vulkan = 2
}