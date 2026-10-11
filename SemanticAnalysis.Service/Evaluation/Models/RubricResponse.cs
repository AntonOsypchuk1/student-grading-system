namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed class RubricResponse
{
    public required List<RubricRow> Criteria { get; init; }
}
