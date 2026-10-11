using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation;

public sealed class OpenAnswerEvaluationPipeline(
    IEnumerable<IGradingEvidenceProvider> providers,
    IGradeFusionStrategy fusion,
    IRiskEstimator risk,
    ILogger<OpenAnswerEvaluationPipeline> logger) : IEvaluationPipeline
{
    public string Name => "open-answer-v1";

    public async Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        EvaluationValidator.Validate(request);
        var batches = new List<EvidenceBatch>();
        
        foreach (var provider in providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var batch = await provider.CollectAsync(request, cancellationToken);

                if (batch.Source != provider.Source
                    || batch.Items.Count != request.Criteria.Count
                    || request.Criteria.Any(c => batch.Items.Count(e => e.CriterionId == c.Id) != 1)
                    || batch.Items.Any(e => e.Source != provider.Source || !double.IsFinite(e.NormalizedScore) ||
                                            e.NormalizedScore < 0 || e.NormalizedScore > 1))
                {
                    throw new InvalidDataException("Invalid evidence batch.");
                }

                batches.Add(batch);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Evidence provider {Provider} failed", provider.Source);
                batches.Add(new EvidenceBatch(provider.Source, [], "provider_failed:" + provider.Source));
            }
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var failures = batches
            .Where(b => b.Failure is not null)
            .Select(b => b.Failure!)
            .ToList();

        foreach (var required in new[] { "llm-rubric-v1", "lexical-concepts-v1" })
        {
            if (batches.Count(b => b.Source == required && b.Failure is null) != 1)
            {
                failures.Add("required_evidence_unavailable:" + required);
            }
        }

        if (failures.Count > 0)
        {
            return new EvaluationResult(
                null,
                request.MaxScore,
                [],
                new RiskAssessment(1, true, failures), Name);
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var criteria = request.Criteria.Select(c =>
        {
            var evidence = batches
                .SelectMany(b => b.Items)
                .Where(e => e.CriterionId == c.Id)
                .ToList();
            
            return fusion.Fuse(c, evidence);
        }).ToList();
        
        var totalWeight = request.Criteria.Sum(c => c.Weight);
        
        var score = criteria.Zip(request.Criteria, (c, rubric) =>
                        c.NormalizedScore * (rubric.Weight / totalWeight)).Sum() * request.MaxScore;
        
        return new EvaluationResult(score, request.MaxScore, criteria, risk.Estimate(criteria), Name);
    }
}
