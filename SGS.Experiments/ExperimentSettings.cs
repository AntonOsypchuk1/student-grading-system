using SGS.Application.Common.Options;
using SGS.Application.Services.Document.Processing;

namespace SGS.Experiments;

public sealed class ExperimentSettings
{
    public string DatasetPath { get; set; } = "";
    public string PreparedPath { get; set; } = "";
    public string OutputDirectory { get; set; } = "";
    public string ExtractionMode { get; set; } = "ocr";
    public string ServiceUrl { get; set; } = "";
    public string Pipeline { get; set; } = "";
    public string ModelLabel { get; set; } = "";
    public int Repetitions { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 120;
    public string? Revision { get; set; }
    public string? ApiKey { get; set; }
    public TesseractOcrOptions Ocr { get; set; } = new();
    public ChunkingOptions Chunking { get; set; } = new();
}
