using System.Text.RegularExpressions;

namespace SemanticAnalysis.Service.Evaluation.Providers;

// Literal mention coverage is an independent signal, not a semantic correctness check.
public sealed class LexicalConceptEvidenceProvider : IGradingEvidenceProvider
{
    public string Source => "lexical-concepts-v1";

    public Task<EvidenceBatch> CollectAsync(EvaluationRequest request, CancellationToken cancellationToken)
    {
        var items = new List<Evidence>();
        foreach (var criterion in request.Criteria)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quotes = new List<string>();
            var missing = new List<string>();
            var covered = 0;
            foreach (var concept in criterion.Concepts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var match = concept.Alternatives.Select(term => Regex.Match(request.DocumentText,
                    @"(?<![\p{L}\p{N}_])" + Regex.Escape(term.Trim()) + @"(?![\p{L}\p{N}_])",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
                    .FirstOrDefault(m => m.Success);
                if (match is not null) { covered++; quotes.Add(match.Value); }
                else if (concept.Required) missing.Add(concept.Id);
            }
            items.Add(new(criterion.Id, Source, (double)covered / criterion.Concepts.Count,
                quotes, missing, "Literal concept mention coverage; does not establish correctness or handle negation."));
        }
        return Task.FromResult(new EvidenceBatch(Source, items));
    }
}
