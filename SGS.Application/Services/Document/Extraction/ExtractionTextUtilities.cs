using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Services.Document.Extraction;

internal static class ExtractionTextUtilities
{
    private static readonly Regex Lines = new(@"[^\n]+", RegexOptions.Compiled);
    private static readonly Regex NumberedTitle = new(@"^\d+(?:\.\d+)*[.)]?\s+\p{L}", RegexOptions.Compiled);

    public static string DocumentId(byte[] data) =>
        Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    public static string Normalize(string s) => s.Replace("\r\n", "\n").Replace('\r', '\n');

    public static void AddPageBlocks(
        string pageText, string sourceId, List<DocumentBlock> blocks, ref string? section)
    {
        foreach (Match match in Lines.Matches(pageText))
        {
            var trimmed = match.Value.Trim();
            if (trimmed.Length == 0) continue;
            var start = match.Index + (match.Value.Length - match.Value.TrimStart().Length);
            var heading = LooksLikeHeading(trimmed);
            if (heading) section = trimmed;
            blocks.Add(new DocumentBlock($"b-{blocks.Count + 1:D6}",
                heading ? DocumentContentKind.Heading : DocumentContentKind.Paragraph,
                trimmed, section, sourceId, start));
        }
    }

    private static bool LooksLikeHeading(string value)
    {
        if (value.Length > 110) return false;
        if (NumberedTitle.IsMatch(value)) return true;
        var letters = value.Where(char.IsLetter).ToArray();
        return letters.Length >= 4 && value.Length <= 95 && letters.All(char.IsUpper);
    }

    public static ExtractedDocument Build(
        string id,
        Models.DocumentProcessing.DocumentFormat format,
        IReadOnlyList<DocumentSource> sources,
        IReadOnlyList<DocumentBlock> blocks,
        IReadOnlyList<string> warnings)
    {
        var missing = sources.Any(s => s.Status == SourceExtractionStatus.EmptyOrUnreadable);
        var nonText = sources.Any(s => s.HasNonTextContent);
        var status = blocks.Count == 0 ? ExtractionStatus.Unreadable :
            missing || nonText ? ExtractionStatus.Partial : ExtractionStatus.Complete;
        return new ExtractedDocument(id, format, sources, blocks, status, warnings);
    }
}
