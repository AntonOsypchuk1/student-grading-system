namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record Evidence(string CriterionId, string Source, double NormalizedScore,
    IReadOnlyList<string> Quotes, IReadOnlyList<string> MissingRequiredConcepts, string Explanation,
    bool HadRetry = false);
