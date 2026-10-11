using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation.Abstractions;

public interface IRiskEstimator
{
    RiskAssessment Estimate(IReadOnlyList<CriterionEvaluation> criteria);
}
