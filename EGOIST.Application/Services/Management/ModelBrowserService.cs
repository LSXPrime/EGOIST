using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EGOIST.Application.Services.Management;

public class ModelBrowserService([FromKeyedServices("HuggingFaceModelsRepository")] IModelsRepository huggingFaceRepo, [FromKeyedServices("CivitAIModelsRepository")] IModelsRepository civitAIRepo, [FromKeyedServices("LocalModelsRepository")] IModelsRepository localRepo)
{
    private readonly IModelsRepository _huggingFaceRepo = huggingFaceRepo;
    private readonly IModelsRepository _civitAIRepo = civitAIRepo;
    private readonly IModelsRepository _localRepo = localRepo;

    public async Task<IEnumerable<ModelInfo>> GetModelsAsync(ModelsProvider provider)
    {
        switch (provider)
        {
            case ModelsProvider.Egoist:
                break;
            case ModelsProvider.HuggingFace:
                return await _huggingFaceRepo.GetAllModels(modelsCount: 10);
            case ModelsProvider.CivitAI:
                return await _civitAIRepo.GetAllModels(modelsCount: 10);
            case ModelsProvider.Local:
                return await _localRepo.GetAllModels(null);
            default:
                break;
        }

        return [];
    }

}
