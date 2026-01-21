using Microsoft.Extensions.Logging;
using SemanticAnalysis.Service.Protos;
using CriterionResultResponse = SGS.Domain.Structures.PromptResponses.CriterionResultResponse;
using EvaluateCriteriaResponse = SGS.Domain.Structures.PromptResponses.EvaluateCriteriaResponse;
using GenerateSummaryResponse = SGS.Domain.Structures.PromptResponses.GenerateSummaryResponse;

namespace SGS.SemanticAnalysis.Client;

public class PromptServiceExecutor(
    SemanticAnalysisService.SemanticAnalysisServiceClient client,
    ILogger<PromptServiceExecutor> logger)
    : IPromptServiceExecutor
{
    public async Task<TResult?> ExecuteAsync<TRequest, TResult>(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return request switch
        {
            EvaluateCriteriaRequest evalReq =>
                (TResult?)(object)await ExecuteEvaluateCriteriaAsync(evalReq, cancellationToken),

            GenerateSummaryRequest summaryReq =>
                (TResult?)(object)await ExecuteGenerateSummaryAsync(summaryReq, cancellationToken),

            _ => throw new NotSupportedException($"Request type {typeof(TRequest).Name} is not supported via gRPC.")
        };
    }

    private async Task<EvaluateCriteriaResponse> ExecuteEvaluateCriteriaAsync(
        EvaluateCriteriaRequest request,
        CancellationToken cancellationToken)
    {
        var grpcRequest = new EvaluateCriteriaRequest
        {
            DocumentText = request.DocumentText,
            Criteria = { request.Criteria.Select(c => new CriterionInfoRequest
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                MaxGrade = c.MaxGrade,
                Weight = c.Weight
            }) }
        };

        var response = await client.EvaluateCriteriaAsync(grpcRequest, cancellationToken: cancellationToken);

        return new EvaluateCriteriaResponse
        {
            CriteriaResults = response.CriteriaResults.Select(r => new CriterionResultResponse
            {
                Id = r.Id,
                Title = r.Title,
                Score = r.Score,
                Weight = r.Weight,
                Justification = r.Justification,
                Strengths = r.Strengths.ToList(),
                Improvements = r.Improvements.ToList()
            }).ToList()
        };
    }

    private async Task<GenerateSummaryResponse> ExecuteGenerateSummaryAsync(
        GenerateSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var grpcRequest = new GenerateSummaryRequest
        {
            EvaluationResult = request.EvaluationResult,
            Score = request.Score,
            MaxScore = request.MaxScore
        };

        var response = await client.GenerateSummaryAsync(grpcRequest, cancellationToken: cancellationToken);

        return new GenerateSummaryResponse
        {
            Summary = response.Summary,
            Strengths = response.Strengths.ToList(),
            Improvements = response.Improvements.ToList()
        };
    }
}
