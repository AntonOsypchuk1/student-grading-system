using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation;

public sealed class EvaluationDispatcher(IEnumerable<IEvaluationPipeline> pipelines)
{
    public Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        var pipeline = pipelines.SingleOrDefault(p => p.Name == request.Pipeline)
            ?? throw new ArgumentException("Unsupported evaluation pipeline.");
        
        return pipeline.EvaluateAsync(request, cancellationToken);
    }
}
