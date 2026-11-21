using Microsoft.Extensions.Logging.Abstractions;
using SGS.Application.Common.Options;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Extraction;
using SGS.Application.Services.Document.Processing;
using Xunit.Abstractions;

namespace SGS.ServicesTests.Documents;

public sealed class DocumentImageProcessingTests(ITestOutputHelper output)
{
    private static DocumentProcessor Processor(IImageOcrProvider? ocr = null) => new(
        [new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance),
            new DocxDocumentTextExtractor()], new StructureAwareChunker(),
        new DocumentImageExtractor(), ocr);

    private static Task<byte[]> Fixture(string name) => File.ReadAllBytesAsync(
        Path.Combine(AppContext.BaseDirectory, "Documents", "Fixtures", name));

    [Fact]
    public async Task Lab1_detects_images_without_calling_ocr_or_changing_native_chunks()
    {
        var bytes = await Fixture("lab1.pdf");
        var ocr = new FakeOcr(new(OcrStatus.Recognized, "Not used"));
        var result = await Processor(ocr).ProcessAsync("lab1.pdf", bytes);
        output.WriteLine($"Text-only chunks: {result.Chunks.Count}.");
        var native = await new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance)
            .ExtractStructuredAsync(bytes);
        Assert.Equal(new StructureAwareChunker().Chunk(native).Select(c => c.ChunkId),
            result.Chunks.Select(c => c.ChunkId));
        Assert.Equal(0, ocr.Calls);
        Assert.Empty(result.Document.OcrResults);
        Assert.Equal(8, result.Document.Images.Select(i => i.PageNumber).Distinct().Count());
        Assert.Contains(result.Document.Images, i => i.PageNumber == 5 && !i.IsLikelyDecorative);
        Assert.Contains(result.Document.Images, i => i.IsLikelyDecorative);
        Assert.All(result.Document.Images, i =>
        {
            Assert.NotEmpty(i.Bytes);
            Assert.NotNull(i.Bounds);
            Assert.Equal(i.PageNumber, result.Document.Resolve(i.SourceId).PageNumber);
        });
        Assert.Equal(result.Document.Images.Count, result.Document.Images.Select(i => i.ImageId).Distinct().Count());
    }

    [Theory]
    [InlineData("image-only.pdf", 1)]
    [InlineData("image-only.docx", null)]
    public async Task Image_only_documents_gain_searchable_ocr_sources_without_rewriting_native_text(
        string fileName, int? page)
    {
        var ocr = new FakeOcr(new(OcrStatus.Recognized, "Visible evidence\nRouterOS console"));
        var result = await Processor(ocr).ProcessWithOcrAsync(fileName, await Fixture(fileName));
        var image = Assert.Single(result.Document.Images);
        Assert.Equal(page, image.PageNumber);
        Assert.Equal(1, ocr.Calls);
        Assert.Equal(ExtractionStatus.Partial, result.Document.Status);
        Assert.Equal(OcrStatus.Recognized, Assert.Single(result.Document.OcrResults).Status);
        var source = Assert.Single(result.Document.Sources, s => s.ImageId is not null);
        Assert.Equal(image.ImageId, source.ImageId);
        Assert.Equal(SourceExtractionStatus.EmptyOrUnreadable, result.Document.Resolve(image.SourceId).Status);
        Assert.Equal(DocumentContentKind.OcrText, Assert.Single(result.Document.Blocks).Kind);
        Assert.All(result.Chunks, chunk => Assert.All(chunk.Sources, span =>
        {
            Assert.Equal(page, span.PageNumber);
            var canonical = result.Document.Resolve(span.SourceId);
            Assert.Equal(image.ImageId, canonical.ImageId);
            Assert.Contains(canonical.Text.Substring(span.StartOffset, span.Length), chunk.Text);
        }));
        Assert.NotEmpty(result.Chunks);
    }

    [Fact]
    public async Task Missing_ocr_language_data_is_reported_without_losing_native_text()
    {
        using var provider = new TesseractImageOcrProvider(new TesseractOcrOptions
        {
            TessDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "tessdata")
        });
        var bytes = await Fixture("lab1.pdf");
        using var processor = Processor(provider);
        var baseline = await processor.ProcessAsync("lab1.pdf", bytes);
        var result = await processor.ProcessWithOcrAsync("lab1.pdf", bytes);
        Assert.Contains(result.Document.OcrResults, r => r.Status == OcrStatus.Failed);
        Assert.Contains(result.Document.Warnings, w => w.Contains("OCR failed", StringComparison.Ordinal));
        Assert.Equal(baseline.Chunks.Select(c => c.ChunkId), result.Chunks.Select(c => c.ChunkId));
        Assert.DoesNotContain(result.Document.Sources, s => s.ImageId is not null);
    }

    [Fact]
    public async Task Empty_ocr_does_not_produce_fake_evidence()
    {
        var result = await Processor(new FakeOcr(new(OcrStatus.Recognized, " \n ")))
            .ProcessWithOcrAsync("image-only.pdf", await Fixture("image-only.pdf"));
        Assert.Equal(OcrStatus.NoText, Assert.Single(result.Document.OcrResults).Status);
        Assert.Equal(ExtractionStatus.Unreadable, result.Document.Status);
        Assert.Empty(result.Chunks);
    }

    [Fact]
    public async Task Cancellation_is_propagated()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Processor()
            .ProcessWithOcrAsync("lab1.pdf", [1], cancellationToken: cancellation.Token));
    }

    [Theory]
    [InlineData(1000, 800, 3, 3)]
    [InlineData(2500, 2000, 3, 2)]
    [InlineData(4000, 3000, 2, 1)]
    [InlineData(1000, 800, 1, 1)]
    public void Upscaling_respects_pixel_budget(int width, int height, int requested, int expected)
    {
        var options = new TesseractOcrOptions { ImageScaleFactor = requested };
        Assert.Equal(expected, options.GetEffectiveScaleFactor(width, height));
        Assert.True((long)width * height * expected * expected <= TesseractOcrOptions.MaxImagePixels);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Invalid_upscaling_factor_is_rejected(int factor)
    {
        var options = new TesseractOcrOptions { ImageScaleFactor = factor };
        Assert.Throws<ArgumentOutOfRangeException>(() => options.GetEffectiveScaleFactor(100, 100));
    }

    [Fact]
    public async Task Lab1_real_tesseract_recovers_console_and_ip_configuration_evidence()
    {
        using var provider = new TesseractImageOcrProvider(new TesseractOcrOptions
        {
            TessDataPath = Environment.GetEnvironmentVariable("SGS_OCR_TESSDATA_PATH") ?? "tessdata",
            NativeLibraryDirectory = Environment.GetEnvironmentVariable("SGS_OCR_NATIVE_LIBRARY_DIRECTORY")
        });
        using var processor = Processor(provider);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await processor.ProcessWithOcrAsync("lab1.pdf", await Fixture("lab1.pdf"));
        stopwatch.Stop();
        var document = result.Document;
        output.WriteLine($"Detected images: {document.Images.Count}; " +
            $"OCR recognized: {document.OcrResults.Count(r => r.Status == OcrStatus.Recognized)}; " +
            $"skipped: {document.OcrResults.Count(r => r.Status == OcrStatus.Skipped)}; " +
            $"no text: {document.OcrResults.Count(r => r.Status == OcrStatus.NoText)}; " +
            $"OCR sources: {document.Sources.Count(s => s.ImageId is not null)}; " +
            $"chunks: {result.Chunks.Count}; elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s.");
        var recognized = document.OcrResults.Where(r => r.Status == OcrStatus.Recognized).ToArray();
        Assert.NotEmpty(recognized);
        Assert.All(recognized, r => Assert.InRange(Assert.IsType<float>(r.MeanConfidence), 0f, 1f));
        output.WriteLine($"Mean image OCR confidence: {recognized.Average(r => r.MeanConfidence):P1}.");
        Assert.DoesNotContain(document.OcrResults, r => r.Status == OcrStatus.Failed);
        Assert.Contains(document.OcrResults, r => r.Status == OcrStatus.Skipped);
        Assert.Contains(document.Sources, s => s.ImageId is not null && s.PageNumber == 5 &&
            s.Text.Contains("console", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(document.Sources, s => s.ImageId is not null && s.PageNumber == 8 &&
            s.Text.Contains("ip address", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Chunks, c => c.Sources.Any(s => document.Resolve(s.SourceId).ImageId is not null));
        Assert.All(result.Chunks, c => Assert.All(c.Sources, s =>
            Assert.Contains(document.Resolve(s.SourceId).Text.Substring(s.StartOffset, s.Length), c.Text)));
        Assert.Equal(ExtractionStatus.Partial, document.Status);
    }

    private sealed class FakeOcr(OcrRecognition result) : IImageOcrProvider
    {
        public int Calls { get; private set; }

        public Task<OcrRecognition> RecognizeAsync(DocumentImage image, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Calls++;
            return Task.FromResult(result);
        }
    }
}

// Standard test runs remain portable. Explicitly enable the native-engine test.
public sealed class OcrIntegrationFactAttribute : FactAttribute
{
    public OcrIntegrationFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("SGS_RUN_OCR_TESTS") != "1")
            Skip = "Set SGS_RUN_OCR_TESTS=1 with native libraries and English traineddata configured.";
    }
}
