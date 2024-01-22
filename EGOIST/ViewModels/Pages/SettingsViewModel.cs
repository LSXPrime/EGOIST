using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;
using EGOIST.Data;
using EGOIST.Helpers;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using Notification.Wpf;
using System.IO.Compression;
using System.Reflection;

namespace EGOIST.ViewModels.Pages;
public partial class SettingsViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private ThemeType _currentTheme = ThemeType.Dark;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    private NotificationManager notification = new();
    private AppConfig _config = AppConfig.Instance;
    public IEnumerable ThemesValues => Enum.GetValues(typeof(ThemeType));

    public AppConfig Config
    {
        get => _config;
        set
        {
            _config = value;
            OnPropertyChanged(nameof(Config));
        }
    }

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        CurrentTheme = Theme.GetAppTheme();
    //    AppVersion = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty}";

        _isInitialized = true;
    }

    [RelayCommand]
    private void SwitchTheme(string parameter)
    {
        Theme.Apply(CurrentTheme);
    }

    [RelayCommand]
    private void ModelsPathBrowse_Click(string targetPath)
    {
        var dialog = new FolderBrowserDialog();
        DialogResult result = dialog.ShowDialog();

        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            switch (targetPath)
            {
                case "Path_Models":
                    Config.ModelsPath = dialog.SelectedPath;
                    break;
                case "Path_Voices":
                    Config.VoicesPath = dialog.SelectedPath;
                    break;
                case "Path_Results":
                    Config.ResultsPath = dialog.SelectedPath;
                    break;
            }
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        Config.Save();
    }

    [RelayCommand]
    private void ResetSettings()
    {
        Config.Reset();
    }

    [RelayCommand]
    private async Task StartBackend()
    {
        Extensions.Notify(new NotificationContent { Title = "EGOIST - Backend", Message = $"Backend is warming up.", Type = NotificationType.Information }, areaName: "NotificationArea");
        await InitlizeBackend();
        await CheckBackendRequirements();
        await FinalizeBackend();
        Extensions.Notify(new NotificationContent { Title = "EGOIST - Backend", Message = $"Backend started successfully.", Type = NotificationType.Information }, areaName: "NotificationArea");
    }

    private async Task InitlizeBackend()
    {
        var resourceName = "EGOIST.Assets.Backend.zip";
        Assembly assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        // Define the extraction directory
        var extractionPath = $"{Directory.GetCurrentDirectory()}\\Backend";
        var extractedZipPath = $"{extractionPath}\\Backend.zip";

        var embeddedHash = stream.CalculateMD5Hash();

        if (File.Exists(extractedZipPath))
        {
            // Compute the hash of the extracted Backend.zip
            using var extractedStream = File.OpenRead(extractedZipPath);
            var extractedHash = extractedStream.CalculateMD5Hash();

            // Compare the hashes to check for differences
            if (embeddedHash == extractedHash)
                return;
        }

        // Create the extraction directory if it doesn't exist
        if (!Directory.Exists(extractionPath))
            Directory.CreateDirectory(extractionPath);

        // Extract all files and folders from the archive
        foreach (var entry in archive.Entries)
        {
            var entryFullName = $"{extractionPath}\\{entry.FullName}";

            // If entry is a directory, create it
            if (entry.FullName.EndsWith("/"))
            {
                Directory.CreateDirectory(entryFullName);
                continue;
            }

            // Ensure the extracted file's parent directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(entryFullName));

            // Extract the file
            entry.ExtractToFile(entryFullName, overwrite: true);
        }

        // Write the original ZIP file to the extraction directory
        using var fileStream = File.Create(extractedZipPath);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
    }

    private async Task CheckBackendRequirements()
    {
        // The embedded batch script as a string
        string batchScript = $@"
@echo off
cd /d ""{Directory.GetCurrentDirectory()}\Backend""

if not exist ""venv"" (
    echo Creating Python virtual environment...
    python -m venv venv
    echo Python virtual environment creation started at %VENV_DIR%
    
    :check_venv
    if exist ""venv\Scripts\activate.bat"" (
        echo Python virtual environment created successfully at ""{Directory.GetCurrentDirectory()}\Backend\venv""
        goto :end
    ) else (
        timeout /t 2 >nul
        goto :check_venv
    )
) else (
    echo Python virtual environment already exists at ""{Directory.GetCurrentDirectory()}\Backend\venv""
    goto :end
)

:end
call ""venv\Scripts\python""  ""{Directory.GetCurrentDirectory()}\Backend\check_requirements.py""
";

        // Save the batch script to a temporary file
        string tempBatchFile = Path.Combine(Path.GetTempPath(), "egoist_check_requirements.bat");
        File.WriteAllText(tempBatchFile, batchScript);

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = tempBatchFile,
            WindowStyle = ProcessWindowStyle.Normal,
            RedirectStandardOutput = true
        };

        using var process = Process.Start(start);
        using var reader = process.StandardOutput;
        var result = "";
        while (!process.HasExited)
        {
            result += await reader.ReadToEndAsync();
        }

        // Check if the completion message was received
        if (result.Contains("Installation Complete"))
            Extensions.Notify(new NotificationContent { Title = "EGOIST - Backend", Message = $"All dependencies installed.", Type = NotificationType.Information }, areaName: "NotificationArea");
    }

    private async Task FinalizeBackend()
    {
        // The embedded batch script as a string
        string batchScript = $@"
@echo off
set ""VENV_DIR={Directory.GetCurrentDirectory()}\Backend\venv""

call ""%VENV_DIR%\Scripts\activate.bat""
python ""{Directory.GetCurrentDirectory()}\Backend\EGOIST_Backend.py""
pause
";

        // Save the batch script to a temporary file
        string tempBatchFile = Path.Combine(Path.GetTempPath(), "egoist_backend_start.bat");
        File.WriteAllText(tempBatchFile, batchScript);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = tempBatchFile,
            WindowStyle = ProcessWindowStyle.Normal
        };
        Process.Start(startInfo);
    }

    [RelayCommand]
    public async Task CheckForUpdate()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "request");

        var response = await client.GetAsync($"https://api.github.com/repos/LSXPrime/EGOIST/releases/latest");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            dynamic releaseInfo = JsonConvert.DeserializeObject(json);
            string downloadUrl = string.Empty;

            foreach (var asset in releaseInfo.assets)
            {
                if (asset.name == "EGOIST.exe")
                {
                    downloadUrl = asset.browser_download_url;
                }
            }

            if (releaseInfo != null && releaseInfo.tag_name != AppVersion)
            {
                
                /*
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseColorCode().Build();
                var htmlContent = Markdig.Markdown.ToHtml((string)releaseInfo.body, pipeline);
            //    var tempHtmlFile = $"{Path.GetTempPath()}\\egoist_update_notes.html";
            //    File.WriteAllText(tempHtmlFile, htmlContent);
                var mdRenderer = new WebView2();
                mdRenderer.Source = new Uri(Path.Combine(Path.GetTempPath(), @"egoist_update_notes.html"));
                // mdRenderer.NavigateToString(htmlContent);
                */

                var VoiceNameR = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Update Available",
                    Content =  $"Update {releaseInfo.tag_name} is available to download.\n\n{releaseInfo.body}",
                    PrimaryButtonText = "Download",
                    CloseButtonText = "Cancel",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var result = await VoiceNameR.ShowDialogAsync();

                if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    Extensions.Notify(new NotificationContent { Title = "EGOIST", Message = $"Update {releaseInfo.tag_name} started downloading.", Type = NotificationType.Information }, areaName: "NotificationArea");

                    var responseDownload = await client.GetAsync(downloadUrl);
                    if (responseDownload.IsSuccessStatusCode)
                    {
                        var content = await responseDownload.Content.ReadAsByteArrayAsync();

                        // Save the downloaded content to the specified location
                        File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\EGOIST.exe.update", content);
                        FinalizeUpdate();
                    }
                }
            }
            else
                Extensions.Notify(new NotificationContent { Title = "EGOIST", Message = $"No available Updates yet.", Type = NotificationType.Information }, areaName: "NotificationArea");
        }

        static void FinalizeUpdate()
        {
            // The embedded batch script as a string
            string batchScript = $@"
@echo off
set ""APP_NAME=EGOIST.exe""
set ""DOWNLOAD_PATH={Directory.GetCurrentDirectory()}\EGOIST.exe.update""
set ""APP_PATH={Directory.GetCurrentDirectory()}\%APP_NAME%""

REM Close the running application
taskkill /IM %APP_NAME% /F

REM Replace the old app with the downloaded one and keep the same name
del /F /Q ""%APP_PATH%"" > nul
ren ""%DOWNLOAD_PATH%"" ""%APP_NAME%""
start %APP_PATH%
";

            // Save the batch script to a temporary file
            string tempBatchFile = Path.Combine(Path.GetTempPath(), "egoist_update.bat");
            File.WriteAllText(tempBatchFile, batchScript);

            // Execute the batch file
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = tempBatchFile,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(processInfo);
        }
    }
}
