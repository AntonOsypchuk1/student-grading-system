namespace SGS.Application.Models.DocumentProcessing;

public enum DocumentFormat { Pdf, Docx }

public enum DocumentContentKind { Heading, Paragraph, Table, OcrText }

public enum ExtractionStatus
{
    Complete,
    Partial,
    Unreadable
}

public enum SourceExtractionStatus
{
    Extracted,
    EmptyOrUnreadable
}

// Source text is the canonical, normalized text against which offsets are verified.
// PDF: one source per physical page. DOCX: one source for the document body; no
// reliable physical page number is asserted for non-paginated DOCX files.
public sealed record DocumentSource(
    string SourceId,
    int? PageNumber,
    string Text,
    SourceExtractionStatus Status,
    bool HasNonTextContent = false)
{
    // OCR has its own canonical text source; native text offsets never change.
    public string? ImageId { get; init; }
}

public sealed record DocumentBlock(
    string BlockId,
    DocumentContentKind Kind,
    string Text,
    string? SectionPath,
    string SourceId,
    int StartOffset)
{
    public int Length => Text.Length;
}

public sealed record ExtractedDocument(
    string DocumentId,
    DocumentFormat Format,
    IReadOnlyList<DocumentSource> Sources,
    IReadOnlyList<DocumentBlock> Blocks,
    ExtractionStatus Status,
    IReadOnlyList<string> Warnings)
{
    public IReadOnlyList<DocumentImage> Images { get; init; } = [];
    public IReadOnlyList<ImageOcrResult> OcrResults { get; init; } = [];

    public DocumentSource Resolve(string sourceId) =>
        Sources.Single(s => s.SourceId == sourceId);
}

public sealed record ChunkSourceSpan(
    string BlockId,
    string SourceId,
    int? PageNumber,
    int StartOffset,
    int Length);

public sealed record DocumentChunk(
    string ChunkId,
    string DocumentId,
    string Text,
    string? SectionPath,
    int EstimatedTokens,
    IReadOnlyList<ChunkSourceSpan> Sources);

public sealed record DocumentProcessingResult(
    ExtractedDocument Document,
    IReadOnlyList<DocumentChunk> Chunks);
