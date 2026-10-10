namespace SemanticAnalysis.Service.Evaluation;

public sealed record ExpectedConcept(string Id, IReadOnlyList<string> Alternatives, bool Required = false);
public sealed record RubricCriterion(string Id, string Description, double MaxScore, double Weight,
    IReadOnlyList<ExpectedConcept> Concepts);
public sealed record EvaluationRequest(string DocumentText, string Question,
    IReadOnlyList<RubricCriterion> Criteria, double MaxScore = 100, string Pipeline = "open-answer-v1");
public sealed record Evidence(string CriterionId, string Source, double NormalizedScore,
    IReadOnlyList<string> Quotes, IReadOnlyList<string> MissingRequiredConcepts, string Explanation,
    bool HadRetry = false);
public sealed record EvidenceBatch(string Source, IReadOnlyList<Evidence> Items, string? Failure = null);
public sealed record CriterionEvaluation(string Id, double NormalizedScore, double LlmWeight,
    double Disagreement, IReadOnlyList<Evidence> Evidence);
public sealed record RiskAssessment(double Indicator, bool RequiresTeacherReview, IReadOnlyList<string> Factors);
public sealed record EvaluationResult(double? Score, double MaxScore,
    IReadOnlyList<CriterionEvaluation> Criteria, RiskAssessment Risk, string MethodVersion);

public interface IEvaluationPipeline
{
    string Name { get; }
    Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default);
}
public interface IGradingEvidenceProvider
{
    string Source { get; }
    Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken);
}
public interface IGradeFusionStrategy
{
    CriterionEvaluation Fuse(RubricCriterion criterion, IReadOnlyList<Evidence> evidence);
}
public interface IRiskEstimator
{
    RiskAssessment Estimate(IReadOnlyList<CriterionEvaluation> criteria);
}
