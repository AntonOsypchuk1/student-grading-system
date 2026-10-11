namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record ExpectedConcept(string Id, IReadOnlyList<string> Alternatives, bool Required = false);
