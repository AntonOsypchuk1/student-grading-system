using System.Text.RegularExpressions;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Services.Document.Extraction;

/// <summary>
/// Removes recurring page furniture from indexing blocks without rewriting source text.
/// Source offsets continue to address the original, normalized extracted PDF text.
/// No text is removed from DocumentSource.Text.
/// </summary>
internal static class PdfBoilerplateCleaner
{
    private static readonly Regex Lines = new(@"[^\n]+", RegexOptions.Compiled);
    private static readonly Regex WhiteSpace = new(@"\s+", RegexOptions.Compiled);

    private sealed record Line(int Offset, string Text, string Key);

    public static (IReadOnlyList<DocumentSource> Sources, List<DocumentBlock> Blocks)
        Clean(IReadOnlyList<DocumentSource> sources, List<string> warnings)
    {
        var pages = sources.Select(s => Lines.Matches(s.Text)
            .Cast<Match>()
            .Select(m => new Line(m.Index, m.Value.Trim(), Key(m.Value)))
            .Where(l => l.Text.Length > 0)
            .ToList()).ToList();

        // A line must occur on several *different* pages to be considered
        // document furniture, not merely a repeated statement on one page.
        int threshold = Math.Max(3, (int)Math.Ceiling(sources.Count * 0.5));
        var recurrent = pages.SelectMany(lines => lines.Select(l => l.Key).Distinct())
            .GroupBy(x => x).Where(g => g.Count() >= threshold && g.Key.Length >= 4)
            .Select(g => g.Key).ToHashSet(StringComparer.Ordinal);

        var retained = new List<DocumentBlock>();
        var processed = new List<DocumentSource>(sources.Count);
        string? section = null;
        for (int page = 0; page < sources.Count; page++)
        {
            var source = sources[page];
            var lines = pages[page];
            var ignored = new HashSet<int>();

            // Repeating headings in the first two lines are headers only if
            // the same text also appears on many other pages.
            for (int i = 0; i < Math.Min(1, lines.Count); i++)
                if (recurrent.Contains(lines[i].Key) && lines.Count >= 8)
                    ignored.Add(lines[i].Offset);

            int anchor = FindFooterStart(lines, recurrent);
            if (anchor >= 0)
            {
                for (int i = anchor; i < lines.Count; i++)
                    ignored.Add(lines[i].Offset);
            }
            else
            {
                // Fallback: remove only high-confidence repeated tail lines.
                for (int i = Math.Max(1, (int)(lines.Count * 0.4)); i < lines.Count; i++)
                    if (recurrent.Contains(lines[i].Key))
                        ignored.Add(lines[i].Offset);
            }

            ExtractionTextUtilities.AddPageBlocks(source.Text, source.SourceId,
                retained, ref section, ignored);

            bool usable = retained.Any(b => b.SourceId == source.SourceId);
            if (!usable && source.Status == SourceExtractionStatus.Extracted)
                warnings.Add($"PDF page {source.PageNumber} has no usable text after removing repeated page furniture; image-based content may need manual review.");

            processed.Add(source with
            {
                Status = usable ? SourceExtractionStatus.Extracted : SourceExtractionStatus.EmptyOrUnreadable
            });
        }
        return (processed, retained);
    }

    private static int FindFooterStart(List<Line> lines, HashSet<string> recurrent)
    {
        if (lines.Count == 0) return -1;
        int earliest = lines.Count <= 12 ? 0 : (int)(lines.Count * 0.35);
        for (int i = earliest; i < lines.Count; i++)
        {
            if (!recurrent.Contains(lines[i].Key)) continue;
            var tail = lines.Skip(i).ToArray();
            // A multi-line footer typically contains multiple recurrent markers
            // and short administrative labels. Do not delete long body paragraphs.
            int markers = tail.Count(line => recurrent.Contains(line.Key));
            if (markers < 2 || tail.Length > 24 || tail.Any(line => line.Text.Length > 120))
                continue;
            return i;
        }
        return -1;
    }

    private static string Key(string text) =>
        WhiteSpace.Replace(text.Trim(), " ").ToLowerInvariant();
}
