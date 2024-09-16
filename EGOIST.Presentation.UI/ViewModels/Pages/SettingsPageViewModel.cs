using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;

namespace EGOIST.Presentation.UI.ViewModels.Pages;

public partial class SettingsPageViewModel
    : ViewModelBase, INavigationAware
{
    public override string Title => "Settings";

    [ObservableProperty] private Dictionary<string, string> _backgrounds = [];
    [ObservableProperty] private KeyValuePair<string, string> _currentBackground;
    [ObservableProperty] private string _device = Config.Parameters.Device.ToString();

    public static AppConfig Config => AppConfig.Instance;

    public Task Initialize(Dictionary<string, object>? parameters)
    {
        Backgrounds = Directory.GetFiles(AppConfig.Instance.Parameters.BackgroundsPath).Prepend("")
            .ToDictionary(x => string.IsNullOrEmpty(x) ? "Transparent" : Path.GetFileNameWithoutExtension(x), x => x);
        CurrentBackground = new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(Config.Parameters.CurrentBackground), Config.Parameters.CurrentBackground);
        return Task.CompletedTask;
    }


    public Task OnNavigatedFrom() => Task.CompletedTask;


    public Task OnNavigatedTo() => Task.CompletedTask;

    partial void OnCurrentBackgroundChanged(KeyValuePair<string, string> value) => Config.Parameters.CurrentBackground = value.Value;

    partial void OnDeviceChanged(string value) => Config.Parameters.Device = Enum.Parse<Device>(value);


    [RelayCommand]
    private async Task Browse(string type)
    {
        var path = await DialogService.OpenFolderDialogAsync();
        if (string.IsNullOrEmpty(path))
            return;

        switch (type)
        {
            case "Models":
                Config.Parameters.ModelsPath = path;
                break;
            case "Memories":
                Config.Parameters.MemoriesPath = path;
                break;
            case "Voices":
                Config.Parameters.VoicesPath = path;
                break;
            case "Prompts":
                Config.Parameters.PromptsPath = path;
                break;
            case "Characters":
                Config.Parameters.CharactersPath = path;
                break;
            case "WorldMemories":
                Config.Parameters.WorldMemoriesPath = path;
                break;
            case "Backgrounds":
                Config.Parameters.BackgroundsPath = path;
                break;
            case "Results":
                Config.Parameters.ResultsPath = path;
                break;
        }
    }

    [RelayCommand]
    private static void SaveSettings()
    {
        Config.Save();
    }

    [RelayCommand]
    private static void ResetSettings()
    {
        Config.Reset();
    }
}