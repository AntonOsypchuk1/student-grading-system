using System.Text.RegularExpressions;

namespace SemanticAnalysis.Service.Evaluation.Providers;

// Literal mention coverage is an independent signal, not a semantic correctness check.
public sealed class LexicalConceptEvidenceProvider : IGradingEvidenceProvider
{
    public string Source => "lexical-concepts-v1";
    
    private const string WordBoundary = @"[\p{L}\p{N}_]";
    
    private const RegexOptions MatchOptions =
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

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
                
                var match = concept.Alternatives
                    .Select(term => FindConceptMention(request.DocumentText, term))
                    .FirstOrDefault(match => match.Success);

                if (match is not null)
                {
                    covered++;
                    quotes.Add(match.Value);
                }
                else if (concept.Required)
                {
                    missing.Add(concept.Id);
                }
            }

            var normalizedScore = (double)covered / criterion.Concepts.Count;
            var explanation = "Literal concept mention coverage; does not establish correctness or handle negation.";
            
            var evidence = new Evidence(criterion.Id, Source, normalizedScore, quotes, missing, explanation);
            
            items.Add(evidence);
        }
        
        return Task.FromResult(new EvidenceBatch(Source, items));
    }
    
    private static Match FindConceptMention(
        string documentText,
        string term)
    {
        var escapedTerm = Regex.Escape(term.Trim());

        var pattern =
            $@"(?<!{WordBoundary}){escapedTerm}(?!{WordBoundary})";

        return Regex.Match(
            documentText,
            pattern,
            MatchOptions,
            MatchTimeout);
    }
}
