using System.Collections;
using System.Diagnostics;
using System.Text.Json;
using EGOIST.Domain.Enums;

namespace EGOIST.Application.Services.Utilities;

public class AppConfig
{
    public readonly string AppVersion = "1.1.1";
    public string ApiUrl => $"{ApiHost}/{ApiPort}";

    public string ApiHost { get; set; } = "http://127.0.0.1";
    public int ApiPort { get; set; } = 8000;
    public string DataSecretKey { get; set; } = "USER_SECRET_KEY_TO_DECRYPT_DATA";
    public string ModelsPath { get; set; } = @"C:\External\Models";// $@"{Directory.GetCurrentDirectory()}\Resources\Models\Checkpoints";
    public string VoicesPath { get; set; } = $@"{Directory.GetCurrentDirectory()}\Resources\Voices";
    public string ResultsPath { get; set; } = $@"{Directory.GetCurrentDirectory()}\Resources\Results";
    public string CharactersPath { get; set; } = $@"{Directory.GetCurrentDirectory()}\Resources\Characters";
    public string BackgroundsPath { get; set; } = $@"{Directory.GetCurrentDirectory()}\Resources\Backgrounds";
    public Device Device { get; set; } = Device.CPU;

    public IEnumerable DeviceValues => Enum.GetValues(typeof(Device));

    private readonly string _configFilePath = $"{Directory.GetCurrentDirectory()}\\Config.json";

    private AppConfig() { }

    public void Load()
    {
        if (File.Exists(_configFilePath))
        {
            var json = File.ReadAllText(_configFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ApiHost = config!.ApiHost;
            ApiPort = config.ApiPort;
            DataSecretKey = config.DataSecretKey;
            ModelsPath = config.ModelsPath;
            VoicesPath = config.VoicesPath;
            ResultsPath = config.ResultsPath;
            CharactersPath = config.CharactersPath;
            BackgroundsPath = config.BackgroundsPath;
            Device = config.Device;
        }

        CheckPaths();
        Save();
    }

    private void CheckPaths()
    {
        Directory.CreateDirectory(ModelsPath);
        Directory.CreateDirectory(VoicesPath);
        Directory.CreateDirectory(ResultsPath);
        Directory.CreateDirectory(CharactersPath);
        Directory.CreateDirectory(BackgroundsPath);
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFilePath, json);
        ExportToBackend();
        CheckPaths();
    }

    public void Reset()
    {
        if (File.Exists(_configFilePath))
            File.Delete(_configFilePath);

        Load();
    }

    public void ExportToBackend()
    {
        var backendPath = Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\Backend");
        using var file = new StreamWriter($"{backendPath}\\.env");

        file.WriteLine($"HOST_IP={new Uri(ApiHost).Host}");
        file.WriteLine($"HOST_PORT={ApiPort}");
        file.WriteLine($"DEVICE={Device}");
        file.WriteLine($"MODELS_PATH={ModelsPath}");
        file.WriteLine($"VOICES_PATH={VoicesPath}");
        file.WriteLine($"RESULTS_PATH={ResultsPath}");
        file.WriteLine($"CHARACTER_PATH={CharactersPath}");
        file.WriteLine($"BACKGROUNDS_PATH={BackgroundsPath}");
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

    private static AppConfig? _instance;
    public static AppConfig Instance => _instance ??= new AppConfig();
}
