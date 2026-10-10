using Microsoft.Extensions.Logging.Abstractions;
using SemanticAnalysis.Service.Evaluation;
using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Prompts;

namespace SGS.SemanticAnalysisTests;

public sealed class EvaluationTests
{
    internal static EvaluationRequest Request(string text = "Independent encoding and vector similarity.") => new(text,
        "Explain a bi-encoder.", [new("meaning", "Explain the mechanism.", 10, 1,
            [new("encoding", ["independent encoding"], true), new("similarity", ["vector similarity"])])]);
    internal static OpenAnswerEvaluationPipeline Pipeline(IStructuredPromptRunner runner) => new(
        [new LlmRubricEvidenceProvider(runner), new LexicalConceptEvidenceProvider()],
        new AdaptiveGradeFusion(), new HeuristicRiskEstimator(), NullLogger<OpenAnswerEvaluationPipeline>.Instance);

    [Fact]
    public async Task CompleteAnswerProducesGradeAndVerbatimEvidence()
    {
        var result = await Pipeline(new FakePromptRunner(10)).EvaluateAsync(Request());
        Assert.Equal(100, result.Score);
        Assert.False(result.Risk.RequiresTeacherReview);
        Assert.Equal(0, result.Risk.Indicator);
        Assert.Equal(2, result.Criteria.Single().Evidence.Count);
        Assert.All(result.Criteria.Single().Evidence.SelectMany(e => e.Quotes),
            q => Assert.Contains(q, Request().DocumentText));
    }

    [Fact]
    public async Task HighLlmGradeWithMissingConceptRequiresReviewAndChangesWeight()
    {
        var result = await Pipeline(new FakePromptRunner(10)).EvaluateAsync(Request("Vector similarity."));
        var criterion = result.Criteria.Single();
        Assert.Equal(0.5, criterion.Disagreement);
        Assert.Equal(0.65, criterion.LlmWeight, 8);
        Assert.Equal(82.5, result.Score!.Value, 8);
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("missing_required_concept", result.Risk.Factors);
    }

    [Fact]
    public async Task MissingRequiredConceptForcesReviewEvenWhenSourcesAgree()
    {
        var result = await Pipeline(new FakePromptRunner(0)).EvaluateAsync(Request("Nothing relevant."));
        Assert.Equal(0, result.Score);
        Assert.True(result.Risk.RequiresTeacherReview);
    }

