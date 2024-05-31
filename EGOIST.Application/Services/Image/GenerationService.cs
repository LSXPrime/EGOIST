using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image;

public class GenerationService
{
    public GenerationState State { get; set; } = GenerationState.None;
    public GenerationMode Mode { get; set; } = GenerationMode.Image;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }

    public StableDiffusionModel? Model;
    public ModelParameter? ModelParameters;
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

        new Thread(async () =>
        {
            try
            {
                if (SelectedGenerationModel == null)
                {
                    _logger.LogWarning("No selected generation model.");
                    return;
                }

                var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight.Weight.RemoveSpaces()}.{SelectedGenerationWeight.Extension.ToLower().RemoveSpaces()}";
                ModelParameters = new ModelParameter
                {
                    ThreadCount = Environment.ProcessorCount,
                    Quantization = (Quantization)Enum.Parse(typeof(Quantization), weight.Weight)
                };
                Model = await Task.Run(() => new StableDiffusionModel(modelPath, ModelParameters));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while switching generation model.");
            }
        }).Start();
    }

    public void Unload()
    {
        Model?.Dispose();
        Model = null;
        SelectedGenerationModel = null;
        State = GenerationState.None;
        GC.Collect();
    }

    private static GenerationService? instance;
    public static GenerationService Instance => instance ??= new GenerationService();

}
