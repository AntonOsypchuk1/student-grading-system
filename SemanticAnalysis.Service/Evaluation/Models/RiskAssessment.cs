namespace SemanticAnalysis.Service.Evaluation.Models;

public sealed record RiskAssessment(double Indicator, bool RequiresTeacherReview, IReadOnlyList<string> Factors);
