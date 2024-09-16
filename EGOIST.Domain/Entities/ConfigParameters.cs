using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Enums;

namespace EGOIST.Domain.Entities;

public class ConfigParameters : EntityBase
{
    public string ApiHost
    {
        get => _apiHost;
        set => Notify(ref _apiHost, value);
    }

    public int ApiPort
    {
        get => _apiPort;
        set => Notify(ref _apiPort, value);
    }

    public string DataSecretKey
    {
        get => _dataSecretKey;
        set => Notify(ref _dataSecretKey, value);
    }

    public string CurrentBackground
    {
        get => _currentBackground;
        set => Notify(ref _currentBackground, value);
    }

    public string ModelsPath
    {
        get => _modelsPath;
        set => Notify(ref _modelsPath, value);
    }

    public string MemoriesPath
    {
        get => _memoriesPath;
        set => Notify(ref _memoriesPath, value);
    }

    public string PromptsPath
    {
        get => _promptsPath;
        set => Notify(ref _promptsPath, value);
    }

    public string VoicesPath
    {
        get => _voicesPath;
        set => Notify(ref _voicesPath, value);
    }

    public string ResultsPath
    {
        get => _resultsPath;
        set => Notify(ref _resultsPath, value);
    }

    public string CharactersPath
    {
        get => _charactersPath;
        set => Notify(ref _charactersPath, value);
    }

    public string WorldMemoriesPath
    {
        get => _worldMemoriesPath;
        set => Notify(ref _worldMemoriesPath, value);
    }

    public string BackgroundsPath
    {
        get => _backgroundsPath;
        set => Notify(ref _backgroundsPath, value);
    }

    public string CachePath
    {
        get => _cachePath;
        set => Notify(ref _cachePath, value);
    }

    public Device Device
    {
        get => _device;
        set => Notify(ref _device, value);
    }

    public bool ChatMemory
    {
        get => _chatMemory;
        set => Notify(ref _chatMemory, value);
    }

    public string MemoryPrompt
    {
        get => _memoryPrompt;
        set => Notify(ref _memoryPrompt, value);
    }

    public string GlobalMemoryPrompt
    {
        get => _globalMemoryPrompt;
        set => Notify(ref _globalMemoryPrompt, value);
    }
    
    public bool HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set => Notify(ref _hardwareAcceleration, value);
    }

    private static readonly string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private string _apiHost = "http://127.0.0.1";
    private int _apiPort = 8000;
    private string _dataSecretKey = "USER_SECRET_KEY_TO_DECRYPT_DATA";
    private string _memoryPrompt =
        "Answer the following question based only on the context provided below. Do not use any prior knowledge or external information. If the context does not contain the information needed to answer the question, respond with \"I cannot answer this question based on the provided context.";
    private string _globalMemoryPrompt =
        "Previously, we discussed some related topics, which you can see below for context. You don't need to directly answer based on this information, but it might help you understand my current input better.";
    private string _modelsPath = @$"{BasePath}\Models";
    private string _memoriesPath = @$"{BasePath}\Memories";
    private string _promptsPath = @$"{BasePath}\Prompts";
    private string _voicesPath = $@"{BasePath}\Voices";
    private string _resultsPath = @$"{BasePath}\Results";
    private string _charactersPath = @$"{BasePath}\Characters";
    private string _worldMemoriesPath = @$"{BasePath}\WorldMemories";
    private string _backgroundsPath = $@"{BasePath}\Backgrounds";
    private string _cachePath = $@"{BasePath}\Cache";
    private Device _device = Device.Vulkan;
    private string _currentBackground = string.Empty;
    private bool _chatMemory = true;
    private bool _hardwareAcceleration = true;
}