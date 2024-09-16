using System.Collections.ObjectModel;
using System.Text.Json;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Management;

public class CharacterService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly ILogger<CharacterService> _logger;
    private readonly ICharacterRepository _characterRepository;

    public CharacterService(IFileSystemService fileSystemService,
        IImageMetadataService imageMetadataService,
        ILogger<CharacterService> logger,
        ICharacterRepository characterRepository)
    {
        _fileSystemService = fileSystemService;
        _imageMetadataService = imageMetadataService;
        _logger = logger;
        _characterRepository = characterRepository;

        _ = RefreshCharactersAsync();
    }

    public ObservableCollection<RoleplayCharacter> Characters { get; set; } = [];


    /// <summary>
    /// Creates a new character with the provided details.
    /// </summary>
    /// <param name="character">The character object to create.</param>
    /// <param name="avatar">The avatar image data.</param>
    /// <returns>A task that completes when the character is created.</returns>
    public async Task CreateCharacterAsync(RoleplayCharacter character, byte[] avatar)
    {
        try
        {
            var characterPath = Path.Combine(AppConfig.Instance.Parameters.CharactersPath, character.Name);

            // Create the character directory
            _fileSystemService.CreateDirectory(characterPath);

            // Handle the character avatar
            if (!_fileSystemService.FileExists(character.Avatar))
            {
                character.Avatar = $"{character.Name}.webp";
            }
            else
            {
                var avatarPath = Path.Combine(characterPath, $"{character.Name}{Path.GetExtension(character.Avatar)}");
                await _fileSystemService.WriteAllBytesAsync(avatarPath, avatar);
                character.Avatar = Path.GetFileName(avatarPath);
            }

            // Serialize and save the character data to JSON
            var characterJson = JsonSerializer.Serialize(character);
            var characterJsonPath = Path.Combine(characterPath, $"{character.Name}.json");
            await _fileSystemService.WriteAllTextAsync(characterJsonPath, characterJson);

            _logger.LogInformation("Character {CharacterName} created successfully", character.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create character {CharacterName}", character.Name);
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
            var characterPath = Path.Combine(AppConfig.Instance.Parameters.CharactersPath, character.Name);

            // Delete the character directory if it exists
            if (_fileSystemService.DirectoryExists(characterPath))
            {
                _fileSystemService.DeleteDirectory(characterPath, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete character {CharacterName}", character.Name);
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
            charParsed = await _fileSystemService.ReadAllTextAsync(charPath);
            var character = JsonSerializer.Deserialize<RoleplayCharacter>(charParsed);
            rpCharacter = character!;
        }
        else if (new[] { ".webp", ".png" }.Contains(Path.GetExtension(charPath)))
        {
            charParsed = charFormat switch
            {
                ".webp" => await _imageMetadataService.ExtractCharacterData(charPath),
                ".png" => await _imageMetadataService.ExtractCharacterData(charPath),
                _ => throw new NotSupportedException(
                    $"Image format '{charFormat}' not supported for metadata extraction.")
            };

            rpCharacter = JsonSerializer.Deserialize<RoleplayCharacter>(charParsed)!;
        }

        if (rpCharacter != null)
        {
            // Serialize and save the character data to JSON
            var characterJson = JsonSerializer.Serialize(rpCharacter);
            var characterPath = Path.Combine(AppConfig.Instance.Parameters.CharactersPath, rpCharacter.Name);
            var characterJsonPath = Path.Combine(characterPath, $"{rpCharacter.Name}.json");
            await _fileSystemService.WriteAllTextAsync(characterJsonPath, characterJson);

            _logger.LogInformation("Character {CharacterName} imported successfully", rpCharacter.Name);
        }

        return rpCharacter;
    }

    public async Task RefreshCharactersAsync()
    {
        Characters.Clear();
        Characters.AddRange(await _characterRepository.GetAllCharacters(null));
    }
}