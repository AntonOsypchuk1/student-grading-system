namespace SemanticAnalysis.Service.Prompts.Models;

public sealed class SummaryOutput
{
    public required string Summary { get; init; }
    public required List<string> Strengths { get; init; }
    public required List<string> Improvements { get; init; }
}
