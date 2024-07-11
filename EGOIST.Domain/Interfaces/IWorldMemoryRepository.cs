using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Interfaces;

public interface IWorldMemoryRepository
{
    Task<bool> SaveWorldAsync(RoleplayWorld roleplayWorld);
    Task<RoleplayWorld?> GetWorldAsync(string worldName);
    Task<IEnumerable<RoleplayWorld>> GetAllWorldsAsync();
    Task<bool> DeleteWorldAsync(string worldName);
}