namespace EGOIST.Infrastructure.Repositories;

using System.Net.Http.Json;
using EGOIST.Application.DTOs.Management.Models.HuggingFace;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

public class HuggingFaceModelsRepository : IModelsRepository
{
    private readonly HttpClient _httpClient = new();

    public async Task<IEnumerable<ModelInfo>> GetAllModels(string query, int modelsCount = 10)
    {
        var response = await _httpClient.GetAsync($"https://huggingface.co/api/models?search={query}&filter=gguf&sort=downloads&limit={modelsCount}\"");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<HuggingFaceModelDTO>>();
        return result.Select(x => x.ToModelInfo());
    }

    public Task<IEnumerable<ModelInfo>> GetAllModels(Dictionary<string, string>? parameters) => throw new NotImplementedException();

    public async Task<ModelInfo?> GetModel(string repoId)
    {
        var response = await _httpClient.GetAsync($"https://huggingface.co/api/models/{repoId}");
        response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<HuggingFaceModelDTO>();
        return model.ToModelInfo();
    }
}