    [Fact]
    public async Task UnavailableModelDoesNotProduceGrade()
    {
        var result = await Pipeline(new FakePromptRunner(10) { Failure = new HttpRequestException() })
            .EvaluateAsync(Request());
        Assert.Null(result.Score);
        Assert.Equal(1, result.Risk.Indicator);
        Assert.True(result.Risk.RequiresTeacherReview);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task InvalidModelScoresDoNotProduceGrade(double score)
    {
        var result = await Pipeline(new FakePromptRunner(score)).EvaluateAsync(Request());
        Assert.Null(result.Score);
        Assert.True(result.Risk.RequiresTeacherReview);
    }

    [Fact]
    public async Task FabricatedQuotationDoesNotProduceGrade()
    {
        var result = await Pipeline(new FakePromptRunner(10) { Quote = "invented evidence" }).EvaluateAsync(Request());
        Assert.Null(result.Score);
    }

    [Fact]
    public async Task PositiveGradeWithoutQuotationDoesNotProduceGrade()
    {
        var result = await Pipeline(new FakePromptRunner(10) { Quote = "" }).EvaluateAsync(Request());
        Assert.Null(result.Score);
    }

    [Fact]
    public async Task RetriedModelOutputRequiresReview()
    {
        var result = await Pipeline(new FakePromptRunner(10) { HadRetry = true }).EvaluateAsync(Request());
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("prompt_retry", result.Risk.Factors);
    }

    [Fact]
    public async Task CancellationIsNotConvertedIntoReviewResult()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Pipeline(new FakePromptRunner(10))
            .EvaluateAsync(Request(), cts.Token));
    }

    [Fact]
    public async Task LexicalMatchingSupportsAlternativesAndWordBoundaries()
    {
        var request = Request("Encoded independently with vector similarity.") with
        {
            Criteria = [new("x", "Mechanism", 1, 1,
                [new("encoding", ["independent encoding", "encoded independently"], true),
                 new("short", ["vector sim"])])]
        };
        var batch = await new LexicalConceptEvidenceProvider().CollectAsync(request, default);
        Assert.Equal(0.5, batch.Items.Single().NormalizedScore);
        Assert.Empty(batch.Items.Single().MissingRequiredConcepts);
    }

    [Fact]
    public async Task RelativeWeightsAndDifferentCriterionScalesProduceCorrectGrade()
    {
        var request = Request() with
        {
            MaxScore = 20,
            Criteria = [new("a", "First", 10, 3, [new("x", ["independent encoding"])]),
                new("b", "Second", 5, 1, [new("x", ["independent encoding"])])]
        };
        var evidence = new EvidenceBatch("llm-rubric-v1",
            [new("a", "llm-rubric-v1", 1, [], [], "first"),
             new("b", "llm-rubric-v1", 0, [], [], "second")]);
        var pipeline = new OpenAnswerEvaluationPipeline([new StaticProvider(evidence), new LexicalConceptEvidenceProvider()],
            new AdaptiveGradeFusion(), new HeuristicRiskEstimator(), NullLogger<OpenAnswerEvaluationPipeline>.Instance);
        var result = await pipeline.EvaluateAsync(request);
        Assert.Equal(17.5, result.Score!.Value, 8);
        Assert.True(result.Risk.RequiresTeacherReview); // Severe disagreement on one criterion.
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidScalesAreRejected(double scale)
        => Assert.Throws<ArgumentException>(() => EvaluationValidator.Validate(Request() with { MaxScore = scale }));

    [Fact]
    public void DuplicateCriteriaAndMissingConceptsAreRejected()
    {
        var request = Request();
        Assert.Throws<ArgumentException>(() => EvaluationValidator.Validate(request with
            { Criteria = [request.Criteria[0], request.Criteria[0]] }));
        Assert.Throws<ArgumentException>(() => EvaluationValidator.Validate(request with
            { Criteria = [request.Criteria[0] with { Concepts = [] }] }));
    }

    [Fact]
    public async Task DispatcherRejectsUnknownPipeline()
        => await Assert.ThrowsAsync<ArgumentException>(() => new EvaluationDispatcher([Pipeline(new FakePromptRunner(10))])
            .EvaluateAsync(Request() with { Pipeline = "unknown" }));

    [Fact]
    public async Task MissingRequiredProviderDoesNotProduceGrade()
    {
        var pipeline = new OpenAnswerEvaluationPipeline([new LexicalConceptEvidenceProvider()],
            new AdaptiveGradeFusion(), new HeuristicRiskEstimator(), NullLogger<OpenAnswerEvaluationPipeline>.Instance);
        Assert.Null((await pipeline.EvaluateAsync(Request())).Score);
    }

    private sealed class StaticProvider(EvidenceBatch batch) : IGradingEvidenceProvider
    {
        public string Source => batch.Source;
        public Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken)
            => Task.FromResult(batch);
    }
}

internal sealed class FakePromptRunner(double score) : IStructuredPromptRunner
{
    public Exception? Failure { get; init; }
    public string? Quote { get; init; }
    public bool HadRetry { get; init; }
    public Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Failure is not null) throw Failure;
        if (prompt == "GenerateSummary")
            return Task.FromResult(new PromptOutput<T>((T)(object)new SemanticAnalysis.Service.Services.SummaryOutput
            {
                Summary = "Deterministic summary.", Strengths = ["Clear explanation"], Improvements = []
            }, HadRetry));
        var json = System.Text.Json.JsonSerializer.SerializeToElement(input);
        var document = json.GetProperty("DocumentText").GetString()!;
        var id = json.GetProperty("Criteria")[0].GetProperty("Id").GetString()!;
        var quote = Quote ?? document;
        var response = new RubricResponse { Criteria = [new RubricRow
        {
            Id = id, Score = score, Explanation = "Deterministic test response; not a live experiment.",
            Quotes = quote.Length == 0 ? [] : [quote]
        }] };
        return Task.FromResult(new PromptOutput<T>((T)(object)response, HadRetry));
    }
}
