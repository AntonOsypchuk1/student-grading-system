using System.Text.Json;
using Google.Protobuf;
using SemanticAnalysis.Service.Protos;
using SGS.Application.Models.DocumentProcessing;
using SGS.Experiments;
using SGS.Grading;

namespace SGS.ServicesTests.Documents;

public sealed class StrategyComparisonTests
{
    private static PreparedDataset Prepared() => new(
        new("comparison", "1", [new("negation", WorkType.Theoretical, "Explain encoding.", 10,
            [new("criterion", "Explain independent encoding.", 10, 1, [new("concept", ["encoding"])])],
            new(ReferenceKind.Synthetic, 0, "Constructed counterexample."), SubmissionText: "Encoding is not independent.")]),
        "manifest", "native", "eng",
        [new("negation", "source", "Encoding is not independent.", ExtractionStatus.Complete, [], [], [])]);

    private static EvaluationAttempt Attempt(int repetition = 1)
    {
        var response = new EvaluateDocumentResponse
        {
            MaxScore = 10, Score = 5, MethodVersion = GradingStrategies.LegacyHybrid,
            RiskIndicator = 0.6, RequiresTeacherReview = true,
            Criteria = { new HybridCriterionResult
            {
                Id = "criterion", NormalizedScore = 0.5, EffectiveLlmWeight = 0.5, Disagreement = 1,
                Evidence =
                {
                    new GradingEvidence { Source = GradingStrategies.LlmSource, NormalizedScore = 0 },
                    new GradingEvidence { Source = GradingStrategies.LexicalSource, NormalizedScore = 1 }
                }
            } }
        };
        using var json = JsonDocument.Parse(JsonFormatter.Default.Format(response));
        return new("negation", WorkType.Theoretical, ReferenceKind.Synthetic, repetition, AttemptStatus.Scored,
            ExtractionStatus.Complete, 10, 0, 5, true, 0.6, 5, GradingStrategies.LegacyHybrid, null, json.RootElement.Clone());
    }

    [Fact]
    public void Replay_uses_same_evidence_and_counts_failures_for_every_method()
    {
        var failed = Attempt(2) with { Status = AttemptStatus.NoScore, Score = null, Response = null,
            Failure = "Provider failed", RequiresReview = true, RiskIndicator = 1 };
        var result = StrategyComparison.Compare(Prepared(), [Attempt(), failed], GradingStrategies.LegacyHybrid, "test-model", 2, "hash", "results");
        Assert.Equal(2, result.PlannedAttempts);
        Assert.Equal(1, result.PairedAttempts);
        Assert.Equal(5, result.Methods.Count);
        foreach (var method in result.Methods)
        {
            var metrics = method.Metrics.Single(x => x.WorkType == null);
            Assert.Equal(0.5, metrics.FailureRate);
            Assert.Null(metrics.MeanLatencyMs);
            Assert.Null(method.Attempts[1].Score);
        }
        var revised = result.Methods.Single(m => m.Method == GradingStrategies.AgreementHybrid);
        Assert.Equal(0, revised.Attempts[0].Score);
        Assert.True(revised.Attempts[0].RequiresReview);
        var original = result.Methods.Single(m => m.Method == GradingStrategies.LegacyHybrid);
        Assert.Equal(50, original.Metrics.Single(m => m.WorkType == null).Mae);
        Assert.Equal(1, original.Metrics.Single(m => m.WorkType == null).LargeErrorReviewRecall);
    }

    [Fact]
    public void Replay_rejects_missing_or_duplicate_attempts_and_modified_score()
    {
        Assert.Throws<InvalidDataException>(() => StrategyComparison.Compare(Prepared(), [Attempt(), Attempt()],
            GradingStrategies.LegacyHybrid, "model", 2, "hash", "results"));
        Assert.Throws<InvalidDataException>(() => StrategyComparison.Compare(Prepared(), [Attempt() with { Score = 4 }],
            GradingStrategies.LegacyHybrid, "model", 1, "hash", "results"));
    }

    [Fact]
    public async Task Replay_requires_original_prepared_hash_and_reads_actual_jsonl()
    {
        var dir = Directory.CreateTempSubdirectory("sgs-comparison-");
        try
        {
            var path = Path.Combine(dir.FullName, "prepared.json");
            var bytes = JsonSerializer.SerializeToUtf8Bytes(Prepared(), BenchmarkJson.Options);
            await File.WriteAllBytesAsync(path, bytes);
            var run = new { id = "comparison", version = "1", preparedSha256 = DatasetPreparer.Sha256(bytes),
                pipeline = GradingStrategies.LegacyHybrid, modelLabel = "model", repetitions = 1 };
            await File.WriteAllTextAsync(Path.Combine(dir.FullName, "run.json"), JsonSerializer.Serialize(run));
            await File.WriteAllTextAsync(Path.Combine(dir.FullName, "results.jsonl"), JsonSerializer.Serialize(Attempt(), BenchmarkJson.Lines));
            var report = await StrategyComparison.CompareAsync(path, dir.FullName);
            Assert.Equal(1, report.PairedAttempts);
            Assert.Equal(run.preparedSha256, report.PreparedSha256);
            await File.AppendAllTextAsync(path, "\n");
            await Assert.ThrowsAsync<InvalidDataException>(() => StrategyComparison.CompareAsync(path, dir.FullName));
        }
        finally { dir.Delete(recursive: true); }
    }
}
