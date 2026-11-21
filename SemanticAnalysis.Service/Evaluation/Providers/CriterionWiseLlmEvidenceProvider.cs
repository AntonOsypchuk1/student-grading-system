using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SGS.Grading;

namespace SemanticAnalysis.Service.Evaluation.Providers;

// Same rubric prompt/model, but each criterion is evaluated in isolation.
// Never silently substitute a partial set of criterion scores.
public sealed class CriterionWiseLlmEvidenceProvider(
    IStructuredPromptRunner prompts,
    ILogger<LlmRubricEvidenceProvider>? logger = null) : IGradingEvidenceProvider
{
    public string Source => GradingStrategies.CriterionWiseSource;

    public async Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken)
    {
        var provider = new LlmRubricEvidenceProvider(prompts, logger);
        var evidence = new List<Evidence>();
        
        foreach (var criterion in request.Criteria)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = await provider.CollectAsync(request with { Criteria = [criterion] }, cancellationToken);
            evidence.Add(batch.Items.Single() with { Source = Source });
        }
        return new(Source, evidence);
    }
}
