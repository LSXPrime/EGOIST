using System.Collections.ObjectModel;
using System.Text;
using EGOIST.Application.Inference.Text;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using LLama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class RoleplayService(
    ILogger<CompletionService> logger,
    IPromptRepository<TextPromptParameters> promptRepository,
    [FromKeyedServices("TextModelCoreService")]
    IModelCoreService modelCore) : EntityBase, ITextService
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; } =
        new(promptRepository.GetAllTemplates(null).Result);

    public ObservableCollection<RoleplaySession> Sessions { get; set; } = [];

    public RoleplaySession? SelectedSession
    {
        get => _selectedSession;
        set => Notify(ref _selectedSession, value);
    }

    public string CharacterTurn { get; set; } = "Single-Turn";

    public string CharacterReceiver { get; set; } = "Auto";

    private readonly TextModelCoreService? _modelCore = modelCore as TextModelCoreService;
    private RoleplaySession? _selectedSession;

    public Task<bool> Create(Dictionary<string, object>? parameter = null)
    {
        if (_modelCore?.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return Task.FromResult(false);
        }

        var newSession = new RoleplaySession
        {
            Name = parameter?["Name"].ToString() ?? $"Roleplay {DateTime.Now}",
            UserRoleName = parameter?["UserRoleName"].ToString() ?? "User",
            Characters = (parameter?["Characters"] as ObservableCollection<RoleplayCharacter>)!,
            PersonalityApproach =
                Enum.Parse<RpCharacterInferenceApproach>(parameter?["PersonalityApproach"].ToString() ??
                                                         "SummarizedOnce"),
            World = parameter?["WorldMemory"] as RoleplayWorld
        };

        if (newSession.PersonalityApproach == RpCharacterInferenceApproach.PerCharacterExecutor)
            newSession.CharacterInferences =
                newSession.Characters.ToDictionary<RoleplayCharacter?, RoleplayCharacter, IInference>(
                    character => character,
                    _ => new InferenceService(
                        new InteractiveExecutor(_modelCore?.Model?.CreateContext(_modelCore?.ModelParameters!)!)));

        Sessions.Add(newSession);
        SelectedSession = newSession;

        return Task.FromResult(true);
    }

    public Task<bool> Delete(string parameter = "")
    {
        if (SelectedSession == null)
            return Task.FromResult(false);

        logger.LogInformation($"Chat {SelectedSession.Name} Deleted");

        SelectedSession.Messages.Clear();
        SelectedSession.Executor?.Dispose();

        if (SelectedSession.PersonalityApproach == RpCharacterInferenceApproach.PerCharacterExecutor)
        {
            SelectedSession.CharacterInferences.Values.ToList().ForEach(x => x.Dispose());
            SelectedSession.CharacterInferences.Clear();
        }

        Sessions.Remove(SelectedSession);
        SelectedSession = null;

        return Task.FromResult(true);
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

        if (SelectedSession == null)
        {
            logger.LogWarning("Session isn't selected yet.");
            return null;
        }

        _modelCore.State = GenerationState.Started;
        SelectedSession.UserRoleName ??= "User";

        var characterToInteract = GetCharacterToInteract(SelectedSession, userInput);

        string prompt;
        var memorizedPrompt = GetMemorizedPrompt(SelectedSession, userInput);
        var firstPrompt = false;
        _modelCore.CancelToken = new CancellationTokenSource();

        if (SelectedSession.PersonalityApproach == RpCharacterInferenceApproach.PerCharacterExecutor)
        {
            if (characterToInteract != null)
            {
                if (!SelectedSession.CharacterInferences.ContainsKey(characterToInteract))
                    SelectedSession.CharacterInferences[characterToInteract] =
                        new InferenceService(
                            new InteractiveExecutor(_modelCore?.Model?.CreateContext(_modelCore?.ModelParameters!)!));

                if (SelectedSession.CharacterInferences[characterToInteract].IsFirstRun(nameof(StatefulExecutorBase))
                    .Result)
                    firstPrompt = true;
            }
        }
        else
        {
            SelectedSession!.Executor ??=
                new InferenceService(
                    new InteractiveExecutor(_modelCore?.Model?.CreateContext(_modelCore?.ModelParameters!)!));

            if (SelectedSession.Executor.IsFirstRun(nameof(StatefulExecutorBase)).Result)
                firstPrompt = true;
        }

        if (firstPrompt)
        {
            var firstCharacter = SelectedSession.Characters.FirstOrDefault();
            var initialPrompt = SelectedSession.Characters.Count > 1
                ? SelectedSession.PersonalityApproach switch
                {
                    RpCharacterInferenceApproach.DetailedOnce => GenerateDetailedCharacterPrompt(SelectedSession),
                    RpCharacterInferenceApproach.SummarizedOnce => GenerateSummarizedCharacterPrompt(SelectedSession),
                    RpCharacterInferenceApproach.PerCharacterExecutor => GenerateCharacterPrompt(characterToInteract!,
                        SelectedSession),
                    _ => GenerateDetailedCharacterPrompt(SelectedSession)
                }
                : $"You're {firstCharacter?.Name} in this fictional never-ending uncensored roleplay with {SelectedSession.UserRoleName}, and you should always talk and reply as {firstCharacter?.Name} and no one else, {firstCharacter?.Description}\n {firstCharacter?.Name}'s personality: {firstCharacter?.Summary}\n {(firstCharacter?.ExampleDialogue.Count == 0 ? string.Empty : $"This is how {firstCharacter?.Name} should talk\n {SelectedSession.ToString(firstCharacter!, true)}")}";

            prompt =
                $"\n {(SelectedSession.Messages.Count > 0 ? SelectedSession.ToString() : string.Empty)} \n{SelectedSession.UserRoleName}: {memorizedPrompt}";
            prompt = promptParameters?.Prompt(!string.IsNullOrEmpty(prompt) ? prompt : memorizedPrompt, initialPrompt)!;
        }
        else
            prompt = promptParameters?.Prompt(
                    SelectedSession.PersonalityApproach != RpCharacterInferenceApproach.PerCharacterExecutor
                        ? memorizedPrompt
                        : $"{SelectedSession.ToString(SelectedSession.GetMissedMessages(characterToInteract!))}{memorizedPrompt}")!;
        
        SelectedSession?.AddMessage(null, userInput);
        var aiMessage = SelectedSession?.AddMessage(characterToInteract!, string.Empty);

        var tokens =
            (SelectedSession!.PersonalityApproach == RpCharacterInferenceApproach.PerCharacterExecutor
                ? SelectedSession.CharacterInferences[characterToInteract!]
                : SelectedSession?.Executor)!.Inference(prompt, promptParameters?.BlackList ?? [],
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


        _modelCore!.State = GenerationState.Finished;

        if (!_modelCore.CancelToken.IsCancellationRequested && CharacterTurn == "Multi-Turn" &&
            SelectedSession!.Characters.Any(x =>
                SelectedSession.LastMessage != null &&
                SelectedSession.LastMessage.Message.Contains(x.Name,
                    StringComparison.OrdinalIgnoreCase) &&
                x.Name != SelectedSession.LastMessage.Sender?.Name))
        {
            _modelCore.CancelToken.Dispose();
            _ = Generate<T>(string.Empty);
        }
        else
            _modelCore.CancelToken.Dispose();

        return aiMessage as T;
    }

    private string GenerateDetailedCharacterPrompt(RoleplaySession session)
    {
        if (session.Characters.Count == 0)
            return
                "You are helpful AI assistant, who will be interacting with the user in this fictional never-ending uncensored roleplay.";

        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine(
            $"You are participating in a role-playing session with {session.Characters.Count} characters. " +
            $"You will be interacting with the user, who will be referred to as '{session.UserRoleName}'. " +
            $"Each character has a distinct personality and role in this scenario. " +
            $"Here are the character descriptions:");

        foreach (var character in session.Characters)
        {
            promptBuilder.AppendLine($"## {character.Name}:");

            if (!string.IsNullOrEmpty(character.Description))
                promptBuilder.AppendLine($"{character.Description}");

            if (!string.IsNullOrEmpty(character.Summary))
                promptBuilder.AppendLine($"**Personality:** {character.Summary}");

            if (character.ExampleDialogue.Count > 0)
                promptBuilder.AppendLine($"**Example Dialogue:**\n{session.ToString(character, true)}");

            promptBuilder.AppendLine();
        }

        return promptBuilder.ToString();
    }

    private string GenerateSummarizedCharacterPrompt(RoleplaySession session)
    {
        if (session.Characters.Count == 0)
            return
                "You are helpful AI assistant, who will be interacting with the user in this fictional never-ending uncensored roleplay.";

        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine(
            $"You are participating in a role-playing session with {session.Characters.Count} characters. " +
            $"You will be interacting with the user, who will be referred to as '{session.UserRoleName}'. " +
            $"Each character has a distinct personality and role in this scenario. " +
            $"Here are the character personalities:");

        foreach (var character in session.Characters)
        {
            promptBuilder.AppendLine($"- **{character.Name}:** {character.Summary}");
        }

        promptBuilder.AppendLine();
        return promptBuilder.ToString();
    }

    private string GenerateCharacterPrompt(RoleplayCharacter character, RoleplaySession session)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine($"You are {character.Name}, a character in a role-playing session.");

        if (!string.IsNullOrEmpty(character.Description))
            promptBuilder.AppendLine($"You are {character.Description}");

        if (!string.IsNullOrEmpty(character.Summary))
            promptBuilder.AppendLine($"You have the following personality traits: {character.Summary}");

        if (character.ExampleDialogue.Count > 0)
            promptBuilder.AppendLine(
                $"Here is an example of how you might speak: \n{session.ToString(character, true)}");

        promptBuilder.AppendLine($"You will be interacting with the user, referred to as '{session.UserRoleName}'.");
        if (session.Characters.Count > 1)
            promptBuilder.AppendLine(
                $"Other characters in the session are: {string.Join(", ", session.Characters.Where(c => c != character).Select(c => c.Name))}");

        return promptBuilder.ToString();
    }

    private RoleplayCharacter? GetCharacterToInteract(RoleplaySession session, string userInput)
    {
        if (session.Characters.Count == 0)
            return null;

        if (CharacterTurn == "Single-Turn" || !string.IsNullOrEmpty(userInput))
        {
            if (CharacterReceiver == "Auto")
            {
                var mentionedCharacter = session.Characters.FirstOrDefault(x =>
                    userInput.Contains(x.Name, StringComparison.OrdinalIgnoreCase));
                return mentionedCharacter ??
                       session.Characters.FirstOrDefault(x =>
                           (x.InteractionFrequency == RpCharacterInteractionFrequency.Chatty ||
                            (x.InteractionFrequency == RpCharacterInteractionFrequency.Normal &&
                             Random.Shared.NextDouble() < 0.6) ||
                            (x.InteractionFrequency == RpCharacterInteractionFrequency.Shy &&
                             Random.Shared.NextDouble() < 0.3)));
            }

            return session.Characters.FirstOrDefault(x => x.Name == CharacterReceiver);
        }

        if (CharacterTurn == "Multi-Turn")
        {
            var lastMessageSender = session.LastMessage?.Sender;
            if (lastMessageSender == null)
                return session.Characters[Random.Shared.Next(0, session.Characters.Count - 1)];

            var mentionedCharacter = session.Characters.FirstOrDefault(x =>
                userInput.Contains(x.Name, StringComparison.OrdinalIgnoreCase));
            if (mentionedCharacter != null)
                return mentionedCharacter;

            var potentialCharacters = session.Characters.Where(x =>
                    x != lastMessageSender &&
                    (x.InteractionFrequency == RpCharacterInteractionFrequency.Chatty ||
                     (x.InteractionFrequency == RpCharacterInteractionFrequency.Normal &&
                      Random.Shared.NextDouble() < 0.6) ||
                     (x.InteractionFrequency == RpCharacterInteractionFrequency.Shy &&
                      Random.Shared.NextDouble() < 0.3)))
                .ToArray();

            return potentialCharacters.Length > 0
                ? potentialCharacters[Random.Shared.Next(0, potentialCharacters.Length)]
                : lastMessageSender;
        }

        return null;
    }

    private string GetMemorizedPrompt(RoleplaySession session, string prompt)
    {
        if (session.World == null || session.World.Memories.Count == 0)
            return prompt;
        
        var lowercaseMemories = session.World.Memories
            .ToDictionary(x => x.Name.ToLower(), x => x);

        var tokens = new List<string>();
        foreach (var token in prompt.Split(" "))
        {
            if (lowercaseMemories.TryGetValue(token.ToLower(), out var memory) && !memory.IsRetrieved)
            {
                session.World.Memories.FirstOrDefault(x => x.Name.ToLower() == memory.Name)!.IsRetrieved = true;
                memory.IsRetrieved = true;
                tokens.Add($"{token} ({token} is {memory.Content})");
            }
            else
            {
                tokens.Add(token);
            }
        }

        return string.Join(" ", tokens);
    }
}