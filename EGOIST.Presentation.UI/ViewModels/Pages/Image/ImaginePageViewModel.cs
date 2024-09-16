using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using Microsoft.Extensions.DependencyInjection;
using StableDiffusion.NET;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Image;

public partial class ImaginePageViewModel(
    [FromKeyedServices("ImagineService")] IImageService service,
    IHistoryRepository historyRepository,
    IFileSystemService fileSystemService) : ViewModelBase, INavigationAware
{
    public override string Title => "Imagine";

    [ObservableProperty] private GenerationState _state = GenerationState.None;


    #region ImagesVariables

    [ObservableProperty] private byte[] _resultImage = [];

    [ObservableProperty] private ObservableCollection<ImageHistoryEntry> _history = [];

    #endregion

    #region GenerationVariables

    [ObservableProperty] private ImageGenerationParameters _generationParameters = new();
    [ObservableProperty] private double _progress;

    private IImageService Service { get; } = service;

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
        foreach (var entry in await historyRepository.GetAllHistory(new Dictionary<string, string>
                     { { "Type", "Image" } }))
        {
            if (entry is ImageHistoryEntry historyEntry)
                History.Add(historyEntry);
        }
    }

    #endregion


    #region History

    [RelayCommand]
    private async Task SelectEntry(ImageHistoryEntry entry)
    {
        GenerationParameters = entry.Parameters;
        ResultImage = await fileSystemService.ReadAllBytesAsync(entry.Path);
    }

    [RelayCommand]
    private void DeleteEntry(ImageHistoryEntry entry)
    {
        History.Remove(entry);
        historyRepository.DeleteHistoryEntry(entry);
    }

    #endregion

    #region Inference

    [RelayCommand]
    private async Task Generate()
    {
        ResultImage = [];
        Progress = 0;
        State = GenerationState.Started;
        GenerationParameters.Seed =
            GenerationParameters.Seed == -1 ? new Random().NextInt64() : GenerationParameters.Seed;
        var history = new ImageHistoryEntry { Parameters = GenerationParameters, Type = "Image", Extension = ".png" };

        await Task.Run(async () =>
                ResultImage = await Service.Imagine(GenerationParameters))
            .ConfigureAwait(false);

        if (ResultImage.Length != 0)
        {
            var entry = await historyRepository.AddHistoryEntry(history, ResultImage);
            History.Add((ImageHistoryEntry)entry);
        }

        State = GenerationState.Finished;
        Progress = 0;
    }

    private void OnProgressChanged(object? sender, StableDiffusionProgressEventArgs e)
    {
        Progress = e.Progress * 100;
    }

    #endregion
}