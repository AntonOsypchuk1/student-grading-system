using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Interfaces;

public interface IDocumentImageExtractor
{
    ExtractedDocument ExtractImages(byte[] fileBytes, ExtractedDocument document,
        CancellationToken cancellationToken = default);
}

public interface IImageOcrProvider
{
    Task<OcrRecognition> RecognizeAsync(DocumentImage image, CancellationToken cancellationToken = default);
}
