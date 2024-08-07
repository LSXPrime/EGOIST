﻿using System.Collections.ObjectModel;
using System.Text;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class RoleplaySession() : SessionBase<RoleplayMessage>($"Roleplay {DateTime.Now}")
{
    private string? _userRoleName = "User";
    private RoleplayMessage? _lastMessage;
    private RpCharacterInferenceApproach _personalityApproach = RpCharacterInferenceApproach.SummarizedOnce;

    public string? UserRoleName
    {
        get => _userRoleName;
        set => Notify(ref _userRoleName, value);
    }

    public RoleplayMessage? LastMessage
    {
        get => _lastMessage;
        private set => Notify(ref _lastMessage, value);
    }
    
    public RpCharacterInferenceApproach PersonalityApproach
    {
        get => _personalityApproach;
        set => Notify(ref _personalityApproach, value);
    }

    public ObservableCollection<RoleplayCharacter> Characters { get; init; } = [];
    public Dictionary<RoleplayCharacter, IInference> CharacterInferences { get; set; } = new();
    public RoleplayWorld? World { get; set; }
    
    [NonSerialized]
    public IInference? Executor;
    [NonSerialized]
    private readonly RoleplayCharacter _user = new() { Name = "User" };


    public RoleplayMessage AddMessage(RoleplayCharacter? user, string message)
    {
        var messageInput = new RoleplayMessage { Sender = user ?? _user, Message = message };
        LastMessage = messageInput;
        Messages.Add(messageInput);

        return messageInput;
    }

    public List<RoleplayMessage>? GetMissedMessages(RoleplayCharacter character)
    {
        var senderMessages = Messages.Where(m => m.Sender == character).ToList();
        if (senderMessages.Count == 0) 
            return null;
        
        var lastMessage = senderMessages.Last();
        var messagesAfterLast = Messages.SkipWhile(m => m != lastMessage).Skip(1).ToList();
        return messagesAfterLast;
    }

    public string ToString(List<RoleplayMessage>? roleplayMessages)
    {
        if (roleplayMessages == null)
            return string.Empty;

        var stringBuilder = new StringBuilder();
        foreach (var message in roleplayMessages)
            stringBuilder.Append($"{message.Sender?.Name ?? UserRoleName}: {message.Message}\n");

        stringBuilder.Append($"{UserRoleName}: ");
        return stringBuilder.ToString();
    }

    public string ToString(RoleplayCharacter character, bool exampleDialogue = false)
    {
        var stringBuilder = new StringBuilder();

        var messages = exampleDialogue
            ? character.ExampleDialogue.Select(msg => new { msg.Sender, msg.Message })
            : Messages.Where(x => x.Sender == character)
                .Select(msg => new { Sender = msg.Sender?.Name ?? UserRoleName, msg.Message });

        foreach (var message in messages)
        {
            stringBuilder.AppendLine($"{message.Sender}: {message.Message}");
        }

        return stringBuilder.ToString();
    }
}