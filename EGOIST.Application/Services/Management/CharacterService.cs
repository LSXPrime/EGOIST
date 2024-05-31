using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EGOIST.Application.Services.Management
{
    public class CharacterService
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IImageMetadataService _imageMetadataService;
        private readonly ILogger<CharacterService> _logger;

        public CharacterService(IFileSystemService fileSystemService, IImageMetadataService imageMetadataService, ILogger<CharacterService> logger)
        {
            _fileSystemService = fileSystemService;
            _imageMetadataService = imageMetadataService;
            _logger = logger;
        }

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
                _fileSystemService.CreateDirectory(characterPath);

                // Handle the character avatar
                if (!_fileSystemService.FileExists(character.Avatar))
                {
                    character.Avatar = $"{character.Name}.webp";
                }
                else
                {
                    var avatarPath = Path.Combine(characterPath, $"{character.Name}{Path.GetExtension(character.Avatar)}");
                    _fileSystemService.CopyFile(character.Avatar, avatarPath, true);
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
                var characterPath = Path.Combine(AppConfig.Instance.CharactersPath, character.Name);

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
        /// <param name="filePath">The path to the character file.</param>
        /// <returns>A task that completes with the imported character object.</returns>
        public async Task<RoleplayCharacter> ImportCharacterAsync(string filePath)
        {
            try
            {
                // Read the character data from the file
                var characterJson = await _fileSystemService.ReadAllTextAsync(filePath);
                var character = JsonSerializer.Deserialize<RoleplayCharacter>(characterJson);

                // Validate the imported character (optional)

                // Return the imported character
                return character;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import character from {FilePath}", filePath);
                throw;
            }
        }
    }
}