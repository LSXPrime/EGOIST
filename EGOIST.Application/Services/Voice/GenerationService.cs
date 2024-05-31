using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using Whisper.net;

namespace EGOIST.Application.Services.Voice;

public class GenerationService
{
    public GenerationState State { get; set; } = GenerationState.None;
    public GenerationMode Mode { get; set; } = GenerationMode.Audio;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }


    private WhisperFactory? TranscribeModel { get; set; }
    public WhisperProcessor? TranscribeProcessor { get; set; }
    public CancellationTokenSource? CancelToken;

    private readonly ILogger<GenerationService> _logger;

    public GenerationService()
    {
        _logger = new LoggerFactory().CreateLogger<GenerationService>();
    }

    public void Switch(ModelInfo model, ModelInfoWeight weight)
    {
        SelectedGenerationModel = model;
        SelectedGenerationWeight = weight;

        _logger.LogInformation($"Model Switching to {SelectedGenerationModel.Name} Started");
        if (SelectedGenerationModel.Task == "generation")
        {
            _logger.LogError($"Model Switching to {SelectedGenerationModel.Name} Failed, Error: Voice generation removed temporarily");
        }
        else if (SelectedGenerationModel.Task == "transcribe")
        {
            new Thread(() =>
            {
                var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight.Weight.RemoveSpaces()}.{SelectedGenerationWeight.Extension.ToLower().RemoveSpaces()}";

                var isGpu = AppConfig.Instance.Device == Device.GPU;
                TranscribeModel = WhisperFactory.FromPath(modelPath, false, null, false, isGpu);
                TranscribeProcessor = TranscribeModel.CreateBuilder().WithLanguage("auto").Build();
                _logger.LogInformation("Audio Transcribe Model loaded Successfully");
            }).Start();
        }
    }

    public void Unload()
    {
        TranscribeModel?.Dispose();
        TranscribeProcessor?.Dispose();
        SelectedGenerationModel = null;
        State = GenerationState.None;
        GC.Collect();
        _logger.LogInformation("Audio Transcribe Model Unloaded");
    }

    private static GenerationService? instance;
    public static GenerationService Instance => instance ??= new GenerationService();

}
