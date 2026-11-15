namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed class RubricRow
{
    public required string Id { get; init; }
    public required double Score { get; init; }
    public required string Explanation { get; init; }
    public required List<string> Quotes { get; init; }
}
