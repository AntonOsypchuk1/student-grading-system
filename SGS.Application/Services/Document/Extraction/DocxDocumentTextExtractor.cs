using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using ProcessingFormat = SGS.Application.Models.DocumentProcessing.DocumentFormat;
using WParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WTable = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace SGS.Application.Services.Document.Extraction;

public sealed class DocxDocumentTextExtractor : IStructuredDocumentTextExtractor
{
    public bool CanExtract(string filePath) =>
        Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        var doc = await ExtractStructuredAsync(fileBytes, cancellationToken);
        return string.Join("\n", doc.Blocks.Select(b => b.Text));
    }

    public Task<ExtractedDocument> ExtractStructuredAsync(
        byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);
        cancellationToken.ThrowIfCancellationRequested();
        var blocks = new List<DocumentBlock>();
        var warnings = new List<string>();
        var sourceText = new StringBuilder();
        string? section = null;
        bool hasImages = false;

        using (var stream = new MemoryStream(fileBytes, writable: false))
        using (var word = WordprocessingDocument.Open(stream, false))
        {
            var body = word.MainDocumentPart?.Document?.Body;
            if (body is null)
            {
                warnings.Add("DOCX has no document body.");
            }
            else
            {
                foreach (OpenXmlElement element in body.Elements())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (element is WParagraph paragraph)
                    {
                        var text = ExtractionTextUtilities.Normalize(paragraph.InnerText).Trim();
                        if (text.Length == 0) continue;
                        var heading = IsHeading(paragraph);
                        if (heading) section = text;
                        AppendBlock(heading ? DocumentContentKind.Heading : DocumentContentKind.Paragraph, text);
                    }
                    else if (element is WTable table)
                    {
                        var rows = table.Elements<TableRow>()
                            .Select(row => string.Join("	", row.Elements<TableCell>().Select(cell =>
                                ExtractionTextUtilities.Normalize(string.Join(" ",
                                    cell.Descendants<WParagraph>().Select(p => p.InnerText))).Trim())))
                            .Where(row => !string.IsNullOrWhiteSpace(row))
                            .ToArray();
                        if (rows.Length > 0)
                            AppendBlock(DocumentContentKind.Table, string.Join("\n", rows));
                    }
                }
                hasImages = body.Descendants<Drawing>().Any() || body.Descendants<Picture>().Any();
                if (hasImages)
                    warnings.Add("DOCX has images/drawings that were not interpreted.");
            }
        }

        var textSource = sourceText.ToString();
        var source = new DocumentSource("docx-body", null, textSource,
            string.IsNullOrWhiteSpace(textSource)
                ? SourceExtractionStatus.EmptyOrUnreadable
                : SourceExtractionStatus.Extracted,
            hasImages);

        if (source.Status == SourceExtractionStatus.EmptyOrUnreadable)
        {
            warnings.Add("DOCX has no extractable text; manual review may be needed.");
        }

        return Task.FromResult(ExtractionTextUtilities.Build(
            ExtractionTextUtilities.DocumentId(fileBytes), ProcessingFormat.Docx,
            [source], blocks, warnings));

        void AppendBlock(DocumentContentKind kind, string text)
        {
            if (sourceText.Length > 0)
            {
                sourceText.Append('\n');
            }
            
            var offset = sourceText.Length;
            sourceText.Append(text);
            
            blocks.Add(new DocumentBlock($"b-{blocks.Count + 1:D6}", kind,
                text, section, "docx-body", offset));
        }
    }

    private static bool IsHeading(WParagraph p)
    {
        var style = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? "";
        return style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) ||
               style.StartsWith("Title", StringComparison.OrdinalIgnoreCase) ||
               style.StartsWith("Заголовок", StringComparison.OrdinalIgnoreCase) ||
               p.ParagraphProperties?.OutlineLevel is not null;
    }
}
