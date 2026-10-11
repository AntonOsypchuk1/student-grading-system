using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation.Abstractions;

public interface IGradeFusionStrategy
{
    CriterionEvaluation Fuse(RubricCriterion criterion, IReadOnlyList<Evidence> evidence);
}
