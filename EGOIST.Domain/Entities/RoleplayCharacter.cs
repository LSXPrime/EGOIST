using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Enums;

namespace EGOIST.Domain.Entities;

public class RoleplayCharacter : BaseEntity
{
    private string _name = string.Empty;
    private string _summary = string.Empty;
    private string _scenario = string.Empty;
    private string _description = string.Empty;
    private string _firstMessage = string.Empty;
    private string _creator = string.Empty;
    private string _version = "1.0";
    private string _notes = string.Empty;
    private string _avatar = string.Empty;
    private RPCharacterInteractionFrequancy _interactionFrequancy = RPCharacterInteractionFrequancy.Normal;

    public string Name { get => _name; set => Notify(ref _name, value); }
    public string Summary { get => _summary; set => Notify(ref _summary, value); }
    public string Scenario { get => _scenario; set => Notify(ref _scenario, value); }
    public string Description { get => _description; set => Notify(ref _description, value); }
    public string FirstMessage { get => _firstMessage; set => Notify(ref _firstMessage, value); }
    public string Creator { get => _creator; set => Notify(ref _creator, value); }
    public string Version { get => _version; set => Notify(ref _version, value); }
    public string Notes { get => _notes; set => Notify(ref _notes, value); }
    public string Avatar { get => _avatar; set => Notify(ref _avatar, value); }
    public ObservableCollection<string> Tags { get; set; } = [];
    public RPCharacterInteractionFrequancy InteractionFrequancy { get => _interactionFrequancy; set => Notify(ref _interactionFrequancy, value); }
    public ObservableCollection<ChatMessage> ExampleDialogue { get; set; } = [];
}
