using System.Text;
using Microsoft.Extensions.Logging;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Services.Bases;
using SGS.SemanticAnalysis.Structures.PromptRequests;
using SGS.SemanticAnalysis.Structures.PromptResponses;

namespace SGS.Application.Services.Analysis;

public class SemanticKernelAiService(
    IPromptServiceExecutor promptServiceExecutor,
    IDocumentService documentService,
    ILogger<SemanticKernelAiService> logger) : IAIService
{
    public async Task<(string Summary, int GeneratedGrade)> AnalyzeDocumentAsync(
        Domain.Entities.Document document,
        List<GradingCriteria> criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Beginning analysis of document: {DocumentId}", document.Id);
            var evaluationResults = await EvaluateCriteriaAsync(criteria, document, cancellationToken);

            var (evaluationDetails, totalWeightedScore) = AnalysePromptResult(criteria, evaluationResults);
            
            var summaryResponse = await GenerateSummaryAsync(document, evaluationDetails, totalWeightedScore, cancellationToken);
            
            logger.LogInformation("Analysis complete for document {DocumentId}, final score: {Score}", 
                document.Id, totalWeightedScore);
            
            // Return the final summary and score (rounded to nearest integer)
            return (summaryResponse.Summary, (int)Math.Round(totalWeightedScore));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing document {DocumentId}", document.Id);
            throw;
        }
    }

    private async Task<EvaluateCriteriaResponse> EvaluateCriteriaAsync(
        List<GradingCriteria> criteria,
        Domain.Entities.Document document,
        CancellationToken cancellationToken)
    {
        var documentResult = await documentService.GetDocumentContentTextAsync(document.Id, cancellationToken);
        var documentText = documentResult.Data;
        
        if (string.IsNullOrWhiteSpace(documentText))
        {
            throw new Exception("Document text extraction failed or returned empty content");
        }
        
        logger.LogDebug("Document text extracted: {CharacterCount} characters", documentText.Length);
        
        var criteriaInfos = criteria.Select(c => new CriterionInfo
        {
            Id = c.Id.ToString(),
            Title = c.Title,
            Description = c.Description,
            MaxGrade = c.MaxGrade,
            Weight = c.Weight
        }).ToList();
            
        var evaluateRequest = new EvaluateCriteriaRequest
        {
            DocumentText = documentText,
            Criteria = criteriaInfos
        };
            
        logger.LogInformation("Evaluating {Count} criteria for document {DocumentId}", criteriaInfos.Count, document.Id);
        var evaluationResults = await promptServiceExecutor.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
            evaluateRequest, cancellationToken);
            
        if (evaluationResults == null || evaluationResults.CriteriaResults.Count == 0)
        {
            throw new Exception("Failed to evaluate document criteria or received empty results");
        }
        
        return evaluationResults;
    }

    private static (string, double) AnalysePromptResult(
        List<GradingCriteria> criteria,
        EvaluateCriteriaResponse evaluationResults)
    {
        double totalWeightedScore = 0;
        var totalWeight = criteria.Sum(c => c.Weight);
            
        var evaluationDetails = new StringBuilder();
        foreach (var result in evaluationResults.CriteriaResults)
        {
            var weightedScore = result.Score * (result.Weight / totalWeight);
            totalWeightedScore += weightedScore;
                
            evaluationDetails.AppendLine($"## {result.Title} (Score: {result.Score:F1}/100, Weight: {result.Weight:F2})");
            evaluationDetails.AppendLine($"**Feedback**: {result.Justification}");
                
            if (result.Strengths.Count != 0)
            {
                evaluationDetails.AppendLine("**Strengths**:");
                foreach (var strength in result.Strengths)
                {
                    evaluationDetails.AppendLine($"- {strength}");
                }
            }
                
            if (result.Improvements.Count != 0)
            {
                evaluationDetails.AppendLine("**Areas for Improvement**:");
                foreach (var improvement in result.Improvements)
                {
                    evaluationDetails.AppendLine($"- {improvement}");
                }
            }
                
            evaluationDetails.AppendLine();
        }

        return (evaluationDetails.ToString(), totalWeightedScore);
    }

    private async Task<GenerateSummaryResponse> GenerateSummaryAsync(
        Domain.Entities.Document document,
        string evaluationDetails,
        double totalWeightedScore,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating final summary for document {DocumentId}", document.Id);
        var summaryRequest = new GenerateSummaryRequest
        {
            EvaluationResult = evaluationDetails,
            Score = totalWeightedScore,
            MaxScore = 100
        };
            
        var summaryResponse = await promptServiceExecutor.ExecuteAsync<GenerateSummaryRequest, GenerateSummaryResponse>(summaryRequest, cancellationToken);
            
        if (summaryResponse == null || string.IsNullOrWhiteSpace(summaryResponse.Summary))
        {
            throw new Exception("Failed to generate summary");
        }

        return summaryResponse;
    }
}