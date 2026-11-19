using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Services.Document.Extraction;

internal static class ExtractionTextUtilities
{
    private static readonly Regex Lines = new(@"[^\n]+", RegexOptions.Compiled);
    private static readonly Regex NumberedTitle = new(@"^\d+(?:\.\d+)*[.)]?\s+\p{L}", RegexOptions.Compiled);
    private static readonly Regex TaskTitle = new(@"^(?:Завдання|Task|Exercise)\s*№?\s*\d+(?:[.)]|\s*:)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NamedTitle = new(@"^(?:Висновки?|Conclusions?)\s*:?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string DocumentId(byte[] data) =>
        Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    public static string Normalize(string s) => s.Replace("\r\n", "\n").Replace('\r', '\n');

    public static void AddPageBlocks(
        string pageText, string sourceId, List<DocumentBlock> blocks, ref string? section,
        IReadOnlySet<int>? ignoredLineOffsets = null)
    {
        foreach (Match match in Lines.Matches(pageText))
        {
            if (ignoredLineOffsets?.Contains(match.Index) == true) continue;
            var trimmed = match.Value.Trim();
            if (trimmed.Length == 0) continue;
            var start = match.Index + (match.Value.Length - match.Value.TrimStart().Length);
            // A page may break a heading at a hyphen. Fix the section label,
            // while keeping the original raw block texts and source offsets.
            if (blocks.Count > 0 && blocks[^1].SourceId == sourceId &&
                blocks[^1].Kind == DocumentContentKind.Heading &&
                blocks[^1].Text.EndsWith('-') && char.IsLower(trimmed[0]))
            {
                section = blocks[^1].Text[..^1] + trimmed;
                blocks[^1] = blocks[^1] with { SectionPath = section };
            }
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
        if (NumberedTitle.IsMatch(value) || TaskTitle.IsMatch(value) || NamedTitle.IsMatch(value)) return true;
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
