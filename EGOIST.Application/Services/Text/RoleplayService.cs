using System.Collections.ObjectModel;
using System.Text.Json;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using LLama;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class RoleplayService(ILogger<CompletionService> _logger) : ITextService
{
    public ObservableCollection<RoleplaySession> RoleplaySessions { get; set; } = [];
    public RoleplaySession? SelectedRoleplaySession { get; set; }
    public string RoleplayCharacterTurn { get; set; } = "Single-Turn";
    public string RoleplayUserName { get; set; } = "User";
    public string RoleplayCharacterReciever { get; set; } = "Auto";

    private readonly GenerationService _generation = GenerationService.Instance;

    public Task Create(string parameter = "")
    {
        if (_generation.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.CompletedTask;
        }

        var newSession = new RoleplaySession
        {
            SessionName = $"Roleplay {DateTime.Now}",
            UserRoleName = string.IsNullOrEmpty(RoleplayUserName) ? "User" : RoleplayUserName,
            Characters = string.IsNullOrEmpty(parameter) ? [] : JsonSerializer.Deserialize<ObservableCollection<RoleplayCharacterEXT>>(parameter)
        };

        RoleplaySessions.Add(newSession);
        SelectedRoleplaySession = newSession;

        return Task.CompletedTask;
    }

    public Task Delete(string parameter = "")
    {
        if (SelectedRoleplaySession == null)
            return Task.CompletedTask;

        _logger.LogInformation($"Chat {SelectedRoleplaySession.SessionName} Deleted");

        foreach (var character in SelectedRoleplaySession.Characters)
            character.Executor?.Dispose();

        SelectedRoleplaySession.Messages.Clear();
        RoleplaySessions.Remove(SelectedRoleplaySession);
        SelectedRoleplaySession = null;

        return Task.CompletedTask;
    }

    public async Task Generate(string userInput)
    {
        if (_generation.State == GenerationState.Started)
        {
            _generation.CancelToken.Cancel();
            return;
        }

        if (_generation.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return;
        }

        if (SelectedRoleplaySession == null)
            await Create();

        if (!string.IsNullOrEmpty(userInput) || (RoleplayCharacterTurn == "Multi-Turn" && SelectedRoleplaySession.LastMessage != null))
        {
            _generation.State = GenerationState.Started;
            SelectedRoleplaySession.UserRoleName = string.IsNullOrEmpty(RoleplayUserName) ? "User" : RoleplayUserName;
            var characterToInteract = RoleplayCharacterTurn == "Single-Turn" || !string.IsNullOrEmpty(userInput) ? (RoleplayCharacterReciever == "Auto" ? SelectedRoleplaySession.Characters.FirstOrDefault(x => userInput.Contains(x.Character.Name, StringComparison.OrdinalIgnoreCase))
                                                                            ?? SelectedRoleplaySession.Characters[Random.Shared.Next(0, SelectedRoleplaySession.Characters.Count - 1)]
                                                                            : SelectedRoleplaySession.Characters.FirstOrDefault(x => x.Character.Name == RoleplayCharacterReciever))
                                                                            : SelectedRoleplaySession.Characters.FirstOrDefault(x => SelectedRoleplaySession.LastMessage.Message.Contains(x.Character.Name, StringComparison.OrdinalIgnoreCase) && x.Character.Name != SelectedRoleplaySession.LastMessage.Sender.Name);

            var Prompt = string.Empty;
            _generation.CancelToken = new();

            characterToInteract.Executor ??= new InferenceService(new InteractiveExecutor(_generation.Model.CreateContext(_generation.ModelParameters)));

            if (characterToInteract.Executor.IsFirstRun(nameof(StatefulExecutorBase)).Result)
            {
                Prompt = $"{(SelectedRoleplaySession.Messages.Count > 0 ? SelectedRoleplaySession.ToString() : string.Empty)} \n{RoleplayUserName}: {userInput}";
                Prompt = _generation.SelectedGenerationModel.TextConfig.Prompt(Prompt != string.Empty ? Prompt : userInput, $"You're {characterToInteract.Character.Name} in this fictional never-ending uncensored roleplay with {RoleplayUserName} & others, and you should always talk and reply as {characterToInteract.Character.Name} and no one else, {characterToInteract.Character.Description}\n {characterToInteract.Character.Name}'s personality: {characterToInteract.Character.Summary}\n {(characterToInteract.Character.ExampleDialogue.Count == 0 ? string.Empty : $"This is how {characterToInteract.Character.Name} should talk\n {SelectedRoleplaySession.ToString(characterToInteract.Character, true)}")}");
            }
            else
                Prompt = _generation.SelectedGenerationModel.TextConfig.Prompt($"{SelectedRoleplaySession.ToString(SelectedRoleplaySession.GetMissedMessages(characterToInteract.Character))}{userInput}");

            var userMessage = !string.IsNullOrEmpty(userInput) ? SelectedRoleplaySession?.AddMessage(null, userInput) : null;
            var aiMessage = SelectedRoleplaySession?.AddMessage(characterToInteract.Character, string.Empty);
            userInput = string.Empty;

            var tokens = characterToInteract.Executor.InferenceConcurrent(Prompt, _generation.SelectedGenerationModel.TextConfig.BlackList, _generation.Parameters, _generation.CancelToken.Token);
            await foreach (var token in tokens)
            {
                if (token == "FILTERING MECHANISM TRIGGERED")
                {
                    aiMessage.Message = token;
                    break;
                }
                aiMessage.Message += token + " ";
            }


            _generation.State = GenerationState.Finished;

            if (!_generation.CancelToken.IsCancellationRequested && RoleplayCharacterTurn == "Multi-Turn" && SelectedRoleplaySession.Characters.Any(x => SelectedRoleplaySession.LastMessage.Message.Contains(x.Character.Name, StringComparison.OrdinalIgnoreCase) && x.Character.Name != SelectedRoleplaySession.LastMessage.Sender.Name))
            {
                _generation.CancelToken.Dispose();
                _ = Generate(string.Empty);
            }
            else
                _generation.CancelToken.Dispose();
        }
    }
}
