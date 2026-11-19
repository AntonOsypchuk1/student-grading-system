using Microsoft.Extensions.Logging.Abstractions;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Extraction;
using SGS.Application.Services.Document.Processing;
using ProcessingFormat = SGS.Application.Models.DocumentProcessing.DocumentFormat;

namespace SGS.ServicesTests.Documents;

public sealed class StructuredDocumentProcessingTests
{
    private static readonly PdfDocumentTextExtractor Pdf = new(NullLogger<PdfDocumentTextExtractor>.Instance);
    private static readonly DocxDocumentTextExtractor Docx = new();
    private readonly StructureAwareChunker _chunker = new(new Utf8TokenEstimator());

    private static async Task<byte[]> FixtureAsync(string fileName) =>
        await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory,
            "Documents", "Fixtures", fileName));

    [Fact]
    public async Task Pdf_retains_physical_page_references_and_unreadable_pages()
    {
        var doc = await Pdf.ExtractStructuredAsync(await FixtureAsync("sample-report.pdf"));
        Assert.Equal(ProcessingFormat.Pdf, doc.Format);
        Assert.Equal(3, doc.Sources.Count);
        Assert.Equal(ExtractionStatus.Partial, doc.Status);
        Assert.Equal(SourceExtractionStatus.EmptyOrUnreadable, doc.Sources[2].Status);
        Assert.Equal(3, doc.Sources[2].PageNumber);
        Assert.Contains(doc.Blocks, b => b.Text.Contains("Retrieval", StringComparison.Ordinal));
        Assert.All(doc.Blocks, block => AssertSourceMatches(doc, block));
    }

    [Fact]
    public async Task Docx_preserves_heading_table_and_non_paginated_source_offsets()
    {
        var doc = await Docx.ExtractStructuredAsync(await FixtureAsync("sample-report.docx"));
        Assert.Equal(ProcessingFormat.Docx, doc.Format);
        Assert.Single(doc.Sources);
        Assert.Null(doc.Sources[0].PageNumber); // DOCX pages require layout/rendering.
        Assert.Contains(doc.Blocks, b => b.Kind == DocumentContentKind.Heading);
        Assert.Contains(doc.Blocks, b => b.Kind == DocumentContentKind.Table && b.Text.Contains("Recall"));
        Assert.All(doc.Blocks, block => AssertSourceMatches(doc, block));
    }

    [Theory]
    [InlineData("image-only.pdf", true)]
    [InlineData("image-only.docx", false)]
    public async Task Image_only_documents_are_unreadable_not_zero_scored(string name, bool pdf)
    {
        var bytes = await FixtureAsync(name);
        var doc = pdf ? await Pdf.ExtractStructuredAsync(bytes) : await Docx.ExtractStructuredAsync(bytes);
        Assert.Equal(ExtractionStatus.Unreadable, doc.Status);
        Assert.Empty(doc.Blocks);
        Assert.Empty(_chunker.Chunk(doc));
        Assert.NotEmpty(doc.Warnings);
    }

    [Fact]
    public async Task Legacy_pdf_and_docx_text_extraction_still_work()
    {
        var extractors = new IDocumentTextExtractor[] { Pdf, Docx, new TxtDocumentTextExtractor() };
        var manager = new DocumentTextExtractionManager(extractors,
            NullLogger<DocumentTextExtractionManager>.Instance);
        var pdf = await manager.ExtractTextAsync("sample.pdf", await FixtureAsync("sample-report.pdf"));
        var docx = await manager.ExtractTextAsync("sample.docx", await FixtureAsync("sample-report.docx"));
        Assert.Contains("Retrieval", pdf);
        Assert.Contains("Recall", docx);
    }

    [Fact]
    public async Task Processing_result_has_stable_chunks_and_resolvable_source_spans()
    {
        var bytes = await FixtureAsync("lab1.pdf");
        IDocumentProcessor processor = new DocumentProcessor([Pdf, Docx], _chunker);
        var options = new ChunkingOptions { MaxEstimatedTokens = 36, OverlapEstimatedTokens = 7 };
        var first = await processor.ProcessAsync("lab1.pdf", bytes, options);
        var second = await processor.ProcessAsync("lab1.pdf", bytes, options);
        Assert.NotEmpty(first.Chunks);
        Assert.Equal(first.Chunks.Select(x => x.ChunkId), second.Chunks.Select(x => x.ChunkId));
        Assert.All(first.Chunks, chunk =>
        {
            Assert.InRange(chunk.EstimatedTokens, 1, options.MaxEstimatedTokens);
            foreach (var span in chunk.Sources)
            {
                var source = first.Document.Resolve(span.SourceId);
                var piece = source.Text.Substring(span.StartOffset, span.Length);
                Assert.False(string.IsNullOrWhiteSpace(piece));
                Assert.Contains(piece, chunk.Text, StringComparison.Ordinal);
                if (first.Document.Format == ProcessingFormat.Docx)
                    Assert.Null(span.PageNumber);
            }
        });
    }

    [Fact]
    public void Long_blocks_are_split_with_overlap_and_preserved_unicode()
    {
        const string phrase = "Використання semantic retrieval improves evidence coverage. ";
        var text = string.Concat(Enumerable.Repeat(phrase, 35));
        var doc = Synthetic(text);
        var options = new ChunkingOptions { MaxEstimatedTokens = 45, OverlapEstimatedTokens = 9 };
        var chunks = _chunker.Chunk(doc, options);
        Assert.True(chunks.Count > 2);
        Assert.Equal(chunks.Count, chunks.Select(c => c.ChunkId).Distinct().Count());
        Assert.All(chunks, c => Assert.InRange(c.EstimatedTokens, 1, options.MaxEstimatedTokens));
        Assert.All(chunks, c => Assert.All(c.Sources, s =>
            Assert.Contains(doc.Resolve(s.SourceId).Text.Substring(s.StartOffset, s.Length),
                c.Text, StringComparison.Ordinal)));
        Assert.True(chunks[0].Sources.Last().StartOffset + chunks[0].Sources.Last().Length >
                    chunks[1].Sources[0].StartOffset);
    }

    [Fact]
    public void Chunking_respects_section_boundaries()
    {
        const string source = "First section content\nSecond section content";
        var doc = new ExtractedDocument("fixture", ProcessingFormat.Docx,
            [new DocumentSource("body", null, source, SourceExtractionStatus.Extracted)],
            [new DocumentBlock("b1", DocumentContentKind.Paragraph, "First section content", "First", "body", 0),
                new DocumentBlock("b2", DocumentContentKind.Paragraph, "Second section content", "Second", "body", 22)],
            ExtractionStatus.Complete, []);
        var chunks = _chunker.Chunk(doc, new ChunkingOptions
        {
            MaxEstimatedTokens = 100, OverlapEstimatedTokens = 10, SplitAtSectionBoundaries = true
        });
        Assert.Equal(2, chunks.Count);
        Assert.NotEqual(chunks[0].SectionPath, chunks[1].SectionPath);
    }

    [Fact]
    public async Task Rejects_unsupported_or_oversized_documents()
    {
        IDocumentProcessor processor = new DocumentProcessor([Pdf, Docx], _chunker);
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            processor.ProcessAsync("notes.txt", [1, 2, 3]));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            processor.ProcessAsync("report.pdf", new byte[32 * 1024 * 1024 + 1]));
    }

    [Fact]
    public void Rejects_invalid_chunk_overlap()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _chunker.Chunk(Synthetic("hello world"),
            new ChunkingOptions { MaxEstimatedTokens = 64, OverlapEstimatedTokens = 64 }));
    }

    private static ExtractedDocument Synthetic(string text) => new("fixture", ProcessingFormat.Docx,
        [new DocumentSource("body", null, text, SourceExtractionStatus.Extracted)],
        [new DocumentBlock("b1", DocumentContentKind.Paragraph, text, null, "body", 0)],
        ExtractionStatus.Complete, []);

    private static void AssertSourceMatches(ExtractedDocument doc, DocumentBlock block)
    {
        var source = doc.Resolve(block.SourceId);
        Assert.Equal(block.Text, source.Text.Substring(block.StartOffset, block.Length));
    }
}
