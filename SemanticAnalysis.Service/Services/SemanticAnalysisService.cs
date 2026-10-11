using Grpc.Core;
using SemanticAnalysis.Service.Evaluation;
using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;
using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Prompts.Models;
using SemanticAnalysis.Service.Protos;
using InternalRequest = SemanticAnalysis.Service.Evaluation.Models.EvaluationRequest;
using InternalConcept = SemanticAnalysis.Service.Evaluation.Models.ExpectedConcept;

namespace SemanticAnalysis.Service.Services;

public sealed class SemanticAnalysisService(
    EvaluationDispatcher dispatcher,
    IStructuredPromptRunner prompts,
    ILogger<SemanticAnalysisService> logger)
    : Protos.SemanticAnalysisService.SemanticAnalysisServiceBase
{
    public override Task<EvaluateDocumentResponse> EvaluateDocument(EvaluateDocumentRequest request, ServerCallContext context)
        => HandleAsync(async () =>
        {
            var criteria = request.Criteria.Select(c =>
                {
                    var concepts = c.Concepts
                        .Select(x => new InternalConcept(x.Id, x.Alternatives.ToList(), x.Required))
                        .ToList();
                    
                    return new RubricCriterion(c.Id, c.Description, c.MaxScore, c.Weight, concepts);
                })
                .ToList();
            
            var input = new InternalRequest(
                request.DocumentText,
                request.Question,
                criteria,
                request.HasMaxScore ? request.MaxScore : 100,
                string.IsNullOrEmpty(request.Pipeline) ? "open-answer-v1" : request.Pipeline);
            
            var result = await dispatcher.EvaluateAsync(input, context.CancellationToken);
            
            var response = new EvaluateDocumentResponse
            {
                MaxScore = result.MaxScore,
                RiskIndicator = result.Risk.Indicator,
                RequiresTeacherReview = result.Risk.RequiresTeacherReview,
                RiskFactors = { result.Risk.Factors },
                MethodVersion = result.MethodVersion
            };
            
            if (result.Score.HasValue)
            {
                response.Score = result.Score.Value;
            }
            
            response.Criteria.AddRange(result.Criteria.Select(c =>
            {
                var evidence = c.Evidence.Select(e => new GradingEvidence
                {
                    Source = e.Source,
                    NormalizedScore = e.NormalizedScore,
                    Quotes = { e.Quotes },
                    MissingRequiredConcepts = { e.MissingRequiredConcepts },
                    Explanation = e.Explanation,
                    HadRetry = e.HadRetry
                });
                
                return new HybridCriterionResult
                {
                    Id = c.Id,
                    NormalizedScore = c.NormalizedScore,
                    EffectiveLlmWeight = c.LlmWeight,
                    Disagreement = c.Disagreement,
                    Evidence = { evidence }
                };
            }));
            
            return response;
        }, context);

    // Compatibility adapters: the current SGS backend still consumes these RPCs.
    public override Task<EvaluateCriteriaResponse> EvaluateCriteria(EvaluateCriteriaRequest request, ServerCallContext context)
        => HandleAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.DocumentText) || request.Criteria.Count == 0 ||
                request.Criteria.Any(c => string.IsNullOrWhiteSpace(c.Id) || string.IsNullOrWhiteSpace(c.Description)) ||
                request.Criteria.Select(c => c.Id).Distinct().Count() != request.Criteria.Count)
                throw new ArgumentException("Document and unique rubric criteria are required.");
            // Existing contract reports scores on 0–100, independent of criterion max_grade.
            var input = new InternalRequest(request.DocumentText, "Evaluate the submission against the supplied rubric.",
                request.Criteria.Select(c => new RubricCriterion(c.Id, c.Description, 100, c.Weight, [])).ToList());
            var evidence = await new LlmRubricEvidenceProvider(prompts).CollectAsync(input, context.CancellationToken);
            var response = new EvaluateCriteriaResponse();
            response.CriteriaResults.AddRange(evidence.Items.Select(e =>
            {
                var criterion = request.Criteria.Single(c => c.Id == e.CriterionId);
                return new CriterionResultResponse
                {
                    Id = e.CriterionId, Title = criterion.Title, Score = e.NormalizedScore * 100,
                    Weight = criterion.Weight, Justification = e.Explanation
                };
            }));
            return response;
        }, context);

    public override Task<GenerateSummaryResponse> GenerateSummary(GenerateSummaryRequest request, ServerCallContext context)
        => HandleAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.EvaluationResult)
                || !double.IsFinite(request.Score)
                || !double.IsFinite(request.MaxScore)
                || request.MaxScore <= 0 
                || request.Score < 0
                || request.Score > request.MaxScore)
            {
                throw new ArgumentException("Summary input needs valid evaluation results and score/scale.");
            }

            var output = await prompts.RunAsync<SummaryOutput>(
                "GenerateSummary",
                new { request.EvaluationResult, request.Score, request.MaxScore },
                context.CancellationToken);

            if (string.IsNullOrWhiteSpace(output.Value.Summary)
                || output.Value.Strengths is null
                || output.Value.Improvements is null
                || output.Value.Strengths.Any(x => x is null)
                || output.Value.Improvements.Any(x => x is null))
            {
                throw new InvalidDataException("Invalid summary output.");
            }

            return new GenerateSummaryResponse
            {
                Summary = output.Value.Summary,
                Strengths = { output.Value.Strengths }, Improvements = { output.Value.Improvements }
            };
        }, context);

    private async Task<T> HandleAsync<T>(Func<Task<T>> action, ServerCallContext context)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Evaluation cancelled."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Semantic operation failed");
            throw new RpcException(new Status(StatusCode.Internal, "Semantic operation failed."));
        }
    }
}

