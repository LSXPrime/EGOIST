using System.Collections.ObjectModel;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Application.Services.Text.Roleplay;

public class WorldMemoryService(IWorldMemoryRepository repository)
{
    public async Task<bool> CreateWorldAsync(string worldName)
    {
        if (await GetWorldAsync(worldName) != null)
        {
            return false;
        }

        var newWorld = new RoleplayWorld { Name = worldName };
        return await repository.SaveWorldAsync(newWorld);
    }

    public async Task<RoleplayWorld?> GetWorldAsync(string worldName)
    {
        return await repository.GetWorldAsync(worldName);
    }

    public async Task<IEnumerable<RoleplayWorld>> GetAllWorldsAsync()
    {
        return await repository.GetAllWorldsAsync();
    }

    public async Task<bool> DeleteWorldAsync(string worldName)
    {
        return await repository.DeleteWorldAsync(worldName);
    }
    
    public async Task<bool> StoreMemoryAsync(string worldName, string memoryName, string text,
                                                string author = "Unknown", string? tags = null, bool overwrite = false)
        {
            var world = await GetWorldAsync(worldName);
            if (world == null) return false;

            if (!overwrite && world.Memories.Any(m => 
                              m.Name.Equals(memoryName, StringComparison.OrdinalIgnoreCase)))
            {
                return false; 
            }

            var existingMemory = world.Memories.FirstOrDefault(m => 
                                   m.Name.Equals(memoryName, StringComparison.OrdinalIgnoreCase));
            
            if (existingMemory != null) 
            {
                existingMemory.Content = text;
                existingMemory.Author = author;
                existingMemory.Tags = string.IsNullOrEmpty(tags)
                                            ? []
                                            : new ObservableCollection<string>(tags.Split(',').Select(t => t.Trim()));
            } 
            else 
            {
                // If it's a new memory, create and add it to the list
                var newMemory = new RoleplayWorldMemory
                {
                    Name = memoryName,
                    Content = text,
                    Author = author,
                    CreationDate = DateTime.UtcNow,
                    Tags = string.IsNullOrEmpty(tags)
                                ? []
                                : new ObservableCollection<string>(tags.Split(',').Select(t => t.Trim()))
                };

                world.Memories.Add(newMemory);
            }

            return await repository.SaveWorldAsync(world);
        }

        public async Task<RoleplayWorldMemory> GetMemoryAsync(string worldName, string memoryName)
        {
            var world = await GetWorldAsync(worldName);
            return world?.Memories.FirstOrDefault(m =>
                m.Name.Equals(memoryName, StringComparison.OrdinalIgnoreCase))!;
        }

        public async Task<IEnumerable<RoleplayWorldMemory>> GetAllMemoriesInWorldAsync(string worldName)
        {
            var world = await GetWorldAsync(worldName);
            return world?.Memories ?? [];
        }

        public async Task<bool> DeleteMemoryAsync(string worldName, string memoryName)
        {
            var world = await GetWorldAsync(worldName);
            if (world == null) return false;

            var memoryToRemove = world.Memories.FirstOrDefault(m =>
                                    m.Name.Equals(memoryName, StringComparison.OrdinalIgnoreCase));

            if (memoryToRemove != null)
            {
                world.Memories.Remove(memoryToRemove);
                return await repository.SaveWorldAsync(world);
            }

            return false; 
        }

        public async Task<IEnumerable<RoleplayWorldMemory>> SearchMemoriesInWorldAsync(string worldName, string searchTerm)
        {
            var world = await GetWorldAsync(worldName);
            if (world == null) return [];

            return world.Memories.Where(m =>
                m.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.Tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }
}