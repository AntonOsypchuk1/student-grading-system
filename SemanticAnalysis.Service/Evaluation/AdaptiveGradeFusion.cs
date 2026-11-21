using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;
using SGS.Grading;

namespace SemanticAnalysis.Service.Evaluation;

// A versioned rule baseline. Learned/calibrated fusion can replace this strategy later.
public sealed class AdaptiveGradeFusion : IGradeFusionStrategy
{
    public CriterionEvaluation Fuse(RubricCriterion criterion, IReadOnlyList<Evidence> evidence)
    {
        var llm = evidence.Single(x => x.Source == "llm-rubric-v1");
        var concepts = evidence.Single(x => x.Source == "lexical-concepts-v1");
        var result = GradingStrategies.Fuse(GradingStrategies.LegacyHybrid, llm.NormalizedScore, concepts.NormalizedScore);
        return new(criterion.Id, result.Score, result.LlmWeight, result.Disagreement, evidence);
    }
}
