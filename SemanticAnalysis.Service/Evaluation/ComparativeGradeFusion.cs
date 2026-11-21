using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;
using SGS.Grading;

namespace SemanticAnalysis.Service.Evaluation;

public sealed class ComparativeGradeFusion(string method) : IGradeFusionStrategy
{
    public CriterionEvaluation Fuse(RubricCriterion criterion, IReadOnlyList<Evidence> evidence)
    {
        var result = GradingStrategies.Fuse(method,
            evidence.SingleOrDefault(x => x.Source is GradingStrategies.LlmSource or GradingStrategies.CriterionWiseSource)?.NormalizedScore,
            evidence.SingleOrDefault(x => x.Source == GradingStrategies.LexicalSource)?.NormalizedScore);
        return new(criterion.Id, result.Score, result.LlmWeight, result.Disagreement, evidence);
    }
}
