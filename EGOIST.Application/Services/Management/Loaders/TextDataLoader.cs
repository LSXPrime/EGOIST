using System.Text.Json;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Text;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using LLama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChatSession = EGOIST.Domain.Entities.ChatSession;

namespace EGOIST.Application.Services.Management.Loaders;

public class TextDataLoader(
    ILogger<TextDataLoader> logger,
    IFileSystemService fileSystemService,
    [FromKeyedServices("TextModelCoreService")]
    IModelCoreService modelCoreService)
{
    private readonly string _sessionsPath = Path.Combine(AppConfig.Instance.Parameters.ResultsPath, "Text");
    private readonly string _cachePath = Path.Combine(AppConfig.Instance.Parameters.CachePath, "Text");
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public async Task SaveAllSessions<T>(IEnumerable<T> sessions, string type) where T : ISession
    {
        foreach (var session in sessions)
        {
            await SaveSession(session, type);
        }
    }

    public async Task SaveSession<T>(T baseSession, string type, bool saveCache = true) where T : ISession
    {
        var sessionPath = Path.Combine(_sessionsPath, type, baseSession.Name);
        Directory.CreateDirectory(sessionPath);

        var content = JsonSerializer.Serialize(baseSession, _jsonOptions);
        await fileSystemService.WriteAllTextAsync(Path.Combine(sessionPath, $"{baseSession.Name}.json"), content);

        if (saveCache)
            await SaveSessionCache(baseSession);

        logger.LogInformation("Session {baseSessionName} saved successfully", baseSession.Name);
    }

    public async Task<T?> LoadSession<T>(T baseSession) where T : ISession
    {
        await LoadSessionCache(baseSession);
        baseSession.IsLoaded = true;

        return baseSession;
    }

    public async Task<T?> LoadSession<T>(string sessionName, string type, bool loadCache = true) where T : ISession
    {
        var sessionPath = Path.Combine(_sessionsPath, type, sessionName, $"{sessionName}.json");
        if (!fileSystemService.FileExists(sessionPath))
        {
            logger.LogWarning("Session file not found: {sessionPath}", sessionPath);
            return default;
        }

        var content = await fileSystemService.ReadAllTextAsync(sessionPath);
        if (!content.TryDeserialize<T>(out var session))
        {
            logger.LogError("Failed to deserialize session from {sessionPath}", sessionPath);
            return default;
        }

        if (loadCache)
        {
            await LoadSessionCache(session);
            session.IsLoaded = true;
        }

        return session;
    }

    public async Task<IEnumerable<T>> LoadAllSessions<T>(string type) where T : ISession
    {
        var sessions = new List<T>();

        var sessionPath = Path.Combine(_sessionsPath, type);
        if (!fileSystemService.DirectoryExists(sessionPath))
        {
            fileSystemService.CreateDirectory(sessionPath);
            return [];
        }

        var sessionDirectories = Directory.GetDirectories(sessionPath);

        foreach (var sessionDirectory in sessionDirectories)
        {
            var sessionName = Path.GetFileName(sessionDirectory);
            var session = await LoadSession<T>(sessionName, type,false);
            if (session != null) sessions.Add(session);
        }

        return sessions;
    }

    public Task DeleteSession(string sessionName)
    {
        var sessionPath = Path.Combine(_sessionsPath, sessionName);
        if (fileSystemService.DirectoryExists(sessionPath))
            fileSystemService.DeleteDirectory(sessionPath, true);

        var cachePath = Path.Combine(_cachePath, sessionName);
        if (fileSystemService.DirectoryExists(cachePath))
            fileSystemService.DeleteDirectory(cachePath, true);

        logger.LogInformation($"Session {sessionName} deleted successfully");
        return Task.CompletedTask;
    }

    private async Task SaveSessionCache(ISession session)
    {
        if (!fileSystemService.DirectoryExists(_cachePath))
            fileSystemService.CreateDirectory(_cachePath);

        switch (session)
        {
            case ChatSession { Executor: InferenceService { Executor: StatefulExecutorBase chatExecutor } }:
                await SaveExecutorCache(chatExecutor, session.Name);
                break;
            case RoleplaySession roleplaySession:
                if (roleplaySession.PersonalityApproach != RpCharacterInferenceApproach.PerCharacterExecutor &&
                    roleplaySession.Executor is InferenceService
                    {
                        Executor: StatefulExecutorBase roleplayExecutor
                    })
                {
                    await SaveExecutorCache(roleplayExecutor, session.Name);
                }
                else
                {
                    foreach (var character in roleplaySession.CharacterInferences)
                    {
                        if (character.Value is InferenceService
                            {
                                Executor: StatefulExecutorBase characterExecutor
                            })
                        {
                            var cachePath = Path.Combine(_cachePath, roleplaySession.Name, "Characters",
                                character.Key.Name);
                            characterExecutor.Context.SaveState($"{cachePath}_model.bin");
                            await characterExecutor.SaveState($"{cachePath}_session.bin");
                        }
                    }
                }

                break;
        }
    }

    private async Task LoadSessionCache(ISession session)
    {
        if (modelCoreService is not TextModelCoreService model || model.SelectedGenerationModel == null ||
            model.Model == null)
            return;

        switch (session)
        {
            case ChatSession chatSession:
                var chatExecutor = new InteractiveExecutor(new LLamaContext(model.Model, model.ModelParameters!));
                chatSession.Executor = new InferenceService(chatExecutor);
                await LoadExecutorCache(chatExecutor, session.Name);
                break;
            case RoleplaySession roleplaySession:
                if (roleplaySession.PersonalityApproach != RpCharacterInferenceApproach.PerCharacterExecutor)
                {
                    var roleplayExecutor =
                        new InteractiveExecutor(new LLamaContext(model.Model, model.ModelParameters!));
                    roleplaySession.Executor = new InferenceService(roleplayExecutor);
                    await LoadExecutorCache(roleplayExecutor, session.Name);
                }
                else
                {
                    foreach (var character in roleplaySession.Characters)
                    {
                        var characterExecutor =
                            new InteractiveExecutor(new LLamaContext(model.Model, model.ModelParameters!));
                        roleplaySession.CharacterInferences[character] = new InferenceService(characterExecutor);

                        var cachePath = Path.Combine(_cachePath, roleplaySession.Name, "Characters", character.Name);
                        characterExecutor.Context.LoadState($"{cachePath}_model.bin");
                        await characterExecutor.LoadState($"{cachePath}_session.bin");
                    }
                }

                break;
        }
    }

    private async Task SaveExecutorCache(StatefulExecutorBase executor, string sessionName)
    {
        var cachePath = Path.Combine(_cachePath, sessionName);
        fileSystemService.CreateDirectory(cachePath);
        await executor.SaveState($@"{cachePath}\session.bin");
        executor.Context.SaveState($@"{cachePath}\model.bin");
    }

    private async Task LoadExecutorCache(StatefulExecutorBase executor, string sessionName)
    {
        var cachePath = Path.Combine(_cachePath, sessionName);
        if (!fileSystemService.DirectoryExists(cachePath))
            return;

        await executor.LoadState($@"{cachePath}\session.bin");
        executor.Context.LoadState($@"{cachePath}\model.bin");
    }
}