using EGOIST.Domain.Interfaces;

namespace EGOIST.Infrastructure.Services.Storage;

/// <summary>
/// Provides file system operations using the `System.IO` namespace.
/// </summary>
public class FileSystemService : IFileSystemService
{
    /// <summary>
    /// Copies a file from one location to another.
    /// </summary>
    /// <param name="sourcePath">The path to the source file.</param>
    /// <param name="destinationPath">The path to the destination file.</param>
    /// <param name="overwrite">Indicates whether to overwrite an existing file at the destination path.</param>
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        File.Copy(sourcePath, destinationPath, overwrite);
    }

    /// <summary>
    /// Creates a new directory at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to create.</param>
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Deletes a directory at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to delete.</param>
    /// <param name="recursive">Indicates whether to delete subdirectories recursively.</param>
    public void DeleteDirectory(string path, bool recursive)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive);
    }

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    public void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to check.</param>
    /// <returns>True if the directory exists, false otherwise.</returns>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The size of the file in bytes.</returns>
    public long GetFileSize(string filePath)
    {
        return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
    }

    public FileStream? Open(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read,
        FileShare share = FileShare.ReadWrite)
    {
        return FileExists(path) ? new FileStream(path, mode, access, share) : null;
    }

    /// <summary>
    /// Reads the entire contents of a text file.
    /// </summary>
    /// <param name="path">The path to the text file.</param>
    /// <returns>The contents of the file as a string.</returns>
    public string ReadAllText(string path)
    {
        return FileExists(path) ? File.ReadAllText(path) : string.Empty;
    }

    /// <summary>
    /// Writes text to a file.
    /// </summary>
    /// <param name="path">The path to the file to write to.</param>
    /// <param name="content">The text content to write to the file.</param>
    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Reads the entire contents of a text file asynchronously.
    /// </summary>
    /// <param name="path">The path to the text file.</param>
    /// <returns>A task that completes with the contents of the file.</returns>
    public async Task<string> ReadAllTextAsync(string path)
    {
        return FileExists(path) ? await File.ReadAllTextAsync(path, CancellationToken.None) : string.Empty;
    }

    /// <summary>
    /// Writes text to a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to write to.</param>
    /// <param name="content">The text content to write to the file.</param>
    /// <returns>A task that completes when the text is written to the file.</returns>
    public async Task WriteAllTextAsync(string path, string content)
    {
        await File.WriteAllTextAsync(path, content, CancellationToken.None);
    }
}