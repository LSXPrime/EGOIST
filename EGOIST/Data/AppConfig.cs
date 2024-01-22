using System.Collections;
using System.IO;
using EGOIST.Enums;

namespace EGOIST.Data;

public partial class AppConfig : ObservableObject
{
    #region API
    public string ApiUrl => string.Format("{0}:{1}", ApiHost, ApiPort);

    [ObservableProperty]
    private string _apiHost = "http://127.0.0.1";
    [ObservableProperty]
    private int _apiPort = 8000;
    [ObservableProperty]
    private string _dataSecretKey = "USER_SECRET_KEY_TO_DECRYPT_DATA";
    #endregion
    #region Paths
    [ObservableProperty]
    private string _modelsPath = string.Empty;
    [ObservableProperty]
    private string _voicesPath = string.Empty;
    [ObservableProperty]
    private string _resultsPath = string.Empty;
    private static readonly string filePath = $"{Directory.GetCurrentDirectory()}\\Config.json";
    #endregion
    #region Inference
    [ObservableProperty]
    public Device _device = Device.GPU;
    public static IEnumerable DeviceValues => Enum.GetValues(typeof(Device));
    #endregion

    public AppConfig()
    {
        ApiHost = "http://127.0.0.1";
        ApiPort = 8000;
        ModelsPath = $"{Directory.GetCurrentDirectory()}\\Resources\\Models\\Checkpoints";
        VoicesPath = $"{Directory.GetCurrentDirectory()}\\Resources\\Voices";
        ResultsPath = $"{Directory.GetCurrentDirectory()}\\Resources\\Results";
    }

    // Load settings from a JSON file
    public void LoadSelf()
    {
        AppConfig self = new();
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            self = Newtonsoft.Json.JsonConvert.DeserializeObject<AppConfig>(json);
        }

        ApiHost = "http://127.0.0.1";
        ApiPort = 8000;
        ModelsPath = self.ModelsPath;
        VoicesPath = self.VoicesPath;
        ResultsPath = self.ResultsPath;
        Device = self.Device;

        CheckPaths();
        Save();
    }

    private void CheckPaths()
    {
        var transcribeModelsPath = ModelsPath + "\\VoiceGeneration\\transcribe";
        var cloneModelsPath = ModelsPath + "\\VoiceGeneration\\clone";
        Directory.CreateDirectory(transcribeModelsPath);
        Directory.CreateDirectory(cloneModelsPath);
        Directory.CreateDirectory(VoicesPath);
        Directory.CreateDirectory(ResultsPath+ "\\VoiceGeneration");
    }

    // Save settings to a JSON file
    public void Save()
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, json);
        ExportToBackend();
        CheckPaths();
        ConfigSavedEvent?.Invoke();
    }

    public void Reset()
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        ApiHost = "http://127.0.0.1";
        ApiPort = 8000;
        ModelsPath = Directory.GetCurrentDirectory() + "\\Resources\\Models\\Checkpoints";
        VoicesPath = Directory.GetCurrentDirectory() + "\\Resources\\Voices";
        ResultsPath = Directory.GetCurrentDirectory() + "\\Resources\\Results";
        Save();
    }

    // For backend config
    public void ExportToBackend()
    {
        var backendPath = Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Backend");
        using StreamWriter file = new StreamWriter($"{backendPath}\\.env");

        file.WriteLine($"HOST_IP={new Uri(ApiHost).Host}");
        file.WriteLine($"HOST_PORT={ApiPort}");
        file.WriteLine($"DEVICE={Device}");
        file.WriteLine($"MODELS_PATH={ModelsPath}");
        file.WriteLine($"VOICES_PATH={VoicesPath}");
        file.WriteLine($"RESULTS_PATH={ResultsPath}");
        file.Close();
    }

    public delegate void ConfigSavedDelegate();
    public event ConfigSavedDelegate ConfigSavedEvent;

    private static AppConfig? instance;
    public static AppConfig Instance => instance ??= new AppConfig();
}
