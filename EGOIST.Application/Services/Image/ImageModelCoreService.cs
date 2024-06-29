using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image;

public class ImageModelCoreService(ILogger<ImageModelCoreService> logger) : IModelCoreService
{
    public GenerationState State { get; set; } = GenerationState.None;
    public GenerationMode Mode { get; set; } = GenerationMode.Image;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }

    public StableDiffusionModel? Model;
    public ModelParameter? ModelParameters;
    public CancellationTokenSource? CancelToken { get; set; }


    public async Task Switch(ModelInfo? model, ModelInfoWeight? weight)
    {
        SelectedGenerationModel = model;
        SelectedGenerationWeight = weight;


        try
        {
            if (SelectedGenerationModel == null)
            {
                logger.LogWarning("No selected generation model.");
                return;
            }

            var modelPath =
                $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight?.Weight.RemoveSpaces()}.{SelectedGenerationWeight?.Extension.ToLower().RemoveSpaces()}";
            ModelParameters = new ModelParameter
            {
                ThreadCount = Environment.ProcessorCount,
                Quantization = (Quantization)Enum.Parse(typeof(Quantization), weight?.Weight!)
            };
            Model =  new StableDiffusionModel(modelPath, ModelParameters);
            await OnSwitch?.Invoke([this])!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while switching generation model.");
        }
    }

    public async Task Unload()
    {
        Model?.Dispose();
        Model = null;
        SelectedGenerationModel = null;
        State = GenerationState.None;
        await OnUnload?.Invoke([this])!;
        GC.Collect();
    }

    public event IModelCoreService.SwitchHandler? OnSwitch;
    public event IModelCoreService.UnloadHandler? OnUnload;
}