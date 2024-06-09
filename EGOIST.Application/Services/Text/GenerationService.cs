using System.Diagnostics;
using System.Globalization;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using LLama;
using LLama.Common;
using LLama.Native;
using LLamaSharp.KernelMemory;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.KernelMemory.ContentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;

namespace EGOIST.Application.Services.Text;

public class GenerationService
{
    public GenerationState State { get; set; } = GenerationState.None;
    public GenerationMode Mode { get; set; } = GenerationMode.Text;
    public ModelInfo? SelectedGenerationModel { get; set; }
    public ModelInfoWeight? SelectedGenerationWeight { get; set; }

    public LLamaWeights? Model;
    public LLamaEmbedder? Embedder;
    public ModelParams? ModelParameters;
    public IKernelMemory? Memory;
    public CancellationTokenSource? CancelToken;

    private readonly ILogger<GenerationService> _logger = new LoggerFactory().CreateLogger<GenerationService>();

    public async Task Switch(ModelInfo? model, ModelInfoWeight? weight)
    {
        if (model == null || weight == null)
        {
            Unload();
            return;
        }

        SelectedGenerationModel = model;
        SelectedGenerationWeight = weight;

        try
        {
            if (SelectedGenerationModel == null)
            {
                _logger.LogWarning("No selected generation model.");
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
                        SystemInfoService.Instance?.systemInfo?.VRAMFree ?? 0)
                    : 0
            };
            Model = await LLamaWeights.LoadFromFileAsync(ModelParameters);

            Debug.WriteLine($"Model: {SelectedGenerationModel.Name} Loading Finished");
            Debug.WriteLine($"Weight: {SelectedGenerationWeight.Weight} Loading Finished");
            /*
            if (Mode == GenerationMode.Embeddings)
            {
                Embedder = new LLamaEmbedder(Model, ModelParameters);

                Memory = new KernelMemoryBuilder()
                    .WithLLamaSharpTextEmbeddingGeneration(new LLamaSharpTextEmbeddingGenerator(Embedder))
                    .WithLLamaSharpTextGeneration(new LlamaSharpTextGenerator(Model, Embedder.Context))
                    .WithSearchClientConfig(new SearchClientConfig { MaxMatchesCount = 1, AnswerTokens = 100 })
                    .With(new TextPartitioningOptions { MaxTokensPerParagraph = 300, MaxTokensPerLine = 100, OverlappingTokens = 30 })
                    .WithSimpleFileStorage(new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk })
                    .WithSimpleVectorDb(new SimpleVectorDbConfig { StorageType = FileSystemTypes.Disk })
                    .Build();
            }
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while switching generation model.");
            Debug.WriteLine($"Model Loading Error: {ex.Message}");
            Unload();
        }
    }

    public void Unload()
    {
        Model?.Dispose();
        Model = null;
        Memory = null;
        SelectedGenerationModel = null;
        State = GenerationState.None;
        GC.Collect();
    }

    private static GenerationService? _instance;
    public static GenerationService Instance => _instance ??= new GenerationService();
}