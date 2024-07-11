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
    public CancellationTokenSource? CancelToken { get; set; }

    public async Task Switch(ModelInfo? model, ModelInfoWeight? weight)
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

            if (!NativeLibraryConfig.LibraryHasLoaded)
                NativeLibraryConfig.Instance.WithCuda(AppConfig.Instance.Device == Device.GPU).WithAutoFallback();

            var modelPath =
                $@"{AppConfig.Instance.ModelsPath}\{SelectedGenerationModel.Type.RemoveSpaces()}\{SelectedGenerationModel.Name.RemoveSpaces()}\{SelectedGenerationWeight.Weight.RemoveSpaces()}.{SelectedGenerationWeight.Extension.ToLower().RemoveSpaces()}";
            ModelParameters = new ModelParams(modelPath)
            {
                ContextSize = 4096,
                Embeddings = Mode == GenerationMode.Embeddings,
                GpuLayerCount = AppConfig.Instance.Device == Device.GPU
                    ? Extensions.TextModelLayersCount(SelectedGenerationModel.Parameters,
                        SelectedGenerationWeight.Size.BytesToGB().ToString(CultureInfo.InvariantCulture),
                        SystemInfoService.Instance.Info.VRAMFree)
                    : 0
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
            await OnUnload.Invoke([this])!;
        GC.Collect();
    }

    public event IModelCoreService.SwitchHandler? OnSwitch;
    public event IModelCoreService.UnloadHandler? OnUnload;
}