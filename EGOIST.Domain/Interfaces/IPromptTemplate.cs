namespace EGOIST.Domain.Interfaces;

public interface IPromptTemplate
{
    string Name { get; set; }
    string Type { get; init; }
    string Category { get; init; }
    string? Content { get; set; }
}