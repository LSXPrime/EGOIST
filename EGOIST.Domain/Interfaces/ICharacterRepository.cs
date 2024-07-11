using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Interfaces;

public interface ICharacterRepository
{
    Task<IEnumerable<RoleplayCharacter>> GetAllCharacters(Dictionary<string, string>? parameters = null);
    Task<IEnumerable<RoleplayCharacter>> GetAllCharacters(string query = "", int modelsCount = 10);
    Task<RoleplayCharacter?> GetCharacter(string name);
}