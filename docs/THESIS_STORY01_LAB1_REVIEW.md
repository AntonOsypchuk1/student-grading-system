# Story 01 — PDF boilerplate cleanup and chunking review

## Reproduction case

`lab1.pdf` (eight-page RouterOS laboratory report, already included as a fixture in PR #20) was tested with the extracted-document and chunk JSON from the initial implementation. The existing PR #20 test fixture provides a reproducible real-document case; synthetic repeated-footer PDF tests cover the same failure mode.

### Baseline observations

- 8 PDF page sources; 79 line-oriented blocks; 49 chunks using the test configuration (~36 estimated tokens/chunk).
- Repeated university stamp / author / reviewer / page labels dominated the extracted text on pages 2–7.
- Many tasks are represented mainly by screenshots; text-layer extraction does not read the screenshot contents.
- Page 5 contained only recurring footer text. Without a separate quality check, this was incorrectly considered usable text.
- Some words are physically hyphenated across PDF lines; they remain raw in `DocumentSource.Text` so source offsets and citations are not falsified.

### Base

PR #20, `back/01-document-processing` at `e4474b88f2237b0cdee1aaa4d1184249eec50312`. Apply the patch on top of that commit.

### Change

- Detect a footer from **multiple distinct recurring lines** across at least three pages and at least half the pages; remove footer blocks in the repeated tail region. Preserve all original `DocumentSource.Text` and offsets.
- Avoid treating all content as part of a single top-level title by recognizing numbered task headings. Keep generic `Хід роботи:` as an introductory paragraph to avoid an orphan section chunk.
- Mark footer-only pages as `EmptyOrUnreadable`, retaining a warning. This does **not** indicate an incorrect student answer.
- Split chunks at page and section boundaries by default, prefer ending at a completed block where feasible, and keep within the estimated token budget.

### Local JSON replay (not a .NET execution)

| Configuration | Chunks |
|---|---:|
| Original ~36 token max / 7 overlap | 49 |
| Proposed ~36 token max / 7 overlap | 26 |
| Proposed 128 max / 20 overlap | 11 |
| Proposed 512 max / 64 overlap | 8 |

For the 512/64 proposal, the replay yielded 22 kept blocks (down from 79), no footer text in chunks, and no chunks spanning two PDF pages. Page 5 is marked as no usable text. The original source strings remain unchanged.

### Limitations and next work

- These rules are conservative and may not remove all non-repeating title-block labels, especially in documents under three pages.
- Line-position and recurrence heuristics can still mistake a repeated short section near the end of many pages for page furniture. Validate on reports with different templates.
- The UTF-8 estimator is a rough token-count approximation; provider-specific tokenization is a separate concern.
- Screenshot/OCR and figure understanding are not implemented. Such evidence must not be deemed absent merely because no text-layer chunk exists.
- Legacy `ExtractTextAsync` returns the raw source text (for compatibility). The filtering affects structured `Blocks` and derived chunks only.

Run focused tests with:

```powershell
dotnet test SGS.ServicesTests/SGS.ServicesTests.csproj --filter "FullyQualifiedName~PdfBoilerplateAndChunkingTests"
```
