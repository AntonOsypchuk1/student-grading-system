namespace SGS.Application.Common.Options;

public sealed class TesseractOcrOptions
{
    public const long MaxImagePixels = 20_000_000;

    public string TessDataPath { get; set; } = "tessdata";
    public string? NativeLibraryDirectory { get; set; }
    public string Languages { get; set; } = "eng";
    // PSM 6 reads a contiguous block, including terminal output and tables.
    public int PageSegmentationMode { get; set; } = 6;
    public int ImageScaleFactor { get; set; } = 2;
    public bool ConvertToGrayscale { get; set; }

    public int GetEffectiveScaleFactor(int width, int height)
    {
        if (ImageScaleFactor is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(ImageScaleFactor));
        if (width <= 0 || height <= 0 || (long)width * height > MaxImagePixels)
            throw new ArgumentOutOfRangeException(nameof(width), "Invalid OCR image dimensions.");

        var pixels = (long)width * height;
        var factor = ImageScaleFactor;
        while (pixels * factor * factor > MaxImagePixels)
            factor--;
        return factor;
    }
}
