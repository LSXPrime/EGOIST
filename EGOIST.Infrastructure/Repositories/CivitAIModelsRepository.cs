using System.Net.Http.Json;
using EGOIST.Application.DTOs.Management.Models.CivitAI;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Infrastructure.Repositories;

public class CivitAIModelsRepository : IModelsRepository
{
    private readonly HttpClient _httpClient = new();

    public async Task<IEnumerable<ModelInfo>> GetAllModels(string query, int modelsCount = 10)
    {
        var response = await _httpClient.GetAsync($"https://civitai.com/api/v1/models?query={query}&limit={modelsCount}\"");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CivitAIModelDTO>>();
        return result.ToModelsInfo();
    }

    public Task<IEnumerable<ModelInfo>> GetAllModels(Dictionary<string, string>? parameters) => throw new NotImplementedException();

    public async Task<ModelInfo?> GetModel(string modelVersionId)
    {
        var response = await _httpClient.GetAsync($"https://civitai.com/api/v1/model-versions/{modelVersionId}");
        response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<ModelVersion>();
        return model.ToModelInfo();
    }
}
