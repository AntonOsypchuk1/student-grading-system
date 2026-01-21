using Grpc.Core;
using SemanticAnalysis.Service.Protos;
using SemanticAnalysis.Service.Services.Bases;

namespace SemanticAnalysis.Service.Services;

public class SemanticAnalysisService(
    IPromptServiceExecutor promptServiceExecutor,
    ILogger<SemanticAnalysisService> logger)
    : Protos.SemanticAnalysisService.SemanticAnalysisServiceBase
{
    public override async Task<EvaluateCriteriaResponse> EvaluateCriteria(
        EvaluateCriteriaRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("Received EvaluateCriteria request for document length: {Length}", request.DocumentText.Length);

        // Map Proto Request -> Internal DTO
        var internalRequest = new Structures.PromptRequests.EvaluateCriteriaRequest
        {
            DocumentText = request.DocumentText,
            Criteria = request.Criteria.Select(c => new Structures.PromptRequests.CriterionInfoRequest
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                MaxGrade = c.MaxGrade,
                Weight = c.Weight
            })
        };

        var result = await promptServiceExecutor.ExecuteAsync<
            Structures.PromptRequests.EvaluateCriteriaRequest,
            Structures.PromptResponses.EvaluateCriteriaResponse>(internalRequest, context.CancellationToken);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Evaluation failed to produce a result."));
        }

        // Map Internal Response -> Proto Response
        var response = new EvaluateCriteriaResponse();
        response.CriteriaResults.AddRange(result.CriteriaResults.Select(r => new CriterionResultResponse
        {
            Id = r.Id,
            Title = r.Title,
            Score = r.Score,
            Weight = r.Weight,
            Justification = r.Justification,
            Strengths = { r.Strengths },
            Improvements = { r.Improvements }
        }));

        return response;
    }

    public override async Task<GenerateSummaryResponse> GenerateSummary(
        GenerateSummaryRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("Received GenerateSummary request with score: {Score}", request.Score);

        // Map Proto Request -> Internal DTO
        var internalRequest = new Structures.PromptRequests.GenerateSummaryRequest
        {
            EvaluationResult = request.EvaluationResult,
            Score = request.Score,
            MaxScore = request.MaxScore
        };

        var result = await promptServiceExecutor.ExecuteAsync<
            Structures.PromptRequests.GenerateSummaryRequest,
            Structures.PromptResponses.GenerateSummaryResponse>(internalRequest, context.CancellationToken);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Summary generation failed to produce a result."));
        }

        // Map Internal Response -> Proto Response
        return new GenerateSummaryResponse
        {
            Summary = result.Summary,
            Strengths = { result.Strengths },
            Improvements = { result.Improvements }
        };
    }
}
