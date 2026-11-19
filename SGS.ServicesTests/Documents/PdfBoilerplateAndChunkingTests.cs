using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using Microsoft.Extensions.Logging.Abstractions;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Extraction;
using SGS.Application.Services.Document.Processing;

namespace SGS.ServicesTests.Documents;

public sealed class PdfBoilerplateAndChunkingTests
{
    [Fact]
    public async Task Recurring_footer_is_not_indexed_and_footer_only_page_is_unreadable()
    {
        byte[] pdf = GenerateReport();
        var extractor = new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance);
        var result = await extractor.ExtractStructuredAsync(pdf);

        Assert.Equal(4, result.Sources.Count);
        Assert.All(result.Sources, source =>
            Assert.Contains("Footer reviewer", source.Text, StringComparison.Ordinal));
        Assert.DoesNotContain(result.Blocks, block =>
            block.Text.Contains("Footer reviewer", StringComparison.Ordinal) ||
            block.Text.Contains("Footer university", StringComparison.Ordinal));
        Assert.Equal(SourceExtractionStatus.EmptyOrUnreadable, result.Sources[2].Status);
        Assert.Contains(result.Warnings, warning => warning.Contains("page 3", StringComparison.Ordinal));
        Assert.Contains(result.Blocks, block => block.Text.Contains("Task 1", StringComparison.Ordinal));

        foreach (var block in result.Blocks)
        {
            var source = result.Resolve(block.SourceId);
            Assert.Equal(block.Text, source.Text.Substring(block.StartOffset, block.Length));
        }
    }

    [Fact]
    public async Task Chunks_keep_page_and_task_boundaries_and_retain_source_offsets()
    {
        var extractor = new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance);
        var doc = await extractor.ExtractStructuredAsync(GenerateReport());
        var chunks = new StructureAwareChunker().Chunk(doc, new ChunkingOptions
        {
            MaxEstimatedTokens = 512,
            OverlapEstimatedTokens = 64
        });

        Assert.NotEmpty(chunks);
        Assert.DoesNotContain(chunks, chunk => chunk.Text.Contains("Footer reviewer", StringComparison.Ordinal));
        Assert.All(chunks, chunk =>
        {
            Assert.Single(chunk.Sources.Select(s => s.PageNumber).Distinct());
            Assert.InRange(chunk.EstimatedTokens, 1, 512);
            foreach (var span in chunk.Sources)
            {
                string piece = doc.Resolve(span.SourceId).Text.Substring(span.StartOffset, span.Length);
                Assert.Contains(piece, chunk.Text, StringComparison.Ordinal);
            }
        });
        Assert.Contains(chunks, chunk => chunk.SectionPath?.StartsWith("Task 2", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task Lab1_fixture_has_no_page_stamp_in_chunks_and_retains_task_sections()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Documents", "Fixtures", "lab1.pdf");
        var pdf = await File.ReadAllBytesAsync(path);
        var extractor = new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance);
        var doc = await extractor.ExtractStructuredAsync(pdf);
        var chunks = new StructureAwareChunker().Chunk(doc);

        Assert.Equal(8, doc.Sources.Count);
        Assert.Equal(SourceExtractionStatus.EmptyOrUnreadable, doc.Sources[4].Status);
        Assert.DoesNotContain(doc.Blocks, block =>
            block.Text.Contains("Змн. Арк.", StringComparison.Ordinal) ||
            block.Text.Contains("ДУ «Житомирська політехніка».26", StringComparison.Ordinal));
        Assert.Contains(doc.Blocks, block =>
            block.Kind == DocumentContentKind.Heading &&
            block.Text.StartsWith("Завдання 7.", StringComparison.Ordinal));
        Assert.All(chunks, chunk =>
        {
            Assert.Single(chunk.Sources.Select(s => s.PageNumber).Distinct());
            Assert.InRange(chunk.EstimatedTokens, 1, 512);
        });
    }

    private static byte[] GenerateReport()
    {
        using var stream = new MemoryStream();
        using (var pdf = new PdfDocument(new PdfWriter(stream)))
        {
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            for (int pageNumber = 1; pageNumber <= 4; pageNumber++)
            {
                var page = pdf.AddNewPage();
                var canvas = new PdfCanvas(page);
                if (pageNumber != 3)
                {
                    Draw(canvas, font, 700, $"Task {pageNumber}. Configure RouterOS");
                    Draw(canvas, font, 680, $"Distinct evidence from exercise {pageNumber} is recorded here.");
                }
                Draw(canvas, font, 75, "Footer university document identifier");
                Draw(canvas, font, 55, "Footer reviewer signature");
                Draw(canvas, font, 35, "Footer reviewer");
            }
        }
        return stream.ToArray();
    }

    private static void Draw(PdfCanvas canvas, PdfFont font, float y, string text) =>
        canvas.BeginText().SetFontAndSize(font, 11).MoveText(45, y)
            .ShowText(text).EndText();
}
