using System.Text.Json;
using System.Text.Json.Serialization;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Experiments;

public enum ReferenceKind
{
    Unlabeled,
    Synthetic,
    Teacher
}

public enum WorkType
{
    Theoretical,
    Laboratory,
    Programming
}

public sealed record BenchmarkDataset(
    string Id,
    string Version,
    IReadOnlyList<BenchmarkCase> Cases);

public sealed record BenchmarkCase(
    string Id,
    WorkType WorkType,
    string Question,
    double MaxScore,
    IReadOnlyList<BenchmarkCriterion> Criteria,
    ReferenceGrade Reference,
    string? SubmissionFile = null,
    string? SubmissionText = null);

public sealed record BenchmarkCriterion(
    string Id,
    string Description,
    double MaxScore,
    double Weight,
    IReadOnlyList<BenchmarkConcept> Concepts);

public sealed record BenchmarkConcept(
    string Id,
    IReadOnlyList<string> Alternatives,
    bool Required = false);

public sealed record ReferenceGrade(
    ReferenceKind Kind,
    double? Score,
    string Note);

// Both the full, non-overlapping text and the original chunks are frozen for later retrieval experiments.
public sealed record PreparedDataset(
    BenchmarkDataset Dataset,
    string ManifestSha256,
    string ExtractionMode,
    string OcrLanguages,
    IReadOnlyList<PreparedCase> Submissions);

public sealed record PreparedCase(
    string CaseId,
    string SourceSha256,
    string Text,
    ExtractionStatus Status,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<DocumentSource> Sources,
    IReadOnlyList<DocumentChunk> Chunks);

public static class BenchmarkJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    public static readonly JsonSerializerOptions Lines = new(Options) { WriteIndented = false };
}
