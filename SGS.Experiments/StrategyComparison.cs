using System.Text.Json;
using Google.Protobuf;
using SemanticAnalysis.Service.Protos;
using SGS.Application.Models.DocumentProcessing;
using SGS.Grading;

namespace SGS.Experiments;

public sealed record StrategyComparisonReport(string DatasetId, string DatasetVersion, string PreparedSha256,
    string SourceResultsSha256, string SourceMethod, string DeclaredModel, int PlannedAttempts, int PairedAttempts,
    IReadOnlyList<StrategyComparisonMethod> Methods);

public sealed record StrategyComparisonMethod(string Method, IReadOnlyList<MetricGroup> Metrics,
    IReadOnlyList<ComparisonAttempt> Attempts);

public sealed record ComparisonAttempt(string CaseId, int Repetition, double? ReferenceScore, double? Score,
    bool RequiresReview, double? RiskIndicator, string? Failure);

public static class StrategyComparison
{
    public static async Task<StrategyComparisonReport> CompareAsync(string preparedPath, string sourceDirectory,
        CancellationToken token = default)
    {
        var bytes = await File.ReadAllBytesAsync(preparedPath, token);
        var prepared = JsonSerializer.Deserialize<PreparedDataset>(bytes, BenchmarkJson.Options)
            ?? throw new InvalidDataException("Prepared dataset is empty.");
        DatasetPreparer.Validate(prepared.Dataset);
        using var run = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(sourceDirectory, "run.json"), token));
        var meta = run.RootElement;
        var hash = DatasetPreparer.Sha256(bytes);
        if (meta.GetProperty("preparedSha256").GetString() != hash ||
            meta.GetProperty("id").GetString() != prepared.Dataset.Id ||
            meta.GetProperty("version").GetString() != prepared.Dataset.Version)
            throw new InvalidDataException("Comparison needs the exact prepared snapshot used by the source run.");
        var method = meta.GetProperty("pipeline").GetString();
        if (method != GradingStrategies.LegacyHybrid && method != GradingStrategies.FixedHybrid &&
            method != GradingStrategies.AgreementHybrid)
            throw new InvalidDataException("Comparison requires a run containing both LLM and lexical evidence.");
        var repetitions = meta.GetProperty("repetitions").GetInt32();
        if (repetitions is < 1 or > 100) throw new InvalidDataException("Invalid source repetitions.");
        var source = await File.ReadAllBytesAsync(Path.Combine(sourceDirectory, "results.jsonl"), token);
        var attempts = new List<EvaluationAttempt>();
        using (var reader = new StreamReader(new MemoryStream(source)))
        {
            while (await reader.ReadLineAsync(token) is { } line)
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrWhiteSpace(line))
                    attempts.Add(JsonSerializer.Deserialize<EvaluationAttempt>(line, BenchmarkJson.Options)
                        ?? throw new InvalidDataException("Empty result line."));
            }
        }
        return Compare(prepared, attempts, method, meta.GetProperty("modelLabel").GetString() ?? "",
            repetitions, hash, DatasetPreparer.Sha256(source));
    }

    public static StrategyComparisonReport Compare(PreparedDataset prepared, IReadOnlyList<EvaluationAttempt> source,
        string sourceMethod, string declaredModel, int repetitions, string preparedHash, string resultsHash)
    {
        if (GradingStrategies.RequiredSources(sourceMethod).Count != 2 || repetitions is < 1 or > 100)
            throw new ArgumentException("A completed two-source run is required.");
        DatasetPreparer.Validate(prepared.Dataset);
        var cases = prepared.Dataset.Cases.ToDictionary(c => c.Id);
        var submissions = prepared.Submissions.ToDictionary(c => c.CaseId);
        var expected = cases.Keys.SelectMany(id => Enumerable.Range(1, repetitions).Select(r => (id, r))).ToHashSet();
        if (source.Count != expected.Count || !source.Select(a => (a.CaseId, a.Repetition)).ToHashSet().SetEquals(expected)
            || !submissions.Keys.ToHashSet().SetEquals(cases.Keys))
            throw new InvalidDataException("Source run must contain every planned case/repetition exactly once.");

        var paired = source.Select(attempt =>
        {
            var c = cases[attempt.CaseId];
            if (attempt.ReferenceScore != c.Reference.Score || attempt.ReferenceKind != c.Reference.Kind ||
                attempt.MaxScore != c.MaxScore || attempt.WorkType != c.WorkType ||
                attempt.ExtractionStatus != submissions[c.Id].Status)
                throw new InvalidDataException($"Source metadata mismatch for {c.Id}.");
            if (attempt.Status != AttemptStatus.Scored)
                return (Attempt: attempt, Case: c, Evidence: (EvaluateDocumentResponse?)null);
            if (attempt.Response is null || attempt.MethodVersion != sourceMethod)
                throw new InvalidDataException("Scored attempt is missing evidence or has a different method.");
            var response = JsonParser.Default.Parse<EvaluateDocumentResponse>(attempt.Response.Value.GetRawText());
            if (response.MethodVersion != sourceMethod || !response.HasScore || !double.IsFinite(response.Score) ||
                response.MaxScore != c.MaxScore || response.Criteria.Count != c.Criteria.Count ||
                !response.Criteria.Select(x => x.Id).ToHashSet().SetEquals(c.Criteria.Select(x => x.Id)))
                throw new InvalidDataException("Incomplete or mismatched criterion evidence.");
            var replay = Rescore(attempt, c, response, sourceMethod);
            if (attempt.Score is null || !double.IsFinite(attempt.Score.Value) ||
                Math.Abs(replay.Score!.Value - attempt.Score.Value) > 1e-8 * c.MaxScore ||
                Math.Abs(response.Score - attempt.Score.Value) > 1e-8 * c.MaxScore)
                throw new InvalidDataException("Source score cannot be reproduced from its evidence.");
            return (Attempt: attempt, Case: c, Evidence: (EvaluateDocumentResponse?)response);
        }).ToArray();

        var methods = GradingStrategies.ReplayMethods.Select(method =>
        {
            var rescored = paired.Select(p => p.Evidence is null
                ? p.Attempt with { Score = null, LatencyMs = null, Response = null, RequiresReview = true, MethodVersion = method }
                : Rescore(p.Attempt, p.Case, p.Evidence, method)).ToArray();
            return new StrategyComparisonMethod(method, ExperimentMetrics.Calculate(rescored),
                rescored.Select(a => new ComparisonAttempt(a.CaseId, a.Repetition, a.ReferenceScore, a.Score,
                    a.RequiresReview, a.RiskIndicator, a.Failure)).ToArray());
        }).ToArray();
        return new(prepared.Dataset.Id, prepared.Dataset.Version, preparedHash, resultsHash, sourceMethod, declaredModel,
            expected.Count, paired.Count(p => p.Evidence is not null), methods);
    }

    private static EvaluationAttempt Rescore(EvaluationAttempt attempt, BenchmarkCase c, EvaluateDocumentResponse response,
        string method)
    {
        var required = GradingStrategies.RequiredSources(method);
        var weightSum = c.Criteria.Sum(x => x.Weight);
        var risk = new List<CriterionRisk>();
        var score = 0d;
        foreach (var criterion in c.Criteria)
        {
            var evidence = response.Criteria.Single(x => x.Id == criterion.Id).Evidence;
            if (evidence.Count != 2 || evidence.Count(x => x.Source == GradingStrategies.LlmSource) != 1 ||
                evidence.Count(x => x.Source == GradingStrategies.LexicalSource) != 1)
                throw new InvalidDataException("Comparison requires exactly one result from each evidence provider.");
            var selected = evidence.Where(e => required.Contains(e.Source)).ToArray();
            var fused = GradingStrategies.Fuse(method,
                selected.SingleOrDefault(e => e.Source == GradingStrategies.LlmSource)?.NormalizedScore,
                selected.SingleOrDefault(e => e.Source == GradingStrategies.LexicalSource)?.NormalizedScore);
            score += fused.Score * (criterion.Weight / weightSum);
            risk.Add(new(fused.Disagreement, selected.Any(e => e.MissingRequiredConcepts.Count > 0),
                selected.Any(e => e.HadRetry)));
        }
        var assessment = GradingStrategies.Assess(method, risk, attempt.ExtractionStatus == ExtractionStatus.Partial);
        return attempt with { Score = score * c.MaxScore, MethodVersion = method, LatencyMs = null, Response = null,
            RequiresReview = assessment.RequiresReview, RiskIndicator = assessment.Indicator };
    }
}
