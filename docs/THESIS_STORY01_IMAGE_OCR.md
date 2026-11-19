# Document image extraction and OCR

This follow-up to Story 01 adds local image extraction and opt-in OCR. It does not
change the legacy flat-text upload path, evaluation RPC, grading logic, or database.
It does not interpret image semantics, diagrams, or correctness of work.

## Run locally

`SGS.Application` references [TesseractOCR 5.5.2](https://www.nuget.org/packages/TesseractOCR/5.5.2),
an in-process .NET wrapper for native Tesseract. It has no HTTP API, proxy,
subprocess or per-request charge. Recognition works offline. The package supplies
Windows x86/x64 native binaries; language data is installed separately.

On Windows, install the Microsoft Visual C++ 2015–2022 Redistributable for the
application architecture if required. Download `eng.traineddata` (and optionally
`ukr.traineddata`) from [tessdata_fast](https://github.com/tesseract-ocr/tessdata_fast)
into `SGS.Application/tessdata/` in the solution. The project automatically
copies the language files to `tessdata/` beside the application or test binaries
on build and publish. No `tesseract.exe` or PATH configuration is needed.
The default path already works; override it only when necessary:

```json
{
  "DocumentOcr": {
    "TessDataPath": "tessdata",
    "Languages": "eng",
    "PageSegmentationMode": 11
  }
}
```

Relative paths are resolved against `AppContext.BaseDirectory` (the deployed
application directory), not the working directory or source solution directory.
This works when the solution is moved, published or launched from another folder.
Absolute paths remain supported as an optional deployment override. On Linux
the package does not bundle native shared libraries. For example, on Debian 13
x64 install compatible libraries and language data, and provide the filenames
expected by the wrapper's loader:

```sh
sudo apt-get install libtesseract5 tesseract-ocr-eng tesseract-ocr-ukr
mkdir -p /opt/sgs/ocr-native/x64 # or use a writable directory
ln -s /lib/x86_64-linux-gnu/libtesseract.so.5 /opt/sgs/ocr-native/x64/libtesseract55.dll.so
ln -s /lib/x86_64-linux-gnu/libleptonica.so.6 /opt/sgs/ocr-native/x64/libleptonica-1.85.0.dll.so
```

Linux configuration (paths depend on distribution and architecture):

```json
{
  "DocumentOcr": {
    "TessDataPath": "tessdata",
    "NativeLibraryDirectory": "/opt/sgs/ocr-native",
    "Languages": "eng",
    "PageSegmentationMode": 11
  }
}
```

On Debian you may also copy the installed `eng.traineddata` and `ukr.traineddata`
into `SGS.Application/tessdata/` or override the default path to use the installed
system data directly. Use `eng+ukr` when both language files are available. Default `eng` suffices for
the English RouterOS UI in `lab1.pdf`. Use the same language configuration for
all documents in a controlled experiment. The default PSM 11 targets sparse text
in screenshots; it may misorder tables or omit small text. OCR is disabled for
ordinary `ProcessAsync` calls. Docker needs compatible libraries and traineddata
in its runtime image as well. Other OS/architecture combinations have not been
validated. The Linux loader path is process-wide.

The DI singleton lazily creates one engine and serializes recognition because an
engine is not safe for concurrent processing. Native recognition is synchronous;
cancellation is checked while waiting and before/after recognition, but does not
interrupt an active native call. Unlike the old CLI provider, there is no hard
per-image timeout or process crash isolation. Do not treat size limits as a sandbox.
If hard timeouts or stronger isolation become necessary, consider a local worker.

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
image. `MeanConfidence` is the engine's normalized [0, 1] confidence estimate
for recognized text, not a probability that a student's answer is correct.
Failures do not discard native text. Image-bearing documents remain Partial
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

Native Tesseract test (PowerShell, after placing the language files in
`SGS.Application/tessdata/`):

```powershell
$env:SGS_RUN_OCR_TESTS = '1'
dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj --filter FullyQualifiedName~DocumentImageProcessingTests
Remove-Item Env:SGS_RUN_OCR_TESTS
```

Linux x64 (using paths configured above):

```sh
SGS_RUN_OCR_TESTS=1 \
  SGS_OCR_NATIVE_LIBRARY_DIRECTORY=/opt/sgs/ocr-native OMP_THREAD_LIMIT=1 \
  dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj --filter FullyQualifiedName~DocumentImageProcessingTests
```

`SGS_OCR_TESSDATA_PATH` is optional and can point to a different directory when
running tests with externally managed language data. It defaults to `tessdata`.

Tests use the existing `lab1.pdf`, `image-only.pdf` and `image-only.docx` fixtures.
The test project references the matching iText Bouncy Castle adapter because the
existing boilerplate regression tests generate PDF files with `PdfWriter`.
The real-engine test checks evidence on previously text-empty page 5 and on page 8.
OCR text can contain mistakes and sensitive information (e.g. credentials shown in
screenshots). Do not log full OCR content or use it as proof of task completion.

## Research boundary

Initial CLI baseline on the committed `lab1.pdf` using .NET 9 and Tesseract 5.5.0,
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

Native-wrapper validation on Debian 13 x64 with .NET SDK 9.0.321,
TesseractOCR 5.5.2 and native Tesseract 5.5.0 preserved all counts above.
One run took 5.16 seconds (extraction, OCR and chunking), with mean per-image
OCR confidence of 70.3%. This is a smoke-test measurement, not a controlled
performance comparison or an accuracy score. All 37 active service tests
passed; two existing tests were skipped. Windows execution was not tested.

Compare text-only and text+OCR against identical human labels. Track image/page
coverage, OCR failures, grading error, runtime, and false evidence. Keep OCR and
vision-language interpretation separate; the latter will be added in the semantic
analysis service later and can reference original images by ID.
