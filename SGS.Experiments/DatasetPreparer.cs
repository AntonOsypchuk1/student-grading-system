using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Processing;
using ProcessingFormat = SGS.Application.Models.DocumentProcessing.DocumentFormat;

namespace SGS.Experiments;

public static class DatasetPreparer
{
    public static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static void Validate(BenchmarkDataset dataset)
    {
        if (string.IsNullOrWhiteSpace(dataset.Id) || string.IsNullOrWhiteSpace(dataset.Version) ||
            dataset.Cases is not { Count: > 0 } || dataset.Cases.Any(c => c is null) ||
            dataset.Cases.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != dataset.Cases.Count)
        {
            throw new InvalidDataException("Dataset needs an ID, version and unique case IDs.");
        }

        foreach (var c in dataset.Cases)
        {
            if (string.IsNullOrWhiteSpace(c.Id) || string.IsNullOrWhiteSpace(c.Question) || c.Question.Length > 10_000 ||
                !Positive(c.MaxScore) || !Enum.IsDefined(c.WorkType) ||
                (string.IsNullOrWhiteSpace(c.SubmissionFile) == string.IsNullOrWhiteSpace(c.SubmissionText)))
            {
                throw new InvalidDataException(
                    $"Case {c.Id}: provide a question, scale and exactly one submission file or text.");
            }
            
            var reference = c.Reference;
            if (reference is null || !Enum.IsDefined(reference.Kind) || string.IsNullOrWhiteSpace(reference.Note) ||
                (reference.Kind == ReferenceKind.Unlabeled
                    ? reference.Score is not null
                    : reference.Score is not double score || !double.IsFinite(score) || score < 0 || score > c.MaxScore))
            {
                throw new InvalidDataException($"Case {c.Id}: invalid reference grade or provenance.");
            }
            
            if (c.Criteria is not { Count: >= 1 and <= 50 } || c.Criteria.Any(x => x is null) ||
                c.Criteria.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != c.Criteria.Count ||
                !double.IsFinite(c.Criteria.Sum(x => x.Weight)))
            {
                throw new InvalidDataException($"Case {c.Id}: provide 1–50 unique criteria.");
            }
            
            foreach (var criterion in c.Criteria)
            {
                if (string.IsNullOrWhiteSpace(criterion.Id) || string.IsNullOrWhiteSpace(criterion.Description) ||
                    !Positive(criterion.MaxScore) || !Positive(criterion.Weight) ||
                    criterion.Concepts is not { Count: >= 1 and <= 100 } || criterion.Concepts.Any(x => x is null) ||
                    criterion.Concepts.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != criterion.Concepts.Count ||
                    criterion.Concepts.Any(x => string.IsNullOrWhiteSpace(x.Id) || x.Alternatives is not { Count: >= 1 and <= 20 } ||
                        x.Alternatives.Any(string.IsNullOrWhiteSpace)))
                {
                    throw new InvalidDataException($"Case {c.Id}: invalid criterion or concept definition.");
                }
            }
        }
    }

    public static async Task<PreparedDataset> PrepareAsync(string manifestPath, IDocumentProcessor processor,
        string extractionMode, string ocrLanguages, CancellationToken cancellationToken = default,
        ChunkingOptions? chunking = null, int? ocrPageSegmentationMode = null)
    {
        if (extractionMode is not ("native" or "ocr"))
        {
            throw new ArgumentException("Mode must be native or ocr.");
        }
        
        chunking ??= new ChunkingOptions();
        chunking.Validate();
        var manifest = await File.ReadAllBytesAsync(manifestPath, cancellationToken);
        var dataset = JsonSerializer.Deserialize<BenchmarkDataset>(manifest, BenchmarkJson.Options)
            ?? throw new InvalidDataException("Dataset is empty.");
        
        Validate(dataset);
        var directory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var submissions = new List<PreparedCase>();
        foreach (var c in dataset.Cases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(c.SubmissionText))
            {
                submissions.Add(FromText(c.Id, c.SubmissionText, Sha256(Encoding.UTF8.GetBytes(c.SubmissionText)), chunking));
                continue;
            }
            
            var path = Path.GetFullPath(c.SubmissionFile!, directory);
            if (new FileInfo(path).Length > 32 * 1024 * 1024)
            {
                throw new InvalidDataException($"Case {c.Id}: source exceeds 32 MiB.");
            }
            
            byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            string hash = Sha256(bytes);
            if (Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                submissions.Add(FromText(c.Id, new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF'), hash, chunking));
                continue;
            }
            
            if (Path.GetExtension(path).ToLowerInvariant() is not (".pdf" or ".docx"))
            {
                throw new InvalidDataException($"Case {c.Id}: only PDF, DOCX and UTF-8 TXT are supported.");
            }
            
            try
            {
                var result = extractionMode == "ocr"
                    ? await processor.ProcessWithOcrAsync(path, bytes, chunking, cancellationToken)
                    : await processor.ProcessAsync(path, bytes, chunking, cancellationToken);
                var document = result.Document;
                // Use canonical blocks, not concatenated overlapping chunks.
                submissions.Add(new(c.Id, hash, string.Join("\n\n", document.Blocks.Select(b => b.Text)),
                    document.Status, document.Warnings, document.Sources, result.Chunks));
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or OutOfMemoryException))
            {
                submissions.Add(new(c.Id, hash, "", ExtractionStatus.Unreadable,
                    [$"Extraction failed ({ex.GetType().Name})."], [], []));
            }
        }
        return new(dataset, Sha256(manifest), extractionMode, ocrLanguages, submissions, chunking,
            extractionMode == "ocr" ? ocrPageSegmentationMode : null);
    }

    private static PreparedCase FromText(string id, string text, string hash, ChunkingOptions chunking)
    {
        text = text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return new(id, hash, text, ExtractionStatus.Unreadable, [], [], []);
        }
        
        var source = new DocumentSource("text", null, text, SourceExtractionStatus.Extracted);
        var block = new DocumentBlock("b-000001", DocumentContentKind.Paragraph, text, null, source.SourceId, 0);
        var document = new ExtractedDocument(hash, ProcessingFormat.Text, [source], [block], ExtractionStatus.Complete, []);
        return new(id, hash, text, document.Status, [], document.Sources, new StructureAwareChunker().Chunk(document, chunking));
    }

    private static bool Positive(double value) => double.IsFinite(value) && value > 0;
}
