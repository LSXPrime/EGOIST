using System.Collections.ObjectModel;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EGOIST.Application.Services.Management;

public class CharacterService(
    IFileSystemService fileSystemService,
    IImageMetadataService imageMetadataService,
    ILogger<CharacterService> logger,
    ICharacterRepository characterRepository)
{
    public ObservableCollection<RoleplayCharacter> Characters => new(characterRepository.GetAllCharacters(null).Result);


    /// <summary>
    /// Creates a new character with the provided details.
    /// </summary>
    /// <param name="character">The character object to create.</param>
    /// <returns>A task that completes when the character is created.</returns>
    public async Task CreateCharacterAsync(RoleplayCharacter character)
    {
        try
        {
            var characterPath = Path.Combine(AppConfig.Instance.CharactersPath, character.Name);

            // Create the character directory
            fileSystemService.CreateDirectory(characterPath);

            // Handle the character avatar
            if (!fileSystemService.FileExists(character.Avatar))
            {
                character.Avatar = $"{character.Name}.webp";
            }
            else
            {
                var avatarPath = Path.Combine(characterPath, $"{character.Name}{Path.GetExtension(character.Avatar)}");
                fileSystemService.CopyFile(character.Avatar, avatarPath, true);
                character.Avatar = Path.GetFileName(avatarPath);
            }

            // Serialize and save the character data to JSON
            var characterJson = JsonSerializer.Serialize(character);
            var characterJsonPath = Path.Combine(characterPath, $"{character.Name}.json");
            await fileSystemService.WriteAllTextAsync(characterJsonPath, characterJson);

            logger.LogInformation("Character {CharacterName} created successfully", character.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create character {CharacterName}", character.Name);
            throw;
        }
    }

    /// <summary>
    /// Deletes a character and its associated files.
    /// </summary>
    /// <param name="character">The character object to delete.</param>
    public void DeleteCharacter(RoleplayCharacter character)
    {
        try
        {
            var characterPath = Path.Combine(AppConfig.Instance.CharactersPath, character.Name);

            // Delete the character directory if it exists
            if (fileSystemService.DirectoryExists(characterPath))
            {
                fileSystemService.DeleteDirectory(characterPath, true);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete character {CharacterName}", character.Name);
            throw;
        }
    }

    /// <summary>
    /// Imports a character from a specified file.
    /// </summary>
    /// <param name="charPath">The path to the character file.</param>
    /// <returns>A task that completes with the imported character object.</returns>
    public async Task<RoleplayCharacter?> ImportCharacterAsync(string charPath)
    {
        var charFormat = Path.GetExtension(charPath);
        string charParsed;
        RoleplayCharacter? rpCharacter = null;
        if (new[] { ".json", ".txt", ".log" }.Contains(Path.GetExtension(charPath)))
        {
            charParsed = await fileSystemService.ReadAllTextAsync(charPath);
            var character = JsonSerializer.Deserialize<RoleplayCharacter>(charParsed);
            rpCharacter = character!;
        }
        else if (new[] { ".webp", ".png" }.Contains(Path.GetExtension(charPath)))
        {
            charParsed = charFormat switch
            {
                ".webp" => await imageMetadataService.ExtractCharacterData(charPath),
                ".png" => await imageMetadataService.ExtractCharacterData(charPath),
                _ => throw new NotSupportedException(
                    $"Image format '{charFormat}' not supported for metadata extraction.")
            };

            rpCharacter = JsonSerializer.Deserialize<RoleplayCharacter>(charParsed)!;
        }

        if (rpCharacter != null)
        {
            // Serialize and save the character data to JSON
            var characterJson = JsonSerializer.Serialize(rpCharacter);
            var characterPath = Path.Combine(AppConfig.Instance.CharactersPath, rpCharacter.Name);
            var characterJsonPath = Path.Combine(characterPath, $"{rpCharacter.Name}.json");
            await fileSystemService.WriteAllTextAsync(characterJsonPath, characterJson);

            logger.LogInformation("Character {CharacterName} imported successfully", rpCharacter.Name);
        }
        
        return rpCharacter;
    }
}