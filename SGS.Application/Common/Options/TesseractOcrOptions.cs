namespace SGS.Application.Common.Options;

public sealed class TesseractOcrOptions
{
    public string ExecutablePath { get; set; } = "tesseract";
    public string Languages { get; set; } = "eng";
    // Sparse text mode is useful for terminal and application screenshots.
    public int PageSegmentationMode { get; set; } = 11;
    public int TimeoutSeconds { get; set; } = 30;
}
