using System.Diagnostics;
using System.Globalization;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using LLama;
using LLama.Common;
using LLama.Native;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class TextModelCoreService(ILogger<TextModelCoreService> logger) : IModelCoreService
{
    public GenerationState State { get; set; } = GenerationState.None;
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
            
            Debug.WriteLine($"Model: {SelectedGenerationModel.Name} Loading Started");
            Debug.WriteLine($"Weight: {SelectedGenerationWeight.Weight} Loading Started");
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
                        SystemInfoService.Instance.systemInfo.VRAMFree)
                    : 0
            };
            Model = await LLamaWeights.LoadFromFileAsync(ModelParameters);

            await OnSwitch?.Invoke([this])!;

            Debug.WriteLine($"Model: {SelectedGenerationModel.Name} Loading Finished");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while switching generation model.");
            Debug.WriteLine($"Model Loading Error: {ex.Message}");
            await Unload();
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