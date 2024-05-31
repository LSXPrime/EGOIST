using EGOIST.Application.Utilities;
using System.IO.Compression;
using System.Reflection;
using System.Diagnostics;

namespace EGOIST.Application.Services.Utilities;

public class BackendService
{
    private static BackendService? _instance;

    public static BackendService Instance => _instance ??= new BackendService();

    private BackendService() { }

    /// <summary>
    /// Initializes the backend by extracting assets and checking requirements.
    /// </summary>
    /// <returns>A task that completes when the backend initialization is finished.</returns>
    public async Task Start()
    {
        await InitializeBackend();
        await CheckBackendRequirements();
        FinalizeBackend();
    }

    /// <summary>
    /// Extracts backend assets from the embedded resource.
    /// </summary>
    /// <returns>A task that completes when the assets are extracted.</returns>
    private async Task InitializeBackend()
    {
        const string backendZipFile = "EGOIST.Assets.Backend.zip";
        var backendPath = Path.Combine(Directory.GetCurrentDirectory(), "Backend");
        var backendZipPath = Path.Combine(backendPath, "Backend.zip");

        // Check if the backend assets already exist and have the correct hash
        if (File.Exists(backendZipPath))
        {
            var existingHash = File.OpenRead(backendZipPath).CalculateMD5Hash();
            var embeddedHash = Assembly.GetExecutingAssembly().GetManifestResourceStream(backendZipFile).CalculateMD5Hash();
            if (existingHash == embeddedHash)
            {
                return;
            }
        }

        // Create the backend directory if it doesn't exist
        if (!Directory.Exists(backendPath))
        {
            Directory.CreateDirectory(backendPath);
        }

        // Extract the embedded backend zip file
        await using var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(backendZipFile);
        using var zipArchive = new ZipArchive(embeddedStream, ZipArchiveMode.Read);

        foreach (var entry in zipArchive.Entries)
        {
            var fullPath = Path.Combine(backendPath, entry.FullName);

            // Extract files
            if (!entry.FullName.EndsWith("/"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                entry.ExtractToFile(fullPath, true);
            }
            // Create directories
            else
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        // Save the extracted zip file
        await using var fileStream = File.Create(backendZipPath);
        embeddedStream.Seek(0, SeekOrigin.Begin);
        await embeddedStream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// Checks if the backend dependencies are met.
    /// </summary>
    /// <returns>A task that completes when the requirements check is finished.</returns>
    private async Task CheckBackendRequirements()
    {
        // Add your logic to check the backend requirements here.
        // This could include verifying the presence of Python, specific packages, etc.
        // Example:
        //  var process = new Process();
        //  process.StartInfo.FileName = "python";
        //  process.StartInfo.Arguments = "--version";
        //  process.StartInfo.RedirectStandardOutput = true;
        //  process.StartInfo.UseShellExecute = false;
        //  process.Start();
        //  var output = process.StandardOutput.ReadToEnd();
        //  // Check the output for the expected Python version.
    }

    /// <summary>
    /// Finalizes the backend by creating a batch script to start it.
    /// </summary>
    private void FinalizeBackend()
    {
        var scriptContent = $@"
@echo off
set ""VENV_DIR={Directory.GetCurrentDirectory()}\Backend\venv""

call ""%VENV_DIR%\Scripts\activate.bat""
python ""{Directory.GetCurrentDirectory()}\Backend\EGOIST_Backend.py""
pause
";

        var scriptPath = Path.Combine(Path.GetTempPath(), "egoist_backend_start.bat");
        File.WriteAllText(scriptPath, scriptContent);

        // Start the backend script
        var process = new Process();
        process.StartInfo.FileName = scriptPath;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.Start();
    }
}