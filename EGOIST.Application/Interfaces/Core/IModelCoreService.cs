using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;

namespace EGOIST.Application.Interfaces.Core;

public interface IModelCoreService
{
    GenerationState State { get; set; }
    GenerationMode Mode { get; set; }
    ModelInfo? SelectedGenerationModel { get; set; }
    ModelInfoWeight? SelectedGenerationWeight { get; set; }
    CancellationTokenSource? CancelToken { get; set; }
    Task Switch(ModelInfo? model, ModelInfoWeight? weight);
    Task Unload();
    
    delegate Task SwitchHandler(params object[] args);
    event SwitchHandler? OnSwitch; 
    
    delegate Task UnloadHandler(params object[] args);
    event UnloadHandler? OnUnload;
}