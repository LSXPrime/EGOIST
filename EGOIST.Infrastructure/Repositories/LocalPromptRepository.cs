using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Interfaces;
using NetFabric.Hyperlinq;
using Microsoft.Extensions.Logging;

namespace EGOIST.Infrastructure.Repositories;

public class LocalPromptRepository<TPromptTemplate>(ILogger<LocalPromptRepository<TPromptTemplate>> logger) : IPromptRepository<TPromptTemplate> where TPromptTemplate : IPromptTemplate
{
    private readonly ConcurrentDictionary<string, IEnumerable<TPromptTemplate>> _promptsCache = new();

    public Task<IEnumerable<TPromptTemplate>> GetAllTemplates(Dictionary<string, string>? parameters = null)
    {
        var cacheKey = "all_prompts" + (parameters != null ? JsonSerializer.Serialize(parameters) : "");
        if (_promptsCache.TryGetValue(cacheKey, out var cachedPrompts))
        {
            logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return Task.FromResult(cachedPrompts);
        }

        try
        {
            var promptsPath = Path.Combine(AppConfig.Instance.PromptsPath, parameters?["Type"] ?? string.Empty);
            if (!Directory.Exists(promptsPath))
            {
                logger.LogWarning("Prompts path {PromptsPath} does not exist.", promptsPath);
                return Task.FromResult(Enumerable.Empty<TPromptTemplate>());
            }

            var prompts = Directory
                .EnumerateFiles(promptsPath, "*.json", SearchOption.TopDirectoryOnly)
                .Select(GetPromptTemplate)
                .Where(prompt =>
                {
                    if (prompt == null) return false;
                    if (parameters == null) return true;

                    var matchesType = !parameters.ContainsKey("Type") || prompt.Type == parameters["Type"];
                    var matchesCategory = !parameters.ContainsKey("Category") || prompt.Category == parameters["Category"];

                    return matchesType && matchesCategory;
                })
                .ToList();

            _promptsCache[cacheKey] = prompts!;
            return Task.FromResult<IEnumerable<TPromptTemplate>>(prompts!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all templates.");
            throw;
        }
    }

    public Task<IEnumerable<TPromptTemplate>> GetAllTemplates(string query = "", int templatesCount = 10)
    {
        var cacheKey = $"search_{query}_{templatesCount}";
        if (_promptsCache.TryGetValue(cacheKey, out var cachedPrompts))
        {
            logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return Task.FromResult(cachedPrompts);
        }

        try
        {
            var promptsPath = AppConfig.Instance.PromptsPath;
            if (!Directory.Exists(promptsPath))
            {
                logger.LogWarning("Prompts path {PromptsPath} does not exist.", promptsPath);
                return Task.FromResult(Enumerable.Empty<TPromptTemplate>());
            }

            var prompts = Directory
                .EnumerateFiles(promptsPath, "*.json", SearchOption.TopDirectoryOnly)
                .Select(GetPromptTemplate)
                .Where(prompt =>
                    prompt != null &&
                    (string.IsNullOrEmpty(query) ||
                     prompt.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     prompt.Type.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     prompt.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
                )
                .Take(templatesCount)
                .ToList();

            _promptsCache[cacheKey] = prompts!;
            return Task.FromResult<IEnumerable<TPromptTemplate>>(prompts!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while searching for templates.");
            throw;
        }
    }

    public Task<TPromptTemplate?> GetTemplate(string template)
    {
        try
        {
            var promptsPath = AppConfig.Instance.PromptsPath;
            if (!Directory.Exists(promptsPath))
            {
                logger.LogWarning("Prompts path {PromptsPath} does not exist.", promptsPath);
            //    return Task.FromResult<TPromptTemplate?>(null);
            }

            var promptTemplate = Directory
                .EnumerateFiles(promptsPath, "*.json", SearchOption.TopDirectoryOnly)
                .AsValueEnumerable()
                .Select(GetPromptTemplate)
                .FirstOrDefault(prompt => prompt != null && prompt.Name.Equals(template, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(promptTemplate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting template {Template}", template);
            throw;
        }
    }

    private TPromptTemplate? GetPromptTemplate(string configPath)
    {
        if (!File.Exists(configPath))
        {
            logger.LogWarning("Template file not found in {configPath}", configPath);
       //     return null;
        }

        try
        {
            var content = File.ReadAllText(configPath);
            var template = JsonSerializer.Deserialize<TPromptTemplate>(content);
            if (template == null)
            {
                logger.LogWarning("Failed to deserialize template from {ConfigPath}", configPath);
                //     return null;
            }

            return template;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while reading or deserializing {ConfigPath}", configPath);
            throw;
        }
    }
}