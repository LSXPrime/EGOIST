using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Management;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Management.Characters;

public partial class CreatePageViewModel(IFileSystemService fileSystemService, CharacterService characterService)
    : ViewModelBase, INavigationAware
{
    public override string Title => "Create";

    [ObservableProperty] private RoleplayCharacter _character = new();

    [ObservableProperty] private string _interactionFrequency = RpCharacterInteractionFrequency.Normal.ToString();
    [ObservableProperty] private byte[]? _avatar;


    #region Navigation

    public async Task Initialize(Dictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("Character", out var parameter))
        {
            Character = parameter as RoleplayCharacter ?? new RoleplayCharacter();
            InteractionFrequency = Character.InteractionFrequency.ToString();
            Avatar = await fileSystemService.ReadAllBytesAsync(
                Path.Combine(AppConfig.Instance.Parameters.CharactersPath, $@"{Character.Name}\{Character.Avatar}"));
            Avatar = Avatar.Length == 0 ? null : Avatar;
        }
    }

    public Task OnNavigatedFrom() => Task.CompletedTask;
    public Task OnNavigatedTo() => Task.CompletedTask;

    [RelayCommand]
    private async Task Back() => await NavigationService.NavigateTo<CharactersPageViewModel>();

    partial void OnInteractionFrequencyChanged(string value) =>
        Character.InteractionFrequency = Enum.Parse<RpCharacterInteractionFrequency>(value);

    #endregion

    #region Character

    [RelayCommand]
    private void AddMessage(string role) =>
        Character.ExampleDialogue.Add(new ChatMessage
        {
            Sender = !string.IsNullOrEmpty(role)
                ? role
                : (Character.ExampleDialogue[^1].Sender == Character.Name ? "User" : Character.Name),
        });

    [RelayCommand]
    private void RemoveMessage(ChatMessage message) => Character.ExampleDialogue.Remove(message);

    [RelayCommand]
    private async Task SelectImage()
    {
        var path = (await DialogService.OpenFileDialogAsync(false, new List<FilePickerFileType>
        {
            new("Images")
            {
                Patterns = new List<string>
                    { "*.bmp", "*.gif", "*.jpeg", "*.jpg", "*.pbm", "*.png", "*.tiff", "*.tga", "*.webp" },
                AppleUniformTypeIdentifiers = new List<string>
                    { "public.bmp", "public.gif", "public.jpeg", "public.png", "public.tiff" },
                MimeTypes = new List<string>
                    { "image/bmp", "image/gif", "image/jpeg", "image/png", "image/tiff", "image/tga", "image/webp" }
            },
        })).FirstOrDefault();

        if (!string.IsNullOrEmpty(path))
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Avatar = await fileSystemService.ReadAllBytesAsync(path);
                Character.Avatar = Path.GetFileName(path);
            });
    }

    [RelayCommand]
    private void ClearImage() => Avatar = null;

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(Character.Name) || Avatar is not { Length: not 0 })
            return;
        
        await characterService.CreateCharacterAsync(Character, Avatar);
        await characterService.RefreshCharactersAsync();
        await Back();
    }

    #endregion
}