namespace EGOIST.Domain.Interfaces;

/// <summary>
/// Defines the interface for a file system service, providing methods for file and directory operations.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Copies a file from one location to another.
    /// </summary>
    /// <param name="sourcePath">The path to the source file.</param>
    /// <param name="destinationPath">The path to the destination file.</param>
    /// <param name="overwrite">Indicates whether to overwrite an existing file at the destination path.</param>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);

    /// <summary>
    /// Creates a new directory at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to create.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Deletes a directory at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to delete.</param>
    /// <param name="recursive">Indicates whether to delete subdirectories recursively.</param>
    void DeleteDirectory(string path, bool recursive);

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    void DeleteFile(string path);

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to check.</param>
    /// <returns>True if the directory exists, false otherwise.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The size of the file in bytes.</returns>
    long GetFileSize(string filePath);

    /// <summary>
    /// Reads the entire contents of a text file asynchronously.
    /// </summary>
    /// <param name="path">The path to the text file.</param>
    /// <returns>A task that completes with the contents of the file.</returns>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>
    /// Writes text to a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to write to.</param>
    /// <param name="content">The text content to write to the file.</param>
    /// <returns>A task that completes when the text is written to the file.</returns>
    Task WriteAllTextAsync(string path, string content);
}