using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation;

// A versioned rule baseline. Learned/calibrated fusion can replace this strategy later.
public sealed class AdaptiveGradeFusion : IGradeFusionStrategy
{
    public CriterionEvaluation Fuse(RubricCriterion criterion, IReadOnlyList<Evidence> evidence)
    {
        var llm = evidence.Single(x => x.Source == "llm-rubric-v1");
        var concepts = evidence.Single(x => x.Source == "lexical-concepts-v1");
        var disagreement = Math.Abs(llm.NormalizedScore - concepts.NormalizedScore);
        var llmWeight = 0.8 - 0.3 * disagreement;
        var score = llmWeight * llm.NormalizedScore + (1 - llmWeight) * concepts.NormalizedScore;
        return new(criterion.Id, score, llmWeight, disagreement, evidence);
    }
}
