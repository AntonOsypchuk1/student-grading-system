using System.Text;

namespace SemanticAnalysis.Service.Evaluation.Providers;

// Accept only contiguous source text. Whitespace runs may differ in the model's
// copy, but all stored evidence is the original, unmodified source substring.
internal sealed class VerbatimQuoteMatcher(string document)
{
    private (string Text, List<(int Start, int End)> Offsets)? normalizedDocument;

    public bool TryMatch(string quote, out string verbatim)
    {
        verbatim = "";
        if (string.IsNullOrWhiteSpace(quote))
            return false;
        if (document.Contains(quote, StringComparison.Ordinal))
        {
            verbatim = quote;
            return true;
        }

        normalizedDocument ??= Normalize(document);
        var (text, offsets) = normalizedDocument.Value;
        var normalizedQuote = Normalize(quote.Trim()).Text;
        var index = text.IndexOf(normalizedQuote, StringComparison.Ordinal);
        if (index < 0)
            return false;

        var start = offsets[index].Start;
        var end = offsets[index + normalizedQuote.Length - 1].End;
        verbatim = document[start..end];
        return true;
    }

    private static (string Text, List<(int Start, int End)> Offsets) Normalize(string text)
    {
        var result = new StringBuilder(text.Length);
        var offsets = new List<(int Start, int End)>(text.Length);
        for (var index = 0; index < text.Length;)
        {
            var start = index++;
            if (char.IsWhiteSpace(text[start]))
            {
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;
                result.Append(' ');
            }
            else
            {
                result.Append(text[start]);
            }
            offsets.Add((start, index));
        }
        return (result.ToString(), offsets);
    }
}
