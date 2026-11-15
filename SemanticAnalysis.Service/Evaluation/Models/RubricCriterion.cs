namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record RubricCriterion(string Id, string Description, double MaxScore, double Weight,
    IReadOnlyList<ExpectedConcept> Concepts);
