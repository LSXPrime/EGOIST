using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Image;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using StableDiffusion.NET;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Image;

public partial class UpscalePageViewModel : ViewModelBase, INavigationAware
{
    public override string Title => "Upscale";

    [ObservableProperty] private GenerationState _state = GenerationState.None;


    #region ImagesVariables

    [ObservableProperty] private byte[]? _sourceImage;
    [ObservableProperty] private byte[] _resultImage = [];
    [ObservableProperty] private ObservableCollection<HistoryEntryBase> _history = [];

    #endregion

    #region ModelVariables

    [ObservableProperty] private ObservableCollection<ModelInfo> _models = [];

    [ObservableProperty] private ModelInfo? _selectedModel;
    [ObservableProperty] private ModelInfoWeight? _selectedWeight;

    public GenerationState ModelState => _modelCoreService.State;

    #endregion

    #region GenerationVariables

    [ObservableProperty] private int _upscaleFactor = 4;
    [ObservableProperty] private double _progress;
    private readonly IHistoryRepository _historyRepository;
    private readonly IFileSystemService _fileSystemService;
    private readonly IModelsRepository _localRepository;
    private readonly IModelCoreService _modelCoreService;

    public UpscalePageViewModel([FromKeyedServices("UpscaleService")] IImageService service,
        IHistoryRepository historyRepository,
        IFileSystemService fileSystemService,
        [FromKeyedServices("LocalModelsRepository")]
        IModelsRepository localRepository,
        [FromKeyedServices("ImageModelCoreService")]
        IModelCoreService modelCoreService)
    {
        _historyRepository = historyRepository;
        _fileSystemService = fileSystemService;
        _localRepository = localRepository;
        _modelCoreService = modelCoreService;
        Service = service;

        _ = RefreshModels();
    }

    private IImageService Service { get; }

    #endregion

    #region Navigation

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;


    public Task OnNavigatedFrom()
    {
        StableDiffusionCpp.Progress -= OnProgressChanged;
        return Task.CompletedTask;
    }

    public async Task OnNavigatedTo()
    {
        StableDiffusionCpp.Progress += OnProgressChanged;

        History.Clear();
        foreach (var entry in await _historyRepository.GetAllHistory(new Dictionary<string, string>
                     { { "Type", "Image" } }))
        {
            History.Add(entry);
        }
    }

    #endregion

    #region History

    [RelayCommand]
    private async Task SelectEntry(HistoryEntryBase entry)
    {
        ResultImage = await _fileSystemService.ReadAllBytesAsync(entry.Path);
    }

    [RelayCommand]
    private void DeleteEntry(HistoryEntryBase entry)
    {
        History.Remove(entry);
        _historyRepository.DeleteHistoryEntry(entry);
    }

    #endregion

    #region Inference

    [RelayCommand]
    private async Task SelectImage()
    {
        var path = (await DialogService.OpenFileDialogAsync(false, new List<FilePickerFileType>
        {
            new("Images")
            {
                Patterns = new List<string>
                    { "*.bmp", "*.gif", "*.jpeg", "*.jpg", "*.pbm", "*.png", "*.tiff", "*.tga", "*.webp" },
                AppleUniformTypeIdentifiers = new List<string>
                    { "public.bmp", "public.gif", "public.jpeg", "public.png", "public.tiff" },
                MimeTypes = new List<string>
                    { "image/bmp", "image/gif", "image/jpeg", "image/png", "image/tiff", "image/tga", "image/webp" }
            },
        })).FirstOrDefault();

        if (!string.IsNullOrEmpty(path))
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SourceImage = await _fileSystemService.ReadAllBytesAsync(path);
            });
    }

    [RelayCommand]
    private void ClearImage()
    {
        SourceImage = null;
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (SourceImage is not { Length: not 0 })
            return;

        ResultImage = [];
        Progress = 0;
        State = GenerationState.Started;
        var history = new HistoryEntryBase { Type = "Image", Extension = ".png" };

        await Task.Run(async () =>
                ResultImage = await Service.Upscale(UpscaleFactor, SourceImage))
            .ConfigureAwait(false);

        if (ResultImage.Length != 0)
        {
            var entry = await _historyRepository.AddHistoryEntry(history, ResultImage);
            History.Add(entry);
        }

        State = GenerationState.Finished;
        Progress = 0;
    }

    private void OnProgressChanged(object? sender, StableDiffusionProgressEventArgs e)
    {
        Progress = e.Progress * 100;
    }

    #endregion

    #region Models

    [RelayCommand]
    private async Task RefreshModels() =>
        Models = new ObservableCollection<ModelInfo>(await _localRepository.GetAllModels(new Dictionary<string, string>
            { { "Type", "Image" }, { "Task", "Upscale" }, { "WeightExtension", ".gguf,.ckpt,.safetensors" } }));

    [RelayCommand]
    private async Task SwitchModel() =>
        await _modelCoreService.Switch(SelectedModel, SelectedWeight);

    [RelayCommand]
    private async Task UnloadModel()
    {
        if (_modelCoreService is ImageModelCoreService modelCoreService)
            await modelCoreService.Unload("Upscale");
        else
            await _modelCoreService.Unload();
    }

    #endregion
}