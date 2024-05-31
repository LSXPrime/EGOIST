using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Interfaces;

public interface IModelsRepository
{
    Task<IEnumerable<ModelInfo>> GetAllModels(Dictionary<string, string>? parameters = null);
    Task<IEnumerable<ModelInfo>> GetAllModels(string query = "", int modelsCount = 10);
    Task<ModelInfo?> GetModel(string repoId);
}
