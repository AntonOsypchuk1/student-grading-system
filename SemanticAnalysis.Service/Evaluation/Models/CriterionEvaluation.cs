namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record CriterionEvaluation(string Id, double NormalizedScore, double LlmWeight,
    double Disagreement, IReadOnlyList<Evidence> Evidence);
