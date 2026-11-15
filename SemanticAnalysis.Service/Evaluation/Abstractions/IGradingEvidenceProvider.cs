using SemanticAnalysis.Service.Evaluation.Models;

namespace SemanticAnalysis.Service.Evaluation.Abstractions;

public interface IGradingEvidenceProvider
{
    string Source { get; }
    Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken);
}
