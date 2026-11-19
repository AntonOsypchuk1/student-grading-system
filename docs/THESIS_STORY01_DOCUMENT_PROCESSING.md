# Thesis Story 01 — Document extraction and structure-aware chunking

**Repository:** `AntonOsypchuk1/student-grading-system`
**Patch base:** `back/refactore-evaluation-pipeline` at `e3c99b6dfc170478faaea9a82980ff7645db5347` (2026-11-15 inspection)
**Scope:** document processing only. No change to the grading formula, gRPC contracts, or the published results.

## Reuse and implementation

- Existing `SGS.Application/Services/Document/Extraction/PdfDocumentTextExtractor` now produces page-aware data while preserving `IDocumentTextExtractor.ExtractTextAsync`, so the legacy `DocumentService.GetDocumentContentTextAsync` can still retrieve flat text.
- The existing iText dependency is retained. No second PDF parser is introduced.
- `DocxDocumentTextExtractor` adds Open XML headings, paragraphs, and tables, and implements the same flat-text and structured contracts. TXT support is unchanged.
- `IDocumentProcessor.ProcessAsync(fileName, bytes, options)` coordinates extraction and chunking. It is registered in `ApplicationModule` and can be called by subsequent retrieval/grading stories without changing the database or frontend.
- `StructureAwareChunker` preserves page/block/offset references, optionally splits at section boundaries, and accepts a replaceable token estimator. The built-in estimator is an approximation, not the token count for a particular LLM.

## Data contracts

`ExtractedDocument` includes a SHA-256 content identifier, `Sources`, `Blocks`, `Status`, and warnings. `DocumentSource.Text` is normalized canonical text. Every `DocumentBlock` and every `DocumentChunk.Sources` span references exact UTF-16 offsets in that text. Physical pages are represented **only for PDF**, never guessed for DOCX.

`ExtractionStatus` is:

- `Complete`: text was extracted with no detected unreadable pages or non-text images.
- `Partial`: there is text, but unreadable or non-text content is present.
- `Unreadable`: no extractable blocks. **Do not equate this with an incorrect student answer or a justified zero score.**

PDF vector diagrams and mathematical content may evade image detection, and native PDF tables are flattened to text lines. DOCX pages cannot be accurately numbered without rendering. OCR, equations, semantic retrieval, and evaluation against rubrics are deferred.

## Validation

New tests live in the existing `SGS.ServicesTests` project and use synthetic fixtures:

- Three-page PDF with an image-only page, and fully image-only PDF.
- DOCX with heading, paragraphs, and a table, and image-only DOCX.
- Legacy extraction compatibility, chunk stability, exact source offsets, overlap, section boundaries, invalid input, size limits.

Run from the SGS repo root:

```powershell
dotnet restore SGS.ServicesTests/SGS.ServicesTests.csproj
dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj
```

These tests have **not been executed** in the patch-building environment (no .NET SDK). The patch has been constructed against source files retrieved from the stated GitHub commit. Confirm compilation, runtime extraction, and regression tests locally before closing the story.

## Integration boundary

```csharp
var result = await documentProcessor.ProcessAsync(
    fileName,
    documentBytes,
    new ChunkingOptions { MaxEstimatedTokens = 512, OverlapEstimatedTokens = 64 },
    cancellationToken);

if (result.Document.Status != ExtractionStatus.Complete)
{
    // Surface missing visual/text evidence and require review as appropriate.
}

// Future Story 03: retrieve criterion-relevant chunks and pass verified evidence
// to SemanticAnalysis.Service.Evaluation.OpenAnswerEvaluationPipeline.
```

The existing upload endpoint currently accepts PDF only. Structured DOCX processing is available to application services, but enabling DOCX uploads and previews is separate integration work. `EvaluateDocument` continues to accept pre-extracted text.
