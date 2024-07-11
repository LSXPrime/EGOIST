using System.Text.Json;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Infrastructure.Repositories;

public class LocalWorldMemoryRepository(IFileSystemService fileSystemService) : IWorldMemoryRepository
{
    private readonly string _baseDirectory = AppConfig.Instance.WorldMemoriesPath;

    public async Task<bool> SaveWorldAsync(RoleplayWorld roleplayWorld)
    {
        var filePath = GetWorldFilePath(roleplayWorld.Name);
        var jsonData = JsonSerializer.Serialize(roleplayWorld);
        await fileSystemService.WriteAllTextAsync(filePath, jsonData);
        return true;
    }

    public async Task<RoleplayWorld?> GetWorldAsync(string worldName)
    {
        var filePath = GetWorldFilePath(worldName);
        if (!File.Exists(filePath))
            return null;

        var jsonData = await fileSystemService.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<RoleplayWorld>(jsonData);
    }

    public async Task<IEnumerable<RoleplayWorld>> GetAllWorldsAsync()
    {
        var worlds = new List<RoleplayWorld>();
        if (!fileSystemService.DirectoryExists(_baseDirectory))
            return worlds;

        foreach (var filePath in Directory.EnumerateFiles(_baseDirectory, "*.json"))
        {
            var worldData = JsonSerializer.Deserialize<RoleplayWorld>(await fileSystemService.ReadAllTextAsync(filePath));
            if (worldData != null)
                worlds.Add(worldData);
        }

        return worlds;
    }

    public Task<bool> DeleteWorldAsync(string worldName)
    {
        var filePath = GetWorldFilePath(worldName);
        if (!fileSystemService.FileExists(filePath))
            return Task.FromResult(false);
        fileSystemService.DeleteFile(filePath);
        return Task.FromResult(true);
    }

    private string GetWorldFilePath(string worldName)
    {
        if (!fileSystemService.DirectoryExists(_baseDirectory))
            fileSystemService.CreateDirectory(_baseDirectory);

        return Path.Combine(_baseDirectory, $"{worldName.ToLower()}.json");
    }
}