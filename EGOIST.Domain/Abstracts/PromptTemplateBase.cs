using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Abstracts;

public abstract class PromptTemplateBase : EntityBase, IPromptTemplate
{
    private string _name = string.Empty;

    public string Name { get => _name; set => Notify(ref _name, value); }
    public string Type { get; init; } = "Text";
    public string Category { get; init; } = "Prompt";
    [field: NonSerialized]
    public string? Content { get; set; }
}