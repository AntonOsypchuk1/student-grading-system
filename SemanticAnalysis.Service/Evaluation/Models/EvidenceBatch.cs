namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record EvidenceBatch(string Source, IReadOnlyList<Evidence> Items, string? Failure = null);
