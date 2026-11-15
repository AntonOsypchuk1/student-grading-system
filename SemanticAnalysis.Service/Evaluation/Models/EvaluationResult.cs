namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record EvaluationResult(double? Score, double MaxScore,
    IReadOnlyList<CriterionEvaluation> Criteria, RiskAssessment Risk, string MethodVersion);
