namespace SGS.Application.Common.Options;

public sealed class TesseractOcrOptions
{
    public string TessDataPath { get; set; } = "tessdata";
    public string? NativeLibraryDirectory { get; set; }
    public string Languages { get; set; } = "eng";
    // Sparse text mode is useful for terminal and application screenshots.
    public int PageSegmentationMode { get; set; } = 11;
}
