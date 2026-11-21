using Microsoft.Extensions.Logging.Abstractions;
using SemanticAnalysis.Service.Evaluation;
using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Evaluation.Models;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Prompts.Models;
using SGS.Grading;

namespace SGS.SemanticAnalysisTests;

public sealed class ComparativeStrategyTests
{
    [Theory]
    [InlineData(GradingStrategies.LlmOnly, 0)]
    [InlineData(GradingStrategies.LexicalOnly, 100)]
    [InlineData(GradingStrategies.FixedHybrid, 20)]
    [InlineData(GradingStrategies.LegacyHybrid, 50)]
    [InlineData(GradingStrategies.AgreementHybrid, 0)]
    public async Task Contradictory_keyword_rich_answer_exposes_each_strategy(string method, double expected)
    {
        var runner = new CountingPromptRunner(0);
        var pipeline = Pipeline(method, runner);
        var request = EvaluationTests.Request("There is no independent encoding or vector similarity.");
        var result = await pipeline.EvaluateAsync(request);
        Assert.Equal(expected, result.Score!.Value, 8);
        Assert.Equal(method, result.MethodVersion);
        Assert.Equal(method == GradingStrategies.LexicalOnly ? 0 : 1, runner.Calls);
        Assert.Equal(GradingStrategies.RequiredSources(method).Count, result.Criteria.Single().Evidence.Count);
        if (method is GradingStrategies.AgreementHybrid or GradingStrategies.LegacyHybrid or GradingStrategies.LexicalOnly)
            Assert.True(result.Risk.RequiresTeacherReview);
    }

    [Fact]
    public async Task Paraphrase_is_not_penalized_by_missing_literal_terms_under_strong_disagreement()
    {
        var result = await Pipeline(GradingStrategies.AgreementHybrid, new CountingPromptRunner(10))
            .EvaluateAsync(EvaluationTests.Request("Queries and passages get separate embeddings, then are ranked by distance."));
        Assert.Equal(100, result.Score);
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("agreement_gate", result.Risk.Factors);
        Assert.Contains("missing_required_concept", result.Risk.Factors);
    }

    [Fact]
    public async Task Partial_extraction_and_retry_force_review_even_when_evidence_agrees()
    {
        var result = await Pipeline(GradingStrategies.AgreementHybrid, new CountingPromptRunner(10) { HadRetry = true })
            .EvaluateAsync(EvaluationTests.Request() with { PartialExtraction = true });
        Assert.Equal(100, result.Score);
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("partial_extraction", result.Risk.Factors);
        Assert.Contains("prompt_retry", result.Risk.Factors);
        Assert.True(result.Risk.Indicator >= 0.4);
    }

    [Fact]
    public async Task Criterion_wise_evaluation_calls_one_prompt_per_criterion_and_preserves_weights()
    {
        var runner = new CountingPromptRunner(10);
        var first = EvaluationTests.Request().Criteria[0];
        var request = EvaluationTests.Request() with
        {
            MaxScore = 20,
            Criteria = [first with { Weight = 3 }, first with { Id = "second", MaxScore = 20, Weight = 1 }]
        };
        var result = await Pipeline(GradingStrategies.CriterionWise, runner).EvaluateAsync(request);
        Assert.Equal(2, runner.Calls);
        Assert.Equal(17.5, result.Score!.Value, 8);
        Assert.All(result.Criteria, c => Assert.Equal(GradingStrategies.CriterionWiseSource, Assert.Single(c.Evidence).Source));
    }

    [Fact]
    public async Task Model_failure_does_not_silently_fall_back_to_lexical_scores()
    {
        var runner = new CountingPromptRunner(10) { Failure = new HttpRequestException("private", null, System.Net.HttpStatusCode.TooManyRequests) };
        var result = await Pipeline(GradingStrategies.AgreementHybrid, runner).EvaluateAsync(EvaluationTests.Request());
        Assert.Null(result.Score);
        Assert.Contains("provider_reason:llm-rubric-v1:http_429", result.Risk.Factors);
        Assert.Equal(1, result.Risk.Indicator);
        Assert.True(result.Risk.RequiresTeacherReview);
        var lexical = await Pipeline(GradingStrategies.LexicalOnly, runner).EvaluateAsync(EvaluationTests.Request());
        Assert.Equal(100, lexical.Score);
        Assert.True(lexical.Risk.RequiresTeacherReview);
        Assert.Equal(1, runner.Calls);
    }

    [Fact]
    public async Task Criterionwise_failure_after_a_valid_criterion_abstains_from_entire_grade()
    {
        var runner = new CountingPromptRunner(10) { Failure = new InvalidDataException("invalid"), FailAt = 2 };
        var request = EvaluationTests.Request();
        request = request with { Criteria = [request.Criteria[0], request.Criteria[0] with { Id = "second" }] };
        var result = await Pipeline(GradingStrategies.CriterionWise, runner).EvaluateAsync(request);
        Assert.Equal(2, runner.Calls);
        Assert.Null(result.Score);
        Assert.Empty(result.Criteria);
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("provider_reason:llm-criterionwise-evidence-v1:invalid_evidence", result.Risk.Factors);
    }

    [Fact]
    public async Task Llm_only_supports_rubrics_without_keyword_lists()
    {
        var request = EvaluationTests.Request();
        request = request with { Criteria = [request.Criteria[0] with { Concepts = [] }] };
        Assert.Equal(100, (await Pipeline(GradingStrategies.LlmOnly, new CountingPromptRunner(10)).EvaluateAsync(request)).Score);
        await Assert.ThrowsAsync<ArgumentException>(() => Pipeline(GradingStrategies.FixedHybrid, new CountingPromptRunner(10)).EvaluateAsync(request));
    }

    [Theory]
    [InlineData(0, 0, 0.8)]
    [InlineData(0.5, 0.6, 0.88)]
    [InlineData(0, 0.25, 1)]
    [InlineData(1, 0, 1)]
    public void Agreement_weight_is_bounded_and_never_decreases_with_disagreement(double llm, double lexical, double expected)
    {
        var result = GradingStrategies.Fuse(GradingStrategies.AgreementHybrid, llm, lexical);
        Assert.Equal(expected, result.LlmWeight, 8);
        Assert.InRange(result.Score, 0, 1);
    }

    private static OpenAnswerEvaluationPipeline Pipeline(string method, IStructuredPromptRunner runner) =>
        new([new LlmRubricEvidenceProvider(runner), new LexicalConceptEvidenceProvider(), new CriterionWiseLlmEvidenceProvider(runner)],
            new ComparativeGradeFusion(method), new HeuristicRiskEstimator(), NullLogger<OpenAnswerEvaluationPipeline>.Instance, method);

    private sealed class CountingPromptRunner(double score) : IStructuredPromptRunner
    {
        public int Calls { get; private set; }
        public bool HadRetry { get; init; }
        public Exception? Failure { get; init; }
        public int FailAt { get; init; } = 1;
        public Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken)
        {
            Calls++;
            return new FakePromptRunner(score) { HadRetry = HadRetry, Failure = Calls >= FailAt ? Failure : null }.RunAsync<T>(prompt, input, cancellationToken);
        }
    }
}
