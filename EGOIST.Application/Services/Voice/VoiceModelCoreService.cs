using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using Whisper.net;

namespace EGOIST.Application.Services.Voice;

public class VoiceModelCoreService(ILogger<VoiceModelCoreService> logger)  : IModelCoreService
{
    public GenerationState State { get; set; } = GenerationState.None;
    public GenerationMode Mode { get; set; } = GenerationMode.Audio;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }


    private WhisperFactory? TranscribeModel { get; set; }
    public WhisperProcessor? TranscribeProcessor { get; set; }
    public CancellationTokenSource? CancelToken { get; set; }


    public async Task Switch(ModelInfo? model, ModelInfoWeight? weight)
    {
        SelectedGenerationModel = model;
        SelectedGenerationWeight = weight;

        logger.LogInformation($"Model Switching to {SelectedGenerationModel?.Name} Started");
        if (SelectedGenerationModel?.Task == "generation")
        {
            logger.LogError(
                $"Model Switching to {SelectedGenerationModel.Name} Failed, Error: Voice generation removed temporarily");
        }
        else if (SelectedGenerationModel?.Task == "transcribe")
        {
            var modelPath =
                $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight?.Weight.RemoveSpaces()}.{SelectedGenerationWeight?.Extension.ToLower().RemoveSpaces()}";

            var isGpu = AppConfig.Instance.Device == Device.GPU;
            TranscribeModel = WhisperFactory.FromPath(modelPath, false, null, false, isGpu);
            TranscribeProcessor = TranscribeModel.CreateBuilder().WithLanguage("auto").Build();
            logger.LogInformation("Audio Transcribe Model loaded Successfully");
            await OnSwitch?.Invoke([this])!;
        }
    }

    public async Task Unload()
    {
        TranscribeModel?.Dispose();
        if (TranscribeProcessor != null) 
            await TranscribeProcessor.DisposeAsync();
        SelectedGenerationModel = null;
        State = GenerationState.None;
        await OnUnload?.Invoke([this])!;
        GC.Collect();
        logger.LogInformation("Audio Transcribe Model Unloaded");
    }

    public event IModelCoreService.SwitchHandler? OnSwitch;
    public event IModelCoreService.UnloadHandler? OnUnload;
}