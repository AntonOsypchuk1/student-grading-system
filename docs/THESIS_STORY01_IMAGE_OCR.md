# Document image extraction and OCR

This follow-up to Story 01 adds local image extraction and opt-in OCR. It does not
change the legacy flat-text upload path, evaluation RPC, grading logic, or database.
It does not interpret image semantics, diagrams, or correctness of work.

## Run locally

Install Tesseract 5 (the executable and language data), not an AI API. On Debian/Ubuntu:

```sh
sudo apt-get install tesseract-ocr tesseract-ocr-eng tesseract-ocr-ukr
tesseract --list-langs
```

On Windows, follow the [Tesseract installation instructions](https://tesseract-ocr.github.io/tessdoc/Installation.html)
and add the installation directory to PATH, or set `DocumentOcr:ExecutablePath` to
the installed `tesseract.exe`. Configure in the API using appsettings or environment variables:

```json
{
  "DocumentOcr": {
    "ExecutablePath": "tesseract",
    "Languages": "eng",
    "PageSegmentationMode": 11,
    "TimeoutSeconds": 30
  }
}
```

Use `eng+ukr` when both language files are installed. Default `eng` suffices for
the English RouterOS UI in `lab1.pdf`. Use the same language configuration for
all documents in a controlled experiment. The default PSM 11 targets sparse text
in screenshots; it may misorder tables or omit small text. OCR is disabled for
ordinary `ProcessAsync` calls. To run OCR in a Docker environment, install the
same packages in the runtime image before switching to a non-root user.

## API usage

```csharp
// IDocumentProcessor is registered in ApplicationModule.
var bytes = await File.ReadAllBytesAsync("lab1.pdf", cancellationToken);
var textOnly = await processor.ProcessAsync("lab1.pdf", bytes,
    cancellationToken: cancellationToken);
var enriched = await processor.ProcessWithOcrAsync("lab1.pdf", bytes,
    cancellationToken: cancellationToken);

// Both calls expose detected images. Only the second recognizes text.
foreach (var image in enriched.Document.Images)
{
    // Bytes are available in-process; omitted from JSON to avoid base64 bloat.
    Console.WriteLine($"{image.ImageId}: page {image.PageNumber}, {image.ContentType}");
}
```

Native text, source IDs and offsets are unchanged. Each successful OCR image
produces its own canonical source (`ocr-img-...`), with `ImageId` pointing to its
original image. The existing chunker includes those sources, preserving page and
section metadata. `OcrResults` records Recognized, NoText, Skipped or Failed per
image. Failures do not discard native text. Image-bearing documents remain Partial
even when OCR succeeds because OCR does not establish full visual coverage.

PDF uses rendered image events, including inline/nested images where supported by
iText, recording each occurrence and its user-space rectangle. Section association
is approximate: the last retained heading on the page, or the preceding section on
continuation pages. Mixed-section pages can be misassigned; precise geometry-aware
heading association is a future refinement. DOCX resolves embedded DrawingML/VML
body image references in document order, with no invented physical page numbers.
External URLs are never fetched. Vector graphics, composed page rendering, masks and
cropping are not fully reconstructed by raster extraction; OCR of scanned pages
works when they are embedded raster images. DOCX headers/footers are out of scope.

Small compressed masked PDF images (up to 1024 encoded bytes) are marked as likely
decorative and skipped by OCR, but remain accessible in `Images`. This is a heuristic,
not proof of irrelevance. Identical image bytes reuse OCR within each document,
while retaining individual occurrences and source references. Limits: 256 image
occurrences, 16 MiB per decoded image, 64 MiB retained total, 20 megapixels per PDF
image. These are processing bounds, not a complete sandbox for untrusted files.

## Tests

```sh
dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj
```

Native Tesseract test (PowerShell):

```powershell
$env:SGS_RUN_OCR_TESTS = '1'
dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj --filter FullyQualifiedName~DocumentImageProcessingTests
Remove-Item Env:SGS_RUN_OCR_TESTS
```

Linux/macOS:

```sh
SGS_RUN_OCR_TESTS=1 dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj --filter FullyQualifiedName~DocumentImageProcessingTests
```

Tests use the existing `lab1.pdf`, `image-only.pdf` and `image-only.docx` fixtures.
The test project references the matching iText Bouncy Castle adapter because the
existing boilerplate regression tests generate PDF files with `PdfWriter`.
The real-engine test checks evidence on previously text-empty page 5 and on page 8.
OCR text can contain mistakes and sensitive information (e.g. credentials shown in
screenshots). Do not log full OCR content or use it as proof of task completion.

## Research boundary

Initial local run on the committed `lab1.pdf` using .NET 9 and Tesseract 5.5.0,
English language data, PSM 11, default 512/64 estimated-token chunking:

| Metric | Result |
| --- | ---: |
| PDF pages | 8 |
| Detected raster occurrences | 114 |
| Skipped likely decorative images | 92 |
| Images with recognized text | 22 |
| OCR failures | 0 |
| OCR sources added | 22 |
| Native text-only chunks | 8 |
| Combined native and OCR chunks | 31 |

Page 5, which has no usable native text after footer cleanup, now contains
searchable console evidence. Page 8 contains OCR evidence of IP address commands.
These counts establish processing coverage, not OCR character accuracy or grading
accuracy; both need human-labeled evaluation.

Compare text-only and text+OCR against identical human labels. Track image/page
coverage, OCR failures, grading error, runtime, and false evidence. Keep OCR and
vision-language interpretation separate; the latter will be added in the semantic
analysis service later and can reference original images by ID.
