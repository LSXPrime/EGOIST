using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Services.Text.Roleplay;
using EGOIST.Domain.Entities;
using EGOIST.Presentation.UI.Interfaces.Navigation;

namespace EGOIST.Presentation.UI.ViewModels.Dialogs;

public partial class TextRoleplayWorldMemoryViewModel(WorldMemoryService worldMemoryService) : ViewModelBase, INavigationAware
{
    public override string Title => "Roleplay - World Memory";

    public ObservableCollection<RoleplayWorld> Worlds { get; set; } = [];

    [ObservableProperty] private string? _worldName;
    [ObservableProperty] private string? _memoryName;
    [ObservableProperty] private string? _memoryContent;
    [ObservableProperty] private RoleplayWorld? _selectedWorld;
    
    public async Task Initialize(Dictionary<string, object>? parameters)
    {
        var worlds = await worldMemoryService.GetAllWorldsAsync();
        Worlds = new ObservableCollection<RoleplayWorld>(worlds);
    }

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;
    
    
    [RelayCommand]
    private async Task CreateWorld()
    {
        if (string.IsNullOrWhiteSpace(WorldName))
            return;

        if (await worldMemoryService.CreateWorldAsync(WorldName) == false)
            return;

        Worlds.Add(new RoleplayWorld
        {
            Name = WorldName
        });
        
        WorldName = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteWorld(RoleplayWorld? world)
    {
        if (world is null)
            return;

        if (await worldMemoryService.DeleteWorldAsync(world.Name) == false)
            return;

        Worlds.Remove(world);
    }

    [RelayCommand]
    private async Task CreateMemory()
    {
        if (string.IsNullOrWhiteSpace(MemoryName) || string.IsNullOrWhiteSpace(MemoryContent) || SelectedWorld is null)
            return;

        if (await worldMemoryService.StoreMemoryAsync(SelectedWorld.Name, MemoryName, MemoryContent) == false)
            return;

        SelectedWorld.Memories.Add(new RoleplayWorldMemory
        {
            Name = MemoryName,
            Content = MemoryContent
        });
        MemoryName = string.Empty;
        MemoryContent = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteMemory(RoleplayWorldMemory? memory)
    {
        if (memory is null || SelectedWorld is null)
            return;

        if (await worldMemoryService.DeleteMemoryAsync(SelectedWorld.Name, memory.Name) == false)
            return;

        SelectedWorld.Memories.Remove(memory);
        MemoryName = string.Empty;
        MemoryContent = string.Empty;
    }
}