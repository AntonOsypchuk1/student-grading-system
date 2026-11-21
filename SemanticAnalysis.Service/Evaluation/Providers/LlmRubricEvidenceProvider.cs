using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Evaluation.Models;
using SemanticAnalysis.Service.Prompts.Abstractions;

namespace SemanticAnalysis.Service.Evaluation.Providers;

public sealed class LlmRubricEvidenceProvider(
    IStructuredPromptRunner prompts,
    ILogger<LlmRubricEvidenceProvider>? logger = null) : IGradingEvidenceProvider
{
    public string Source => "llm-rubric-v1";

    public async Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken)
    {
        var output = await prompts.RunAsync<RubricResponse>("EvaluateRubric", new
        {
            request.DocumentText, request.Question,
            Criteria = request.Criteria.Select(c => new { c.Id, c.Description, c.MaxScore })
        }, cancellationToken);
        
        var matcher = new VerbatimQuoteMatcher(request.DocumentText);
        var (items, issues) = Validate(request, output.Value, matcher, output.HadRetry);
        if (issues.Count == 0)
            return new(Source, items);

        cancellationToken.ThrowIfCancellationRequested();
        logger?.LogWarning("Rubric evidence rejected: {Issues}. Requesting one corrective evaluation.",
            Describe(issues));
        var repaired = await prompts.RunAsync<RubricResponse>("RepairRubricEvidence", new
        {
            request.DocumentText, request.Question,
            Criteria = request.Criteria.Select(c => new { c.Id, c.Description, c.MaxScore }),
            ValidationIssues = issues
        }, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        (items, issues) = Validate(request, repaired.Value, matcher, hadRetry: true);
        if (issues.Count != 0)
            throw new InvalidDataException($"LLM evidence failed validation after one repair: {Describe(issues)}.");

        return new(Source, items);
    }

    private (List<Evidence> Items, List<ValidationIssue> Issues) Validate(
        EvaluationRequest request, RubricResponse? response, VerbatimQuoteMatcher matcher, bool hadRetry)
    {
        var issues = new List<ValidationIssue>();
        var items = new List<Evidence>();
        var rows = response?.Criteria;
        if (rows is null)
        {
            issues.Add(new("missing_criterion_set"));
            return (items, issues);
        }

        if (rows.Count != request.Criteria.Count)
            issues.Add(new("criterion_count_mismatch"));
        if (rows.Any(r => r is null))
            issues.Add(new("null_criterion"));
        if (rows.Any(r => r is not null && !request.Criteria.Any(c => c.Id == r.Id)))
            issues.Add(new("unknown_criterion"));

        foreach (var c in request.Criteria)
        {
            var matches = rows.Where(r => r is not null && r.Id == c.Id).ToList();
            if (matches.Count != 1)
            {
                issues.Add(new(matches.Count == 0 ? "missing_criterion" : "duplicate_criterion", c.Id));
                continue;
            }

            var row = matches[0];
            var issueCount = issues.Count;
            if (!double.IsFinite(row.Score) || row.Score < 0 || row.Score > c.MaxScore)
                issues.Add(new("invalid_score", c.Id));
            if (string.IsNullOrWhiteSpace(row.Explanation))
                issues.Add(new("missing_explanation", c.Id));

            var quotes = new List<string>();
            if (row.Quotes is null || (row.Score > 0 && row.Quotes.Count == 0))
                issues.Add(new("missing_quotes", c.Id));
            else
            {
                for (var index = 0; index < row.Quotes.Count; index++)
                {
                    var quote = row.Quotes[index];
                    if (string.IsNullOrWhiteSpace(quote))
                        issues.Add(new("empty_quote", c.Id, index));
                    else if (!matcher.TryMatch(quote, out var verbatim))
                        issues.Add(new("unmatched_quote", c.Id, index));
                    else
                        quotes.Add(verbatim);
                }
            }

            if (issues.Count == issueCount)
                items.Add(new(c.Id, Source, row.Score / c.MaxScore, quotes, [], row.Explanation, hadRetry));
        }

        // A partial valid set must never become a grade.
        return (items, issues);
    }

    private static string Describe(IEnumerable<ValidationIssue> issues) =>
        string.Join("; ", issues.Select(i => i.CriterionId is null
            ? i.Code
            : $"{i.Code} (criterion={i.CriterionId}, quoteIndex={i.QuoteIndex?.ToString() ?? "n/a"})"));

    private sealed record ValidationIssue(string Code, string? CriterionId = null, int? QuoteIndex = null);
}
