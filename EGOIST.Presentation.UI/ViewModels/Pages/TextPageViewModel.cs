using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Services.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages;

public partial class TextPageViewModel : ViewModelBase, INavigationAware
{
    public new string Title => "Text";

    public ObservableCollection<ModelInfo> Models { get; set; } = [];
    [ObservableProperty] 
    private ModelInfo? _selectedGenerationModel;
    [ObservableProperty]
    private ModelInfoWeight? _selectedGenerationWeight;
    private GenerationService GenerationService { get; } = GenerationService.Instance;
    private readonly IModelsRepository _localRepository;
    

    public TextPageViewModel([FromKeyedServices("LocalModelsRepository")] IModelsRepository localRepository)
    {
        _localRepository = localRepository;
        RefreshModels().Wait();
    }

    public void Initialize(Dictionary<string, object>? parameters) { }

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() { }

    [RelayCommand]
    private async Task RefreshModels() => Models = new ObservableCollection<ModelInfo>(await _localRepository.GetAllModels(new Dictionary<string, string>() { { "Type", "Text" } }));

    [RelayCommand]
    private void SwitchModel() => GenerationService.Switch(SelectedGenerationModel, SelectedGenerationWeight);

    [RelayCommand]
    private void UnloadModel() => GenerationService.Unload();
}
