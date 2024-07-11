using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using NetFabric.Hyperlinq;
using Microsoft.Extensions.Logging;

namespace EGOIST.Infrastructure.Repositories;

public class LocalCharacterRepository(ILogger<LocalCharacterRepository> logger) : ICharacterRepository
{
    private readonly ConcurrentDictionary<string, IEnumerable<RoleplayCharacter>> _charactersCache = new();

    public Task<IEnumerable<RoleplayCharacter>> GetAllCharacters(Dictionary<string, string>? parameters = null)
    {
        var cacheKey = "all_characters" + (parameters != null ? JsonSerializer.Serialize(parameters) : "");
        if (_charactersCache.TryGetValue(cacheKey, out var cachedCharacters))
        {
            logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return Task.FromResult(cachedCharacters);
        }

        var charactersPath = Path.Combine(AppConfig.Instance.CharactersPath, parameters?["Type"] ?? string.Empty);
        if (!Directory.Exists(charactersPath))
        {
            logger.LogWarning("Characters path {charactersPath} does not exist.", charactersPath);
            return Task.FromResult<IEnumerable<RoleplayCharacter>>([]);
        }

        var characters = Directory
            .EnumerateFiles(charactersPath, "*.json", SearchOption.AllDirectories)
            .AsValueEnumerable()
            .Select(GetRoleplayCharacter)
            .Where(character =>
            {
                if (character == null) return false;
                if (parameters == null) return true;

                var matchesType = !parameters.ContainsKey("Name") || character.Name == parameters["Name"];
                var matchesCategory = !parameters.ContainsKey("Name") || character.Name.Contains(parameters["Name"]);

                return matchesType && matchesCategory;
            })
            .ToArray();
        
        _charactersCache[cacheKey] = characters!;
        return Task.FromResult(characters.AsEnumerable())!;
    }

    public Task<IEnumerable<RoleplayCharacter>> GetAllCharacters(string query = "", int count = 10)
    {
        var cacheKey = $"search_{query}_{count}";
        if (_charactersCache.TryGetValue(cacheKey, out var cachedCharacters))
        {
            logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return Task.FromResult(cachedCharacters);
        }

        var charactersPath = AppConfig.Instance.CharactersPath;
        if (!Directory.Exists(charactersPath))
        {
            logger.LogWarning("Characters path {charactersPath} does not exist.", charactersPath);
            return Task.FromResult<IEnumerable<RoleplayCharacter>>([]);
        }

        var characters = Directory
            .EnumerateFiles(charactersPath, "*.json", SearchOption.TopDirectoryOnly)
            .AsValueEnumerable()
            .Select(GetRoleplayCharacter)
            .Where(character =>
                character != null &&
                (string.IsNullOrEmpty(query) ||
                 character.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 character.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            )
            .Take(count)
            .ToArray();

        _charactersCache[cacheKey] = characters!;
        return Task.FromResult(characters.AsEnumerable())!;
    }

    public Task<RoleplayCharacter?> GetCharacter(string name)
    {
        var charactersPath = AppConfig.Instance.CharactersPath;
        if (!Directory.Exists(charactersPath))
        {
            logger.LogWarning("characters path {charactersPath} does not exist.", charactersPath);
            return Task.FromResult<RoleplayCharacter?>(null);
        }

        var characterTemplate = Directory
            .EnumerateFiles(charactersPath, "*.json", SearchOption.TopDirectoryOnly)
            .AsValueEnumerable()
            .Select(GetRoleplayCharacter)
            .FirstOrDefault(character =>
                character != null && character.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(characterTemplate);
    }

    private RoleplayCharacter? GetRoleplayCharacter(string configPath)
    {
        if (!File.Exists(configPath))
        {
            logger.LogWarning("Character file not found in {configPath}", configPath);
            return null;
        }

        var content = File.ReadAllText(configPath);
        var template = JsonSerializer.Deserialize<RoleplayCharacter>(content);
        if (template == null)
        {
            logger.LogWarning("Failed to deserialize character from {ConfigPath}", configPath);
            return null;
        }

        return template;
    }
}