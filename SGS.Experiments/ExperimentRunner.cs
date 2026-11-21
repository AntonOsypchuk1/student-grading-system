using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Google.Protobuf;
using Grpc.Core;
using SemanticAnalysis.Service.Protos;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Experiments;

public enum AttemptStatus { Scored, ExtractionUnavailable, InputTooLong, NoScore, InvalidResponse, RpcFailure }

public sealed record EvaluationAttempt(
    string CaseId,
    WorkType WorkType,
    ReferenceKind ReferenceKind,
    int Repetition,
    AttemptStatus Status,
    ExtractionStatus ExtractionStatus,
    double MaxScore,
    double? ReferenceScore,
    double? Score,
    bool RequiresReview,
    double? RiskIndicator,
    double? LatencyMs,
    string? MethodVersion,
    string? Failure,
    JsonElement? Response);

public sealed class ExperimentRunner(Func<EvaluateDocumentRequest, CancellationToken, Task<EvaluateDocumentResponse>> evaluate)
{
    public async IAsyncEnumerable<EvaluationAttempt> RunAsync(PreparedDataset prepared, string pipeline, int repetitions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DatasetPreparer.Validate(prepared.Dataset);
        
        if (string.IsNullOrWhiteSpace(pipeline) || repetitions < 1)
        {
            throw new ArgumentException("A pipeline and positive repetition count are required.");
        }
        
        if (prepared.Submissions.Count != prepared.Dataset.Cases.Count ||
            prepared.Submissions.Select(s => s.CaseId).Distinct(StringComparer.Ordinal).Count() != prepared.Submissions.Count ||
            !prepared.Dataset.Cases.Select(c => c.Id).ToHashSet(StringComparer.Ordinal)
                .SetEquals(prepared.Submissions.Select(s => s.CaseId)))
        {
            throw new InvalidDataException("Prepared submissions must match dataset case IDs exactly.");
        }
        
        var submissions = prepared.Submissions.ToDictionary(s => s.CaseId, StringComparer.Ordinal);
        
        // Interleave cases by repetition instead of running all repetitions per case consecutively.
        for (int repetition = 1; repetition <= repetitions; repetition++)
        {
            foreach (var c in prepared.Dataset.Cases)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return await EvaluateOneAsync(c, submissions[c.Id], pipeline, repetition, cancellationToken);
            }
        }
    }

    private async Task<EvaluationAttempt> EvaluateOneAsync(BenchmarkCase c, PreparedCase submission,
        string pipeline, int repetition, CancellationToken cancellationToken)
    {
        EvaluationAttempt Failed(AttemptStatus status, string reason, double? elapsed = null, JsonElement? response = null) =>
            new(c.Id, c.WorkType, c.Reference.Kind, repetition, status, submission.Status, c.MaxScore,
                c.Reference.Score, null, true, null, elapsed, null, reason, response);
        if (submission.Status == ExtractionStatus.Unreadable || string.IsNullOrWhiteSpace(submission.Text))
        {
            return Failed(AttemptStatus.ExtractionUnavailable, "Submission has no usable extracted text.");
        }
        
        if (submission.Text.Length > 100_000)
        {
            return Failed(AttemptStatus.InputTooLong, "Full-document RPC limit exceeded; input was not truncated.");
        }

        var request = new EvaluateDocumentRequest
        {
            DocumentText = submission.Text, Question = c.Question, MaxScore = c.MaxScore, Pipeline = pipeline,
            Criteria = { c.Criteria.Select(criterion => new EvaluationCriterion
            {
                Id = criterion.Id, Description = criterion.Description, MaxScore = criterion.MaxScore, Weight = criterion.Weight,
                Concepts = { criterion.Concepts.Select(concept => new ExpectedConcept
                {
                    Id = concept.Id, Required = concept.Required, Alternatives = { concept.Alternatives }
                }) }
            }) }
        };
        
        // Reference grades and provenance never enter the service request.
        var watch = Stopwatch.StartNew();
        EvaluateDocumentResponse response;
        
        try
        {
            response = await evaluate(request, cancellationToken);
        }
        catch (RpcException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return Failed(AttemptStatus.RpcFailure, ex.StatusCode.ToString(), watch.Elapsed.TotalMilliseconds);
        }
        catch (RpcException) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        cancellationToken.ThrowIfCancellationRequested();
        watch.Stop();
        
        using var json = JsonDocument.Parse(JsonFormatter.Default.Format(response));
        var raw = json.RootElement.Clone();
        
        if (!response.HasScore)
        {
            return Failed(AttemptStatus.NoScore, "Evaluator returned no grade.", watch.Elapsed.TotalMilliseconds, raw);
        }
        
        if (response.MaxScore != c.MaxScore || !double.IsFinite(response.Score) || response.Score < 0 ||
            response.Score > c.MaxScore || !double.IsFinite(response.RiskIndicator) || response.RiskIndicator is < 0 or > 1)
        {
            return Failed(AttemptStatus.InvalidResponse, "Invalid response score, scale or risk indicator.",
                watch.Elapsed.TotalMilliseconds, raw);
        }
        
        return new(c.Id, c.WorkType, c.Reference.Kind, repetition, AttemptStatus.Scored, submission.Status, c.MaxScore,
            c.Reference.Score, response.Score, response.RequiresTeacherReview, response.RiskIndicator,
            watch.Elapsed.TotalMilliseconds, response.MethodVersion, null, raw);
    }
}
