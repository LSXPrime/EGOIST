using System.Globalization;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using LLama;
using LLama.Common;
using LLama.Native;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class TextModelCoreService(ILogger<TextModelCoreService> logger) : EntityBase, IModelCoreService
{
    private GenerationState _state = GenerationState.None;

    public GenerationState State
    {
        get => _state;
        set => Notify(ref _state, value);
    }

    public GenerationMode Mode { get; set; } = GenerationMode.Text;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }

    public LLamaWeights? Model;
    public ModelParams? ModelParameters;
    public CancellationTokenSource? CancelToken { get; set; } = new();

    public async Task Switch(ModelInfo? model, ModelInfoWeight? weight, Dictionary<string, object?>? parameters = null)
    {
        if (model == null || weight == null)
        {
            await Unload();
            return;
        }

        SelectedGenerationModel = model;
        SelectedGenerationWeight = weight;

        try
        {
            if (SelectedGenerationModel == null)
            {
                logger.LogWarning("No selected generation model.");
                return;
            }

            State = GenerationState.Started;

            var modelPath =
                $@"{AppConfig.Instance.Parameters.ModelsPath}\{SelectedGenerationModel.Type.RemoveSpaces()}\{SelectedGenerationModel.Name.RemoveSpaces()}\{SelectedGenerationWeight.Weight.RemoveSpaces()}.{SelectedGenerationWeight.Extension.ToLower().RemoveSpaces()}";
            TextModelParameters? modelParameters = null;
            
            if (parameters != null && parameters.TryGetValue("ModelParameters", out var modelParametersObject) && modelParametersObject is TextModelParameters textModelParameters)
                modelParameters = textModelParameters;
            
            ModelParameters = new ModelParams(modelPath)
            {
                ContextSize = modelParameters is { ContextLengthAuto: true } ? null : (modelParameters is { ContextLength: > 0 } ? (uint?)modelParameters.ContextLength : 4096),
                FlashAttention = modelParameters is { FlashAttention: true},
                UseMemoryLock = modelParameters is { MemoryLock: true},
                Threads = (uint?)(modelParameters is { CpuThreadsAuto: true } ? Environment.ProcessorCount
                     : modelParameters is { CpuThreads: > 0 } ? modelParameters.CpuThreads : 0),
                GpuLayerCount = modelParameters is { GpuSharedLayersAuto: true } ? (AppConfig.Instance.Parameters.Device != Device.Cpu
                    ? Extensions.TextModelLayersCount(SelectedGenerationModel, SelectedGenerationWeight,
                        SystemInfoService.Instance.Info.VRAMFree)
                    : 0)
                    : modelParameters is { GpuSharedLayers: > 0 } ? modelParameters.GpuSharedLayers : 0
            };
            
            Model = await LLamaWeights.LoadFromFileAsync(ModelParameters);

            State = GenerationState.Finished;
            if (OnSwitch != null)
                await OnSwitch.Invoke([this]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while switching generation model.");
            await Unload();
        }
    }

    public async Task Unload()
    {
        Model?.Dispose();
        Model = null;
        SelectedGenerationModel = null;
        State = GenerationState.None;
        if (OnUnload != null)
            await OnUnload.Invoke([this]);
        GC.Collect();
    }

    public event IModelCoreService.SwitchHandler? OnSwitch;
    public event IModelCoreService.UnloadHandler? OnUnload;
}