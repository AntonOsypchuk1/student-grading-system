using System.Text.Json.Serialization;

namespace SGS.Application.Models.DocumentProcessing;

// PDF user-space coordinates (origin at bottom left, before page rotation).
// DOCX has no reliable physical coordinates without rendering.
public sealed record ImageBounds(float X, float Y, float Width, float Height);

public sealed record DocumentImage(
    string ImageId,
    string SourceId,
    int? PageNumber,
    string? SectionPath,
    string ContentType,
    int? PixelWidth,
    int? PixelHeight,
    ImageBounds? Bounds,
    [property: JsonIgnore] byte[] Bytes)
{
    // Retain decorative images for inspection but avoid OCR of likely PDF furniture.
    public bool IsLikelyDecorative { get; init; }
}

public enum OcrStatus { Recognized, NoText, Skipped, Failed }

public sealed record OcrRecognition(OcrStatus Status, string Text = "", string? Reason = null)
{
    public float? MeanConfidence { get; init; }
}

public sealed record ImageOcrResult(string ImageId, OcrStatus Status, string Text, string? Reason)
{
    public float? MeanConfidence { get; init; }
}
