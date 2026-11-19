using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using SGS.Application.Common.Options;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Extraction;

namespace SGS.Application.Services.Document.Processing;

/// <summary>Local OCR via Tesseract 5 CLI. No network calls, shell or temporary files.</summary>
public sealed class TesseractImageOcrProvider(TesseractOcrOptions options) : IImageOcrProvider
{
    public async Task<OcrRecognition> RecognizeAsync(DocumentImage image,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (image.ContentType is not ("image/png" or "image/jpeg" or "image/tiff" or "image/bmp" or "image/gif" or "image/webp"))
            return new(OcrStatus.Skipped, Reason: $"Unsupported OCR image format: {image.ContentType}.");
        if (options.TimeoutSeconds <= 0 || options.PageSegmentationMode is < 0 or > 13 ||
            string.IsNullOrWhiteSpace(options.ExecutablePath) || string.IsNullOrWhiteSpace(options.Languages))
            throw new InvalidOperationException("Invalid DocumentOcr configuration.");

        var start = new ProcessStartInfo(options.ExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in new[] { "stdin", "stdout", "-l", options.Languages, "--psm",
                     options.PageSegmentationMode.ToString(CultureInfo.InvariantCulture) })
            start.ArgumentList.Add(argument);
        // Avoid Tesseract spawning many threads per concurrent document.
        start.Environment["OMP_THREAD_LIMIT"] = "1";

        using var process = new Process { StartInfo = start };
        try
        {
            if (!process.Start()) return new(OcrStatus.Failed, Reason: "Tesseract did not start.");
        }
        catch (Win32Exception)
        {
            return new(OcrStatus.Failed, Reason: "Tesseract executable is unavailable. Check installation and DocumentOcr:ExecutablePath.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
        var token = timeout.Token;
        // Drain both pipes concurrently so Tesseract cannot deadlock on stderr.
        var stdout = process.StandardOutput.ReadToEndAsync(token);
        var stderr = process.StandardError.ReadToEndAsync(token);
        try
        {
            await process.StandardInput.BaseStream.WriteAsync(image.Bytes.AsMemory(), token);
            process.StandardInput.Close();
            await process.WaitForExitAsync(token);
            var text = ExtractionTextUtilities.Normalize(await stdout).Trim();
            await stderr; // Do not log OCR content or arbitrary diagnostics from student files.

            if (process.ExitCode != 0)
                return new(OcrStatus.Failed, Reason: $"Tesseract exited with code {process.ExitCode}; check image and language data.");
            return string.IsNullOrWhiteSpace(text)
                ? new(OcrStatus.NoText)
                : new(OcrStatus.Recognized, text);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(OcrStatus.Failed, Reason: "Tesseract timed out.");
        }
        catch (IOException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(OcrStatus.Failed, Reason: "Tesseract could not read the image.");
        }
        finally
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { /* Process already exited. */ }
            }
            // Observe canceled/faulted pipe tasks before disposing the process.
            try { await Task.WhenAll(stdout, stderr); }
            catch (OperationCanceledException) { }
            catch (IOException) { }
        }
    }
}
