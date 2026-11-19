using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using ProcessingFormat = SGS.Application.Models.DocumentProcessing.DocumentFormat;
using VmlImageData = DocumentFormat.OpenXml.Vml.ImageData;
using WParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace SGS.Application.Services.Document.Extraction;

/// <summary>Extracts rendered PDF raster images and embedded DOCX body pictures locally.</summary>
public sealed class DocumentImageExtractor : IDocumentImageExtractor
{
    private const int MaxImages = 256;
    private const int MaxImageBytes = 16 * 1024 * 1024;
    private const long MaxTotalBytes = 64L * 1024 * 1024;
    private const long MaxPixels = 20_000_000;

    public ExtractedDocument ExtractImages(byte[] fileBytes, ExtractedDocument document,
        CancellationToken cancellationToken = default)
    {
        var collector = new ImageCollector(document.Warnings);
        using var stream = new MemoryStream(fileBytes, writable: false);
        switch (document.Format)
        {
            case ProcessingFormat.Pdf:
                using (var pdf = new PdfDocument(new PdfReader(stream)))
                {
                    string? section = null;
                    for (int page = 1; page <= pdf.GetNumberOfPages(); page++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var source = document.Sources.Single(s => s.PageNumber == page);
                        // Page-level association, including continuation/image-only pages.
                        // Exact nearest-heading assignment needs text bounding boxes.
                        section = document.Blocks.LastOrDefault(b => b.SourceId == source.SourceId
                            && b.SectionPath is not null)?.SectionPath ?? section;
                        var listener = new PdfImageListener(collector, source, section, cancellationToken);
                        try { new PdfCanvasProcessor(listener).ProcessPageContent(pdf.GetPage(page)); }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex) when (ex is not OutOfMemoryException)
                        {
                            collector.Warn($"PDF page {page}: image detection was incomplete ({ex.GetType().Name}).");
                        }
                    }
                }
                break;
            case ProcessingFormat.Docx:
                using (var word = WordprocessingDocument.Open(stream, false))
                {
                    ExtractDocx(word, document, collector, cancellationToken);
                }
                break;
            default:
                throw new NotSupportedException($"Image extraction is unavailable for {document.Format}.");
        }

