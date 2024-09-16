using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Presentation.UI.ViewModels.Pages;

public partial class ImagePageViewModel : ViewModelBase, INavigationAware
{
    public override string Title => "Image";

    [ObservableProperty] private ObservableCollection<ModelInfo> _models = [];
    [ObservableProperty] private ObservableCollection<ModelInfoWeight> _vaeWeights = [];
    [ObservableProperty] private ObservableCollection<ModelInfoWeight> _controlNetWeights = [];
    [ObservableProperty] private ObservableCollection<ModelInfoWeight> _taesdWeights = [];
    [ObservableProperty] private ModelInfo? _selectedGenerationModel;
    [ObservableProperty] private ModelInfoWeight? _selectedGenerationWeight;
    [ObservableProperty] private ModelInfoWeight? _selectedVaeWeight;
    [ObservableProperty] private ModelInfoWeight? _selectedControlNetWeight;
    [ObservableProperty] private ModelInfoWeight? _selectedTaesdWeight;

    public IModelCoreService ModelCoreService { get; }
    private readonly IModelsRepository _localRepository;


    public ImagePageViewModel([FromKeyedServices("LocalModelsRepository")] IModelsRepository localRepository,
        [FromKeyedServices("ImageModelCoreService")]
        IModelCoreService modelCoreService)
    {
        _localRepository = localRepository;
        ModelCoreService = modelCoreService;
        _ = RefreshModels();
    }

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;


    public Task OnNavigatedFrom() => Task.CompletedTask;


    public Task OnNavigatedTo() => Task.CompletedTask;

    [RelayCommand]
    private async Task RefreshModels()
    {
        var allModels = (await _localRepository.GetAllModels(new Dictionary<string, string>()
            { { "Type", "Image" }, { "WeightExtension", ".gguf,.ckpt,.safetensors"} })).ToArray();
        Models = new ObservableCollection<ModelInfo>(allModels.Where(x => x.Task == "Generation"));
        ControlNetWeights = new ObservableCollection<ModelInfoWeight>(allModels
            .Where(x => x.Task == "ControlNet")
            .SelectMany(x => x.Weights));
        TaesdWeights = new ObservableCollection<ModelInfoWeight>(allModels
            .Where(x => x.Task == "Taesd")
            .SelectMany(x => x.Weights));
    }

    [RelayCommand]
    private async Task SwitchModel() =>
        await ModelCoreService.Switch(SelectedGenerationModel, SelectedGenerationWeight,
            new Dictionary<string, object?>
                { { "ControlNet", SelectedControlNetWeight }, { "Taesd", SelectedTaesdWeight }, { "Vae", SelectedVaeWeight } });

    [RelayCommand]
    private async Task UnloadModel()
    {
        SelectedGenerationModel = null;
        SelectedGenerationWeight = null;
        SelectedControlNetWeight = null;
        SelectedTaesdWeight = null;
        SelectedVaeWeight = null;
        await ModelCoreService.Unload();
    }
}