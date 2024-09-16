using System.Collections;
using System.Diagnostics;
using System.Text.Json;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;

namespace EGOIST.Application.Services.Utilities;

public class AppConfig
{
    public string AppVersion { get; } = "1.2.0";
    public string ApiUrl => $"{Parameters.ApiHost}/{Parameters.ApiPort}";

    public ConfigParameters Parameters { get; private set; } = new();


    private readonly string _configFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Config.json";
    
    public void Load()
    {
        if (File.Exists(_configFilePath))
        {
            var json = File.ReadAllText(_configFilePath);
            var config = JsonSerializer.Deserialize<ConfigParameters>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config != null)
                Parameters = config;
        }

        CheckPaths();
        Save();
    }

    private void CheckPaths()
    {
        Directory.CreateDirectory(Parameters.ModelsPath);
        Directory.CreateDirectory(Parameters.MemoriesPath);
        Directory.CreateDirectory(Parameters.PromptsPath);
        Directory.CreateDirectory(Parameters.VoicesPath);
        Directory.CreateDirectory(Parameters.ResultsPath);
        Directory.CreateDirectory(Parameters.CharactersPath);
        Directory.CreateDirectory(Parameters.WorldMemoriesPath);
        Directory.CreateDirectory(Parameters.BackgroundsPath);
        Directory.CreateDirectory(Parameters.CachePath);
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Parameters, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFilePath, json);
        CheckPaths();
    }

    public void Reset()
    {
        if (File.Exists(_configFilePath))
            File.Delete(_configFilePath);

        Parameters = new ConfigParameters();
        Load();
    }

    public async Task<string?> CheckForUpdate()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "request");

        var response = await client.GetAsync($"https://api.github.com/repos/LSXPrime/EGOIST/releases/latest");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var releaseInfo = JsonSerializer.Deserialize<dynamic>(json);
            if (releaseInfo != null && releaseInfo?.tag_name != AppVersion)
                foreach (var asset in releaseInfo!.assets)
                    if (asset.name == "EGOIST.exe")
                        return asset.browser_download_url;
        }

        return null;
    }

    public async Task DownloadUpdate(string downloadUrl)
    {
        using var client = new HttpClient();
        var responseDownload = await client.GetAsync(downloadUrl);
        if (responseDownload.IsSuccessStatusCode)
        {
            var content = await responseDownload.Content.ReadAsByteArrayAsync();

            // Save the downloaded content to the specified location
            await File.WriteAllBytesAsync($"{Directory.GetCurrentDirectory()}\\EGOIST.exe.update", content);
        }

        // The embedded batch script as a string
        var batchScript = $@"
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
        var tempBatchFile = Path.Combine(Path.GetTempPath(), "egoist_update.bat");
        await File.WriteAllTextAsync(tempBatchFile, batchScript);

        // Execute the batch file
        ProcessStartInfo processInfo = new()
        {
            FileName = tempBatchFile,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };

        Process.Start(processInfo);
    }

    private static Lazy<AppConfig> _instance = new(() => new AppConfig());

    public static AppConfig Instance => _instance.Value;
}