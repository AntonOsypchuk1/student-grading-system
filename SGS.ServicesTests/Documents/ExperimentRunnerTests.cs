using System.Text.Json;
using Grpc.Core;
using Moq;
using SemanticAnalysis.Service.Protos;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Experiments;

namespace SGS.ServicesTests.Documents;

public sealed class ExperimentRunnerTests
{
    private static BenchmarkCase Case(string id, double max = 10, double? reference = 5,
        ReferenceKind kind = ReferenceKind.Teacher) => new(id, WorkType.Theoretical, "Explain isolation.", max,
        [new("mechanism", "Describe isolation.", 10, 1, [new("isolation", ["separate"], true)])],
        new(kind, reference, "Reference fixture; not real student data."), SubmissionText: "Processes are separate.");

    private static PreparedDataset Prepared(params BenchmarkCase[] cases) => new(new("fixture", "1", cases), "hash",
        "native", "eng", cases.Select(c => new PreparedCase(c.Id, "source", c.SubmissionText!, ExtractionStatus.Complete,
            [], [], [])).ToArray());

    [Fact]
    public async Task Preparation_freezes_text_and_chunk_source_offsets()
    {
        var dir = Directory.CreateTempSubdirectory("sgs-experiment-");
        try
        {
            var manifest = Path.Combine(dir.FullName, "dataset.json");
            var input = new BenchmarkDataset("test", "1", [Case("a")]);
            await File.WriteAllTextAsync(manifest, JsonSerializer.Serialize(input, BenchmarkJson.Options));
            var processor = new Mock<IDocumentProcessor>(MockBehavior.Strict);
            var prepared = await DatasetPreparer.PrepareAsync(manifest, processor.Object, "native", "eng");
            var submission = Assert.Single(prepared.Submissions);
            Assert.Equal("Processes are separate.", submission.Text);
            var chunk = Assert.Single(submission.Chunks);
            var span = Assert.Single(chunk.Sources);
            var source = Assert.Single(submission.Sources);
            Assert.Equal(source.Text.Substring(span.StartOffset, span.Length), chunk.Text);
            Assert.Equal(DatasetPreparer.Sha256(await File.ReadAllBytesAsync(manifest)), prepared.ManifestSha256);
            var roundtrip = JsonSerializer.Deserialize<PreparedDataset>(JsonSerializer.Serialize(prepared, BenchmarkJson.Options), BenchmarkJson.Options)!;
            Assert.Equal(submission.Text, Assert.Single(roundtrip.Submissions).Text);
            processor.VerifyNoOtherCalls();
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public async Task Repetitions_record_failures_and_normalize_error_without_mixing_reference_kinds()
    {
        var data = Prepared(Case("a"), Case("b", 100, 50), Case("c", 10, 10, ReferenceKind.Synthetic),
            Case("d", 10, null, ReferenceKind.Unlabeled));
        var calls = 0;
        var runner = new ExperimentRunner((request, _) =>
        {
            Assert.Equal("open-answer-v1", request.Pipeline);
            Assert.Equal("Processes are separate.", request.DocumentText);
            Assert.Single(request.Criteria);
            var n = calls++;
            if (n == 5) throw new RpcException(new Status(StatusCode.Unavailable, "private detail"));
            return Task.FromResult(new EvaluateDocumentResponse
            {
                Score = request.MaxScore * (n == 4 ? .4 : .6), MaxScore = request.MaxScore,
                RiskIndicator = 0.2, RequiresTeacherReview = n == 4, MethodVersion = "fixture"
            });
        });
        var attempts = new List<EvaluationAttempt>();
        await foreach (var attempt in runner.RunAsync(data, "open-answer-v1", 2)) attempts.Add(attempt);
        Assert.Equal(8, attempts.Count);
        Assert.Equal(AttemptStatus.RpcFailure, attempts[5].Status);
        Assert.Null(attempts[5].Score);
        Assert.Equal("Unavailable", attempts[5].Failure);
        var metrics = ExperimentMetrics.Calculate(attempts);
        var teacher = metrics.Single(m => m.ReferenceKind == ReferenceKind.Teacher && m.WorkType is null);
        Assert.Equal(3, teacher.Scored);
        Assert.Equal(10, teacher.Mae!.Value, 8);
        Assert.Equal(10, teacher.Rmse!.Value, 8);
        Assert.Equal(1, teacher.Within10Points);
        Assert.Equal(0.25, teacher.FailureRate);
        Assert.Equal(0.5, teacher.ReviewRate);
        Assert.Equal(0.5, teacher.AutomaticCoverage);
        Assert.Equal(20, teacher.MeanRepeatedScoreRange!.Value, 8);
        Assert.Null(metrics.Single(m => m.ReferenceKind == ReferenceKind.Unlabeled && m.WorkType is null).Mae);
        Assert.Equal(40, metrics.Single(m => m.ReferenceKind == ReferenceKind.Synthetic && m.WorkType is null).Mae!.Value, 8);
    }

    [Fact]
    public async Task Unreadable_and_missing_scores_are_not_zero_grades()
    {
        var data = Prepared(Case("a"), Case("b"));
        data = data with { Submissions = [data.Submissions[0] with { Status = ExtractionStatus.Unreadable }, data.Submissions[1]] };
        var calls = 0;
        var runner = new ExperimentRunner((_, _) =>
        {
            calls++;
            return Task.FromResult(new EvaluateDocumentResponse { MaxScore = 10, RiskIndicator = 1 });
        });
        var attempts = new List<EvaluationAttempt>();
        await foreach (var a in runner.RunAsync(data, "open-answer-v1", 1)) attempts.Add(a);
        Assert.Equal(1, calls);
        Assert.Equal(AttemptStatus.ExtractionUnavailable, attempts[0].Status);
        Assert.Equal(AttemptStatus.NoScore, attempts[1].Status);
        Assert.All(attempts, a => { Assert.Null(a.Score); Assert.True(a.RequiresReview); });
        Assert.Null(ExperimentMetrics.Calculate(attempts)[0].Mae);
    }

    [Fact]
    public void Rejects_invalid_reference_grade_and_duplicate_case_ids()
    {
        Assert.Throws<InvalidDataException>(() => DatasetPreparer.Validate(Prepared(Case("a", reference: 11)).Dataset));
        Assert.Throws<InvalidDataException>(() => DatasetPreparer.Validate(Prepared(Case("a"), Case("a")).Dataset));
        Assert.Throws<InvalidDataException>(() => DatasetPreparer.Validate(Prepared(Case("a", kind: ReferenceKind.Unlabeled)).Dataset));
    }
}
