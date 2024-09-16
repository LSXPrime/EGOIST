using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using NetFabric.Hyperlinq;

namespace EGOIST.Infrastructure.Repositories;

public class LocalModelsRepository : IModelsRepository
{
    public Task<IEnumerable<ModelInfo>> GetAllModels(Dictionary<string, string>? parameters = null)
    {
        var modelsPath = Path.Combine(AppConfig.Instance.Parameters.ModelsPath , parameters?["Type"] ?? string.Empty);
        var models = Directory
            .EnumerateDirectories(modelsPath, "*", SearchOption.AllDirectories)
            .AsValueEnumerable()
            .Select(directoryPath =>
            {
                var model = GetOrCreateModelInfo(directoryPath, weightExtensions: parameters != null && parameters.TryGetValue("WeightExtensions", out var weightExtensions) ? weightExtensions.Split(",") : null);
                return model;
            })
            .Where(model =>
            {
                if (parameters == null) return true;

                var matchesType = !parameters.ContainsKey("Type") ||
                                  string.Equals(model.Type, parameters["Type"], StringComparison.OrdinalIgnoreCase);
                var matchesTask = !parameters.ContainsKey("Task") ||
                                  string.Equals(model.Task, parameters["Task"], StringComparison.OrdinalIgnoreCase);

                return matchesType && matchesTask;
            }).ToArray();

        return Task.FromResult(models.AsEnumerable());
    }

    public Task<IEnumerable<ModelInfo>> GetAllModels(string query = "", int modelsCount = 10, string[]? weightExtensions = null)
    {
        var modelsPath = AppConfig.Instance.Parameters.ModelsPath;
        var models = Directory
            .EnumerateDirectories(modelsPath, "*", SearchOption.AllDirectories)
            .AsValueEnumerable()
            .Select(folder =>
            {
                var model = GetOrCreateModelInfo(folder);
                model.Weights = GetOrCreateWeights(folder, weightExtensions);
                return model;
            })
            .Where(model =>
                string.IsNullOrEmpty(query) ||
                model.Name.Contains(query) ||
                model.Type.Contains(query) ||
                model.Task.Contains(query)
            )
            .Take(modelsCount).ToArray();

        return Task.FromResult(models.AsEnumerable());
    }


    public Task<ModelInfo?> GetModel(string repoId)
    {
        return Task.FromResult(GetAllModels(repoId, 1).Result.FirstOrDefault());
    }
    
    private static ModelInfo GetOrCreateModelInfo(string modelPath, string configFile = "egoist_config.json", string[]? weightExtensions = null)
    {
        var configPath = Path.Combine(modelPath, configFile);
        if (File.Exists(configPath))
            return JsonSerializer.Deserialize<ModelInfo>(File.ReadAllText(configPath)) ?? new ModelInfo();

        var directoryParts = configPath.Split(Path.DirectorySeparatorChar);
        var model = new ModelInfo
        {
            Type = directoryParts[^3],
            Task = "Generation",
            Name = directoryParts[^2].Replace("_", " "),
            Weights = GetOrCreateWeights(modelPath, weightExtensions)
        };
        
        File.WriteAllText(configPath, JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }));
        return model;
    }

    private static ObservableCollection<ModelInfoWeight> GetOrCreateWeights(string directoryPath, string[]? weightExtensions)
    {
        var weights = new ObservableCollection<ModelInfoWeight>();
        var weightFiles = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => weightExtensions?.Any(file.EndsWith) ?? new[] { ".gguf", ".safetensors", ".ckpt", ".pth" }.Any(file.EndsWith));
        foreach (var weightFile in weightFiles)
        {
            var weight = new ModelInfoWeight
            {
                Extension = Path.GetExtension(weightFile)[1..],
                Weight = Path.GetFileNameWithoutExtension(weightFile),
                Link = weightFile,
                Size = new FileInfo(weightFile).Length
            };
            weights.Add(weight);
        }
        return weights;
    }
}