using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation.Abstractions;

public interface IEvaluationPipeline
{
    string Name { get; }
    Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default);
}
