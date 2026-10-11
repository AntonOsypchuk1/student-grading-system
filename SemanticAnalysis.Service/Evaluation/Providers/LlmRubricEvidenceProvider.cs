using SemanticAnalysis.Service.Prompts;

namespace SemanticAnalysis.Service.Evaluation.Providers;

public sealed class LlmRubricEvidenceProvider(IStructuredPromptRunner prompts) : IGradingEvidenceProvider
{
    public string Source => "llm-rubric-v1";

    public async Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken)
    {
        var output = await prompts.RunAsync<RubricResponse>("EvaluateRubric", new
        {
            request.DocumentText, request.Question,
            Criteria = request.Criteria.Select(c => new { c.Id, c.Description, c.MaxScore })
        }, cancellationToken);
        
        var rows = output.Value.Criteria;

        if (rows is null
            || rows.Count != request.Criteria.Count
            || rows.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != rows.Count)
        {
            throw new InvalidDataException("LLM criterion set is incomplete or duplicated.");
        }

        var items = new List<Evidence>();
        foreach (var c in request.Criteria)
        {
            var row = rows.SingleOrDefault(x => x.Id == c.Id);

            if (row is null
                || !double.IsFinite(row.Score)
                || row.Score < 0
                || row.Score > c.MaxScore
                || string.IsNullOrWhiteSpace(row.Explanation)
                || row.Quotes is null
                || (row.Score > 0 && row.Quotes.Count == 0)
                || row.Quotes.Any(q =>
                    string.IsNullOrWhiteSpace(q) || !request.DocumentText.Contains(q, StringComparison.Ordinal)))
            {
                throw new InvalidDataException("LLM evidence contains an invalid score, criterion or supporting quote.");
            }

            var normalizedScore = row.Score / c.MaxScore;
            var evidence = new Evidence(c.Id, Source, normalizedScore, row.Quotes, [], row.Explanation, output.HadRetry);
            items.Add(evidence);
        }
        
        return new EvidenceBatch(Source, items);
    }
}

public sealed class RubricResponse
{
    public required List<RubricRow> Criteria { get; init; }
}

public sealed class RubricRow
{
    public required string Id { get; init; }
    public required double Score { get; init; }
    public required string Explanation { get; init; }
    public required List<string> Quotes { get; init; }
}
