using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SemanticAnalysis.Service.Evaluation;
using SemanticAnalysis.Service.Evaluation.Models;
using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Prompts.Models;
using SGS.Grading;

namespace SGS.SemanticAnalysisTests;

public sealed class RubricEvidenceRepairTests
{
    private const string Document = "Незалежне\r\n  кодування і векторна\u00a0подібність.";
    private const string Quote = "Незалежне кодування і векторна подібність.";

    [Fact]
    public async Task WhitespaceDifferencesReturnOriginalSourceWithoutExtraModelCall()
    {
        var runner = new SequenceRunner(Response(10, Quote));
        var evidence = Assert.Single((await new LlmRubricEvidenceProvider(runner)
            .CollectAsync(EvaluationTests.Request(Document), default)).Items);
        Assert.Equal(Document, Assert.Single(evidence.Quotes));
        Assert.False(evidence.HadRetry);
        Assert.Equal(new[] { "EvaluateRubric" }, runner.Prompts);
    }

    [Theory]
    [InlineData("Independent encoding and vector similarity.")]
    [InlineData("Незалежне кодування і подібність.")]
    [InlineData("Незалежне кодування і векторна подібність!")]
    [InlineData("незалежне кодування і векторна подібність.")]
    public async Task TranslationOmissionPunctuationOrCaseChangesAreNotAcceptedAsQuotes(string quote)
    {
        var runner = new SequenceRunner(Response(10, quote), Response(10, quote));
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new LlmRubricEvidenceProvider(runner).CollectAsync(EvaluationTests.Request(Document), default));
        Assert.Contains("unmatched_quote", exception.Message);
        Assert.DoesNotContain(quote, exception.Message);
        Assert.Equal(new[] { "EvaluateRubric", "RepairRubricEvidence" }, runner.Prompts);
    }

    [Fact]
    public async Task RepairedGradeRequiresTeacherReviewAndUsesOnlyCorrectedEvidence()
    {
        var runner = new SequenceRunner(Response(11, "invented"), Response(8, EvaluationTests.Request().DocumentText));
        var result = await EvaluationTests.Pipeline(runner).EvaluateAsync(EvaluationTests.Request());
        Assert.NotNull(result.Score);
        var evidence = Assert.Single(result.Criteria.Single().Evidence, e => e.Source == "llm-rubric-v1");
        Assert.Equal(0.8, evidence.NormalizedScore, 8);
        Assert.True(evidence.HadRetry);
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("prompt_retry", result.Risk.Factors);
        Assert.Equal(2, runner.Prompts.Count);
        Assert.Contains("invalid_score", runner.LastInput);
        Assert.Contains("unmatched_quote", runner.LastInput);
        Assert.DoesNotContain("invented", runner.LastInput);
    }

    [Theory]
    [InlineData("invalid_score")]
    [InlineData("missing_explanation")]
    [InlineData("missing_quotes")]
    [InlineData("empty_quote")]
    [InlineData("unknown_criterion")]
    [InlineData("duplicate_criterion")]
    [InlineData("null_criterion")]
    [InlineData("missing_criterion_set")]
    public async Task PersistentInvalidEvidenceReportsSpecificReasonAndStops(string reason)
    {
        var valid = Row(10, Document);
        var invalid = reason switch
        {
            "invalid_score" => Response(11, Document),
            "missing_explanation" => new RubricResponse { Criteria = [new()
                { Id = "meaning", Score = 10, Explanation = "", Quotes = [Document] }] },
            "missing_quotes" => new RubricResponse { Criteria = [new()
                { Id = "meaning", Score = 10, Explanation = "Explanation", Quotes = [] }] },
            "empty_quote" => Response(10, " "),
            "unknown_criterion" => new RubricResponse { Criteria = [Row(10, Document, "unknown")] },
            "duplicate_criterion" => new RubricResponse { Criteria = [valid, valid] },
            "null_criterion" => new RubricResponse { Criteria = [null!] },
            "missing_criterion_set" => new RubricResponse { Criteria = null! },
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };
        var runner = new SequenceRunner(invalid, invalid);
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new LlmRubricEvidenceProvider(runner).CollectAsync(EvaluationTests.Request(Document), default));
        Assert.Contains(reason, exception.Message);
        Assert.Equal(2, runner.Prompts.Count);
    }

    [Fact]
    public async Task CriterionWiseRepairsOnlyTheFailedCriterion()
    {
        var request = EvaluationTests.Request(Document);
        request = request with { Criteria = [request.Criteria[0], request.Criteria[0] with { Id = "second" }] };
        var runner = new SequenceRunner(Response(10, Document),
            new RubricResponse { Criteria = [Row(10, "invented", "second")] },
            new RubricResponse { Criteria = [Row(7, Document, "second")] });
        var batch = await new CriterionWiseLlmEvidenceProvider(runner).CollectAsync(request, default);
        Assert.Equal(new[] { "EvaluateRubric", "EvaluateRubric", "RepairRubricEvidence" }, runner.Prompts);
        Assert.False(batch.Items[0].HadRetry);
        Assert.True(batch.Items[1].HadRetry);
        Assert.Equal(0.7, batch.Items[1].NormalizedScore, 8);
        Assert.All(batch.Items, e => Assert.Equal(batch.Source, e.Source));
        Assert.DoesNotContain("\"Id\":\"meaning\"", runner.LastInput);
    }

    [Fact]
    public async Task CriterionWiseAbstainsIfRepairFailsAfterAnEarlierValidCriterion()
    {
        var request = EvaluationTests.Request(Document);
        request = request with { Criteria = [request.Criteria[0], request.Criteria[0] with { Id = "second" }] };
        var invalid = new RubricResponse { Criteria = [Row(10, "invented", "second")] };
        var runner = new SequenceRunner(Response(10, Document), invalid, invalid);
        var pipeline = new OpenAnswerEvaluationPipeline([new CriterionWiseLlmEvidenceProvider(runner)],
            new ComparativeGradeFusion(GradingStrategies.CriterionWise), new HeuristicRiskEstimator(),
            NullLogger<OpenAnswerEvaluationPipeline>.Instance, GradingStrategies.CriterionWise);
        var result = await pipeline.EvaluateAsync(request);
        Assert.Null(result.Score);
        Assert.Empty(result.Criteria);
        Assert.True(result.Risk.RequiresTeacherReview);
        Assert.Contains("provider_reason:llm-criterionwise-evidence-v1:invalid_evidence", result.Risk.Factors);
        Assert.Equal(3, runner.Prompts.Count);
    }

    private static RubricResponse Response(double score, string quote) => new() { Criteria = [Row(score, quote)] };
    private static RubricRow Row(double score, string quote, string id = "meaning") =>
        new() { Id = id, Score = score, Explanation = "Explanation", Quotes = [quote] };

    private sealed class SequenceRunner(params RubricResponse[] responses) : IStructuredPromptRunner
    {
        public List<string> Prompts { get; } = [];
        public string LastInput { get; private set; } = "";

        public Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = responses[Prompts.Count];
            Prompts.Add(prompt);
            LastInput = JsonSerializer.Serialize(input);
            return Task.FromResult(new PromptOutput<T>((T)(object)response, false));
        }
    }
}
