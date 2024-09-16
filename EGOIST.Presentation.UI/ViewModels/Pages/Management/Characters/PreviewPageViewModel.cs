using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Domain.Entities;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Services;

namespace EGOIST.Presentation.UI.ViewModels.Pages.Management.Characters;

public partial class PreviewPageViewModel
    : ViewModelBase, INavigationAware
{
    public override string Title => "Preview";

    [ObservableProperty] private RoleplayCharacter _character = new();

    public Task Initialize(Dictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("Character", out var parameter))
        {
            Character = (RoleplayCharacter)parameter;
        }

        return Task.CompletedTask;
    }

    public Task OnNavigatedFrom() => Task.CompletedTask;
    public Task OnNavigatedTo() => Task.CompletedTask;

    [RelayCommand]
    private async Task Back() => await NavigationService.NavigateTo<CharactersPageViewModel>();

    [RelayCommand]
    private async Task Edit() =>
        await NavigationService.NavigateTo<CreatePageViewModel>(
            new Dictionary<string, object> { { "Character", Character } });
}