namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record EvaluationRequest(string DocumentText, string Question,
    IReadOnlyList<RubricCriterion> Criteria, double MaxScore = 100, string Pipeline = "open-answer-v1");