        var sources = document.Sources.Select(s => collector.ObservedSourceIds.Contains(s.SourceId)
            ? s with { HasNonTextContent = true } : s).ToArray();
        return document with
        {
            Images = collector.Images.ToArray(),
            Sources = sources,
            Warnings = collector.Warnings.ToArray(),
            Status = document.Status == ExtractionStatus.Complete &&
                     (collector.ObservedSourceIds.Count > 0 || collector.Incomplete)
                ? ExtractionStatus.Partial : document.Status
        };
    }

    private static void ExtractDocx(WordprocessingDocument word, ExtractedDocument document,
        ImageCollector collector, CancellationToken cancellationToken)
    {
        var main = word.MainDocumentPart;
        var body = main?.Document?.Body;
        if (main is null || body is null) return;
        string? section = null;
        var headings = document.Blocks.Where(b => b.Kind == DocumentContentKind.Heading)
            .Select(b => b.Text).ToHashSet(StringComparer.Ordinal);
        foreach (var element in body.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (element is WParagraph paragraph)
            {
                var text = ExtractionTextUtilities.Normalize(paragraph.InnerText).Trim();
                if (headings.Contains(text)) section = text;
            }

            foreach (var child in element.Descendants())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relationship = child switch
                {
                    Blip blip => blip.Embed?.Value,
                    VmlImageData vml => vml.RelationshipId?.Value,
                    _ => null
                };
                if (string.IsNullOrEmpty(relationship)) continue; // never fetch external images
                collector.ObservedSourceIds.Add("docx-body");
                if (!main.TryGetPartById(relationship, out var part) || part is not ImagePart image)
                {
                    collector.Warn("DOCX contains an unresolved embedded image relationship.");
                    continue;
                }
                if (!collector.CanAdd()) continue;
                using var input = image.GetStream(FileMode.Open, FileAccess.Read);
                using var output = new MemoryStream();
                var buffer = new byte[81920];
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (output.Length + read > MaxImageBytes) break;
                    output.Write(buffer, 0, read);
                }
                if (read != 0)
                {
                    collector.Warn("An embedded DOCX image exceeded the image size limit.");
                    continue;
                }
                collector.Add("docx-body", null, section, image.ContentType,
                    null, null, null, output.ToArray(), false);
            }
        }
    }

    private sealed class PdfImageListener(ImageCollector collector, DocumentSource source,
        string? section, CancellationToken cancellationToken) : IEventListener
    {
        public ICollection<EventType> GetSupportedEvents() => [EventType.RENDER_IMAGE];

        public void EventOccurred(IEventData data, EventType type)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (data is not ImageRenderInfo info) return;
            collector.ObservedSourceIds.Add(source.SourceId);
            if (!collector.CanAdd()) return;
            try
            {
                var image = info.GetImage();
                int width = (int)image.GetWidth(), height = (int)image.GetHeight();
                if (width <= 0 || height <= 0 || (long)width * height > MaxPixels)
                {
                    collector.Warn($"PDF page {source.PageNumber}: image exceeded the pixel limit or had invalid dimensions.");
                    return;
                }

                var dictionary = image.GetPdfObject();
                // In lab reports, many tiny masked bitmaps are generated by the PDF
                // producer for page furniture. Keep their metadata, skip OCR by default.
                bool decorative = dictionary.ContainsKey(PdfName.SMask) &&
                    dictionary.GetAsNumber(PdfName.Length)?.IntValue() is <= 1024;
                var extension = image.IdentifyImageFileExtension().ToLowerInvariant();
                string mimeType = extension switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    "tif" or "tiff" => "image/tiff",
                    "jp2" => "image/jp2",
                    "jbig2" => "image/jbig2",
                    _ => "application/octet-stream"
                };
                collector.Add(source.SourceId, source.PageNumber, section, mimeType,
                    width, height, Bounds(info.GetImageCtm()), image.GetImageBytes(), decorative);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                collector.Warn($"PDF page {source.PageNumber}: an image could not be decoded ({ex.GetType().Name}).");
            }
        }

        private static ImageBounds Bounds(Matrix m)
        {
            float x = m.Get(Matrix.I31), y = m.Get(Matrix.I32);
            float a = m.Get(Matrix.I11), b = m.Get(Matrix.I12);
            float c = m.Get(Matrix.I21), d = m.Get(Matrix.I22);
            float[] xs = [x, x + a, x + c, x + a + c];
            float[] ys = [y, y + b, y + d, y + b + d];
            return new ImageBounds(xs.Min(), ys.Min(), xs.Max() - xs.Min(), ys.Max() - ys.Min());
        }
    }

    private sealed class ImageCollector(IReadOnlyList<string> warnings)
    {
        private long _totalBytes;
        public List<DocumentImage> Images { get; } = [];
        public List<string> Warnings { get; } = [.. warnings];
        public HashSet<string> ObservedSourceIds { get; } = new(StringComparer.Ordinal);
        public bool Incomplete { get; private set; }

        public void Warn(string warning)
        {
            Incomplete = true;
            if (!Warnings.Contains(warning)) Warnings.Add(warning);
        }

        public bool CanAdd()
        {
            if (Images.Count < MaxImages && _totalBytes < MaxTotalBytes) return true;
            Warn("Image extraction limit reached; remaining images were not extracted.");
            return false;
        }

        public void Add(string sourceId, int? page, string? section, string mimeType,
            int? width, int? height, ImageBounds? bounds, byte[] bytes, bool decorative)
        {
            if (!CanAdd()) return;
            if (bytes.Length == 0 || bytes.Length > MaxImageBytes || _totalBytes + bytes.Length > MaxTotalBytes)
            {
                Warn("An embedded image was empty or exceeded the image byte limit.");
                return;
            }
            Images.Add(new DocumentImage($"img-{Images.Count + 1:D6}", sourceId, page,
                section, mimeType, width, height, bounds, bytes) { IsLikelyDecorative = decorative });
            _totalBytes += bytes.Length;
        }
    }
}
