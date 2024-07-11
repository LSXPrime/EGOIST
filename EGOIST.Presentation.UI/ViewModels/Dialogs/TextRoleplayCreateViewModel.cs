using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EGOIST.Application.Services.Management;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;

namespace EGOIST.Presentation.UI.ViewModels.Dialogs;

public partial class TextRoleplayCreateViewModel(CharacterService? characterService) : ViewModelBase
{
    public override string Title => "Roleplay";
    
    [ObservableProperty] 
    private string? _sessionName;
    [ObservableProperty] 
    private string? _userCharacterName;
    [ObservableProperty]
    private RpCharacterInferenceApproach _personalityApproach = RpCharacterInferenceApproach.SummarizedOnce;

    public ObservableCollection<RoleplayCharacter> Characters { get; } = new(characterService?.Characters!);
    public ObservableCollection<RoleplayCharacter> SelectedCharacters { get; } = [];
}