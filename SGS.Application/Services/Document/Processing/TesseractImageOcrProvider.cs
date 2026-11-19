using System.Runtime.InteropServices;
using SGS.Application.Common.Options;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Extraction;
using TesseractOCR;
using TesseractOCR.Enums;
using TesseractOCR.InteropDotNet;
using OcrImage = TesseractOCR.Pix.Image;

namespace SGS.Application.Services.Document.Processing;

/// <summary>In-process native Tesseract OCR; the engine is reused and serialized.</summary>
public sealed class TesseractImageOcrProvider(TesseractOcrOptions options) : IImageOcrProvider, IDisposable
{
    private static readonly object NativeConfigurationLock = new();
    private static bool _linuxConfigured;
    private static string? _nativeDirectory;
    private readonly SemaphoreSlim _gate = new(1);
    private Engine? _engine;
    private bool _disposed;

    public async Task<OcrRecognition> RecognizeAsync(DocumentImage image,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (image.ContentType is not ("image/png" or "image/jpeg" or "image/tiff" or "image/bmp" or "image/gif" or "image/webp"))
            return new(OcrStatus.Skipped, Reason: $"Unsupported OCR image format: {image.ContentType}.");
        if (options.PageSegmentationMode is < 0 or > 13 || string.IsNullOrWhiteSpace(options.Languages)
            || string.IsNullOrWhiteSpace(options.TessDataPath))
            throw new InvalidOperationException("Invalid DocumentOcr configuration.");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            var dataPath = Path.GetFullPath(options.TessDataPath, AppContext.BaseDirectory);
            if (!Directory.Exists(dataPath))
                return new(OcrStatus.Failed, Reason: "Tesseract language data directory is missing. Check DocumentOcr:TessDataPath.");

            try
            {
                ConfigureNativeLibraries();
                _engine ??= new Engine(dataPath, options.Languages, EngineMode.LstmOnly);
                using var pix = OcrImage.LoadFromMemory(image.Bytes);
                if ((long)pix.Width * pix.Height > 20_000_000)
                    return new(OcrStatus.Skipped, Reason: "Image exceeds OCR pixel limit.");
                using var page = _engine.Process(pix, (PageSegMode)options.PageSegmentationMode);
                var text = ExtractionTextUtilities.Normalize(page.Text).Trim();
                cancellationToken.ThrowIfCancellationRequested();
                return string.IsNullOrWhiteSpace(text)
                    ? new(OcrStatus.NoText)
                    : new(OcrStatus.Recognized, text) { MeanConfidence = page.MeanConfidence };
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or OutOfMemoryException))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Do not include student data or native diagnostic text in warnings.
                return new(OcrStatus.Failed, Reason: $"Native Tesseract OCR failed ({ex.GetType().Name}). Check native libraries, language data and image format.");
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private void ConfigureNativeLibraries()
    {
        if (!OperatingSystem.IsLinux()) return;
        lock (NativeConfigurationLock)
        {
            var directory = string.IsNullOrWhiteSpace(options.NativeLibraryDirectory)
                ? AppContext.BaseDirectory
                : Path.GetFullPath(options.NativeLibraryDirectory, AppContext.BaseDirectory);
            if (_linuxConfigured)
            {
                if (_nativeDirectory != directory)
                    throw new InvalidOperationException("The native library path is process-wide.");
                return;
            }
            // The wrapper expects versioned aliases under the architecture directory.
            LibraryLoader.Instance.CustomSearchPath = directory;
            NativeLibrary.SetDllImportResolver(typeof(Engine).Assembly, (name, assembly, searchPath) =>
                name == "libdl" && NativeLibrary.TryLoad("libdl.so.2", assembly, searchPath, out var handle)
                    ? handle : IntPtr.Zero);
            _nativeDirectory = directory;
            _linuxConfigured = true;
        }
    }

    public void Dispose()
    {
        _gate.Wait();
        try
        {
            if (_disposed) return;
            _disposed = true;
            _engine?.Dispose();
        }
        finally
        {
            _gate.Release();
        }
    }
}
