using System.Text;
using Microsoft.Extensions.Logging;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Domain.Structures.PromptResponses;
using SGS.Domain.Structures.PromptRequests;
using SGS.SemanticAnalysis.Client;

namespace SGS.Application.Services.Analysis;

public class SemanticKernelAiService(
    IPromptServiceExecutor promptServiceExecutor,
    IDocumentService documentService,
    ILogger<SemanticKernelAiService> logger) : IAiService
{
    private readonly string _promptLanguage = "en";

    public async Task<(GenerateSummaryResponse Summary, int GeneratedGrade)> AnalyzeDocumentAsync(
        Domain.Entities.Document document,
        List<GradingCriteria> criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Beginning analysis of document: {DocumentId}", document.Id);
            var evaluationResults = await EvaluateCriteriaAsync(criteria, document, cancellationToken);

            var (evaluationDetails, totalWeightedScore) = AnalysePromptResult(criteria, evaluationResults, _promptLanguage);

            var summaryResponse = await GenerateSummaryAsync(document, evaluationDetails, totalWeightedScore, cancellationToken);

            logger.LogInformation("Analysis complete for document {DocumentId}, final score: {Score}",
                document.Id, totalWeightedScore);

            // Return the final summary and score (rounded to nearest integer)
            return (summaryResponse, (int)Math.Round(totalWeightedScore));
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

        var criteriaInfos = criteria.Select(c => new CriterionInfoRequest
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
        EvaluateCriteriaResponse results,
        string promptLanguage)
    {
        var isUk = promptLanguage.StartsWith("ua", StringComparison.OrdinalIgnoreCase);
        var titleTpl      = isUk
            ? "## {0} (Бал: {1:F1}/100, Вага: {2:F2})"
            : "## {0} (Score: {1:F1}/100, Weight: {2:F2})";
        var feedbackLbl   = isUk ? "**Відгук**: {0}" : "**Feedback**: {0}";
        var strengthsHdr  = isUk ? "**Сильні сторони**:" : "**Strengths**:";
        var improveHdr    = isUk ? "**Напрями для покращення**:" : "**Areas for Improvement**:";

        var sb = new StringBuilder();
        double totalWeighted = 0;
        var totalWeight = criteria.Sum(c => c.Weight);

        foreach (var r in results.CriteriaResults)
        {
            var w = r.Score * (r.Weight / totalWeight);
            totalWeighted += w;

            sb.AppendLine(string.Format(titleTpl, r.Title, r.Score, r.Weight));
            sb.AppendLine(string.Format(feedbackLbl, r.Justification));

            if (r.Strengths.Count != 0)
            {
                sb.AppendLine(strengthsHdr);
                foreach (var s in r.Strengths)
                    sb.AppendLine($"- {s}");
            }

            if (r.Improvements.Count != 0)
            {
                sb.AppendLine(improveHdr);
                foreach (var i in r.Improvements)
                    sb.AppendLine($"- {i}");
            }

            sb.AppendLine();
        }

        return (sb.ToString(), totalWeighted);
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
