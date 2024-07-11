using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using NetFabric.Hyperlinq;

namespace EGOIST.Infrastructure.Repositories;

public class LocalModelsRepository : IModelsRepository
{
    private readonly ConcurrentDictionary<string, IEnumerable<ModelInfo>> _modelsCache = new();

    public Task<IEnumerable<ModelInfo>> GetAllModels(Dictionary<string, string>? parameters = null)
    {
        var cacheKey = "all_models" + (parameters != null ? JsonSerializer.Serialize(parameters) : "");
        if (_modelsCache.TryGetValue(cacheKey, out var cachedModels))
            return Task.FromResult(cachedModels);

        var modelsPath = Path.Combine(AppConfig.Instance.ModelsPath , parameters?["Type"] ?? string.Empty);
        var models = Directory
            .EnumerateDirectories(modelsPath, "*", SearchOption.AllDirectories)
            .AsValueEnumerable()
            .Select(directoryPath =>
            {
                var model = GetOrCreateModelInfo(directoryPath);
                return model;
            })
            .Where(model =>
            {
                if (parameters == null) return true;

                var matchesType = !parameters.ContainsKey("Type") || model.Type == parameters["Type"];
                var matchesTask = !parameters.ContainsKey("Task") || model.Task == parameters["Task"];

                return matchesType && matchesTask;
            }).ToArray();

    //    models = WithExistWeightsOnly(models);
        _modelsCache[cacheKey] = models;
        return Task.FromResult(models.AsEnumerable());
    }

    public Task<IEnumerable<ModelInfo>> GetAllModels(string query = "", int modelsCount = 10)
    {
        var cacheKey = $"search_{query}_{modelsCount}";
        if (_modelsCache.TryGetValue(cacheKey, out var cachedModels))
            return Task.FromResult(cachedModels);

        var modelsPath = AppConfig.Instance.ModelsPath;
        var models = Directory
            .EnumerateDirectories(modelsPath, "*", SearchOption.AllDirectories)
            .AsValueEnumerable()
            .Select(folder =>
            {
                var model = GetOrCreateModelInfo(folder);
                model.Weights = GetOrCreateWeights(folder);
                return model;
            })
            .Where(model =>
                string.IsNullOrEmpty(query) ||
                model.Name.Contains(query) ||
                model.Type.Contains(query) ||
                model.Task.Contains(query)
            )
            .Take(modelsCount).ToArray();

   //     models = WithExistWeightsOnly(models);
        _modelsCache[cacheKey] = models;
        return Task.FromResult(models.AsEnumerable());
    }


    public Task<ModelInfo?> GetModel(string repoId)
    {
        return Task.FromResult(GetAllModels(repoId, 1).Result.FirstOrDefault());
    }

    private static IEnumerable<ModelInfo> WithExistWeightsOnly(IEnumerable<ModelInfo> models)
    {
        return models
            .AsValueEnumerable()
            .Select(x =>
            {
                if (x.Weights != null)
                    x.Weights = new ObservableCollection<ModelInfoWeight>(x.Weights.Where(w =>
                    {
                        var weightPath = $"{AppConfig.Instance.ModelsPath}\\" +
                                         $"{x.Type.RemoveSpaces()}\\" +
                                         $"{x.Name.RemoveSpaces()}\\" +
                                         $"{w.Weight.RemoveSpaces()}.{w.Extension.ToLower().RemoveSpaces()}";

                        return File.Exists(weightPath);
                    }));
                return x;
            });
    }

    private static ModelInfo GetOrCreateModelInfo(string modelPath, string configFile = "egoist_config.json")
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
            Weights = GetOrCreateWeights(modelPath)
        };
        
        File.WriteAllText(configPath, JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }));
        return model;
    }

    private static ObservableCollection<ModelInfoWeight> GetOrCreateWeights(string directoryPath)
    {
        var weights = new ObservableCollection<ModelInfoWeight>();
        var weightFiles = Directory.EnumerateFiles(directoryPath, "*.gguf", SearchOption.TopDirectoryOnly);
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