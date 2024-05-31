using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class TextPromptParameters : BaseEntity
{
    private string _templateName = string.Empty;
    private string _promptPrefix = string.Empty;
    private string _promptSuffix = string.Empty;
    private string _systemPrefix = string.Empty;
    private string _systemSuffix = string.Empty;
    private string _systemPrompt = string.Empty;

    public string TemplateName { get => _templateName; set => Notify(ref _templateName, value); }
    public string PromptPrefix { get => _promptPrefix; set => Notify(ref _promptPrefix, value); }
    public string PromptSuffix { get => _promptSuffix; set => Notify(ref _promptSuffix, value); }
    public string SystemPrefix { get => _systemPrefix; set => Notify(ref _systemPrefix, value); }
    public string SystemSuffix { get => _systemSuffix; set => Notify(ref _systemSuffix, value); }
    public string SystemPrompt { get => _systemPrompt; set => Notify(ref _systemPrompt, value); }
    public ObservableCollection<string> BlackList { get; set; } = [];

    public string Prompt(string prompt, bool system = false) => $"{(system ? $"{SystemPrefix}{SystemPrompt}{SystemSuffix}" : "")}{PromptPrefix}{prompt}{PromptSuffix}";
    public string Prompt(string prompt, string system) => $"{SystemPrefix}{system}{SystemSuffix}{PromptPrefix}{prompt}{PromptSuffix}";
    public string Prompt(string prompt, string system, string prefix, string suffix) => $"{SystemPrefix}{(string.IsNullOrEmpty(system) ? SystemPrompt : system)}{SystemSuffix}{(string.IsNullOrEmpty(prefix) ? PromptPrefix : prefix)}{prompt}{(string.IsNullOrEmpty(suffix) ? PromptSuffix : suffix)}";
    public string Prompt(string prompt, TextPromptParameters promptParameters) => $"{SystemPrefix}{(string.IsNullOrEmpty(promptParameters.SystemPrompt) ? SystemPrompt : promptParameters.SystemPrompt)}{SystemSuffix}{(string.IsNullOrEmpty(promptParameters.PromptPrefix) ? PromptPrefix : promptParameters.PromptPrefix)}{prompt}{(string.IsNullOrEmpty(promptParameters.PromptSuffix) ? PromptSuffix : promptParameters.PromptSuffix)}";
}