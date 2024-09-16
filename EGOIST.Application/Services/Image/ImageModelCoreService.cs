using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image;

public class ImageModelCoreService(ILogger<ImageModelCoreService> logger, IFileSystemService fileSystemService)
    : IModelCoreService
{
    public GenerationState State { get; set; } = GenerationState.None;
    public GenerationMode Mode { get; set; } = GenerationMode.Image;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }

    public DiffusionModel? Model;
    public UpscaleModel? UpscaleModel;
    public CancellationTokenSource? CancelToken { get; set; }


    public async Task Switch(ModelInfo? model, ModelInfoWeight? weight, Dictionary<string, object?>? parameters = null)
    {
        if (string.Equals(model?.Task, "Generation", StringComparison.OrdinalIgnoreCase))
        {
            SelectedGenerationModel = model;
            SelectedGenerationWeight = weight;
        }

        try
        {
            if (model == null || weight == null)
            {
                logger.LogWarning("No selected generation model.");
                return;
            }

            var (modelPath, controlNetPath, loraPath, vaePath, taesdPath) =
                GetWeightPaths(parameters, model, weight);
            if (string.IsNullOrEmpty(modelPath) || !fileSystemService.FileExists(modelPath))
            {
                logger.LogWarning("Model path not found.");
                return;
            }

            State = GenerationState.Started;
            
            await Task.Run(() =>
            {
                if (model.Task.Equals("Upscale", StringComparison.OrdinalIgnoreCase))
                    UpscaleModel = new UpscaleModel(new UpscaleModelParameter
                        { ModelPath = modelPath, ThreadCount = Environment.ProcessorCount });
                else
                    Model = ModelBuilder.StableDiffusion(modelPath)
                        .KeepVaeOnCpu()
                        .WithVaeDecodeOnly()
                        .KeepClipNetOnCpu()
                        .KeepControlNetOnCpu()
                        .WithVae(vaePath)
                        .WithControlNet(controlNetPath)
                        .WithLoraSupport(loraPath)
                        .WithTaesd(taesdPath)
                        .WithMultithreading(Environment.ProcessorCount)
                        .Build();

                State = GenerationState.Finished;

                return Task.CompletedTask;
            });

            if (OnSwitch != null)
                await OnSwitch.Invoke([this]);

        }
        catch (Exception ex)
        {
            SelectedGenerationModel = null;
            SelectedGenerationWeight = null;
            Model = null;
            UpscaleModel = null;
            State = GenerationState.None;
            logger.LogError(ex, "An error occurred while switching generation model.");
        }
    }

    public async Task Unload() => await Unload(string.Empty);
    public async Task Unload(string type)
    {
        switch (type.ToLowerInvariant())
        {
            case "generation":
                Model?.Dispose();
                Model = null;
                SelectedGenerationModel = null;
                break;
            case "upscale":
                UpscaleModel?.Dispose();
                UpscaleModel = null;
                break;
            default:
                Model?.Dispose();
                UpscaleModel?.Dispose();
                Model = null;
                UpscaleModel = null;
                SelectedGenerationModel = null;
                break;
        }
        State = GenerationState.None;
        if (OnUnload != null)
            await OnUnload.Invoke([this]);
        GC.Collect();
    }

    private static (string, string, string, string, string) GetWeightPaths(Dictionary<string, object?>? parameters,
        ModelInfo model,
        ModelInfoWeight weight)
    {
        var modelPath = GetPath(model.Name.RemoveSpaces(), weight);
        var controlNetPath = parameters?.TryGetValue("ControlNet", out var obj) == true && obj is ModelInfoWeight controlNetWeight
                ? GetPath("ControlNet", controlNetWeight) : string.Empty;
        var loraPath = GetPath("Lora", null);
        var vaePath = parameters?.TryGetValue("Vae", out obj) == true && obj is ModelInfoWeight vaeWeight ? GetPath("Vae", vaeWeight) : string.Empty;
        var taesdPath = parameters?.TryGetValue("Taesd", out obj) == true && obj is ModelInfoWeight taesdWeight ? GetPath("Taesd", taesdWeight) : string.Empty;

        return (modelPath, controlNetPath, loraPath, vaePath, taesdPath);

        string GetPath(string subfolder, ModelInfoWeight? modelWeight) =>
            $@"{AppConfig.Instance.Parameters.ModelsPath}\{model.Type.RemoveSpaces()}\{subfolder}\{modelWeight?.Weight.RemoveSpaces()}.{modelWeight?.Extension.ToLower().RemoveSpaces()}";
    }

    public event IModelCoreService.SwitchHandler? OnSwitch;
    public event IModelCoreService.UnloadHandler? OnUnload;
}