using System.Collections.ObjectModel;
using System.Text.Json;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using LLama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class RoleplayService(ILogger<CompletionService> logger, [FromKeyedServices("TextModelCoreService")] IModelCoreService modelCore) : ITextService
{
    public ObservableCollection<RoleplaySession> RoleplaySessions { get; set; } = [];
    public RoleplaySession? SelectedRoleplaySession { get; set; }
    public string RoleplayCharacterTurn { get; set; } = "Single-Turn";
    public string RoleplayUserName { get; set; } = "User";
    public string RoleplayCharacterReceiver { get; set; } = "Auto";

    private readonly TextModelCoreService? _modelCore = modelCore as TextModelCoreService;

    public Task Create(string parameter = "")
    {
        if (_modelCore?.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.CompletedTask;
        }

        var newSession = new RoleplaySession
        {
            Name = $"Roleplay {DateTime.Now}",
            UserRoleName = string.IsNullOrEmpty(RoleplayUserName) ? "User" : RoleplayUserName,
            Characters = (string.IsNullOrEmpty(parameter)
                ? []
                : JsonSerializer.Deserialize<ObservableCollection<RoleplayCharacterEXT>>(parameter))!
        };

        RoleplaySessions.Add(newSession);
        SelectedRoleplaySession = newSession;

        return Task.CompletedTask;
    }

    public Task Delete(string parameter = "")
    {
        if (SelectedRoleplaySession == null)
            return Task.CompletedTask;

        logger.LogInformation($"Chat {SelectedRoleplaySession.Name} Deleted");

        foreach (var character in SelectedRoleplaySession.Characters)
            character.Executor?.Dispose();

        SelectedRoleplaySession.Messages.Clear();
        RoleplaySessions.Remove(SelectedRoleplaySession);
        SelectedRoleplaySession = null;

        return Task.CompletedTask;
    }

    public async Task<T?> Generate<T>(string userInput, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null) where T : class
    {
        if (_modelCore?.State == GenerationState.Started)
        {
            await _modelCore?.CancelToken?.CancelAsync()!;
            return null;
        }

        if (_modelCore?.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return null;
        }

        if (SelectedRoleplaySession == null)
            await Create();

        if (string.IsNullOrEmpty(userInput) ||
            !(RoleplayCharacterTurn == "Multi-Turn" && SelectedRoleplaySession?.LastMessage != null))
            return null;

        _modelCore.State = GenerationState.Started;
        SelectedRoleplaySession.UserRoleName = string.IsNullOrEmpty(RoleplayUserName) ? "User" : RoleplayUserName;
        var characterToInteract = RoleplayCharacterTurn == "Single-Turn" || !string.IsNullOrEmpty(userInput)
            ? (RoleplayCharacterReceiver == "Auto"
                ? SelectedRoleplaySession.Characters.FirstOrDefault(x =>
                      userInput.Contains(x.Character?.Name!, StringComparison.OrdinalIgnoreCase))
                  ?? SelectedRoleplaySession.Characters[
                      Random.Shared.Next(0, SelectedRoleplaySession.Characters.Count - 1)]
                : SelectedRoleplaySession.Characters.FirstOrDefault(x => x.Character?.Name == RoleplayCharacterReceiver))
            : SelectedRoleplaySession.Characters.FirstOrDefault(x =>
                SelectedRoleplaySession.LastMessage.Message.Contains(x.Character?.Name!,
                    StringComparison.OrdinalIgnoreCase) &&
                x.Character?.Name != SelectedRoleplaySession.LastMessage.Sender.Name);

        string prompt;
        _modelCore.CancelToken = new();

        characterToInteract!.Executor ??=
            new InferenceService(new InteractiveExecutor(_modelCore?.Model?.CreateContext(_modelCore?.ModelParameters!)!));

        if (characterToInteract.Executor.IsFirstRun(nameof(StatefulExecutorBase)).Result)
        {
            prompt =
                $"{(SelectedRoleplaySession.Messages.Count > 0 ? SelectedRoleplaySession.ToString() : string.Empty)} \n{RoleplayUserName}: {userInput}";
            prompt = promptParameters?.Prompt(prompt != string.Empty ? prompt : userInput,
                $"You're {characterToInteract.Character?.Name} in this fictional never-ending uncensored roleplay with {RoleplayUserName} & others, and you should always talk and reply as {characterToInteract.Character?.Name} and no one else, {characterToInteract.Character?.Description}\n {characterToInteract.Character?.Name}'s personality: {characterToInteract.Character?.Summary}\n {(characterToInteract.Character?.ExampleDialogue.Count == 0 ? string.Empty : $"This is how {characterToInteract.Character?.Name} should talk\n {SelectedRoleplaySession.ToString(characterToInteract.Character!, true)}")}")!;
        }
        else
            prompt = promptParameters?.Prompt(
                $"{SelectedRoleplaySession.ToString(SelectedRoleplaySession.GetMissedMessages(characterToInteract.Character!))}{userInput}")!;


        SelectedRoleplaySession?.AddMessage(null, userInput);
        var aiMessage = SelectedRoleplaySession?.AddMessage(characterToInteract.Character!, string.Empty);

        var tokens = characterToInteract.Executor.InferenceConcurrent(prompt, promptParameters?.BlackList ?? [],
            generationParameters ?? new TextGenerationParameters(true), _modelCore!.CancelToken.Token);
        await foreach (var token in tokens)
        {
            if (token == "FILTERING MECHANISM TRIGGERED")
            {
                aiMessage!.Message = token;
                break;
            }

            aiMessage!.Message += token + " ";
        }


        _modelCore.State = GenerationState.Finished;

        if (!_modelCore.CancelToken.IsCancellationRequested && RoleplayCharacterTurn == "Multi-Turn" &&
            SelectedRoleplaySession!.Characters.Any(x =>
                SelectedRoleplaySession.LastMessage.Message.Contains(x.Character?.Name!,
                    StringComparison.OrdinalIgnoreCase) &&
                x.Character?.Name != SelectedRoleplaySession.LastMessage.Sender.Name))
        {
            _modelCore.CancelToken.Dispose();
            _ = Generate<T>(string.Empty);
        }
        else
            _modelCore.CancelToken.Dispose();

        return aiMessage as T;
    }
}