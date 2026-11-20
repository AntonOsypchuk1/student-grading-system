using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging.Abstractions;
using SemanticAnalysis.Service.Protos;
using SGS.Application.Common.Options;
using SGS.Application.Services.Document.Extraction;
using SGS.Application.Services.Document.Processing;
using SGS.Experiments;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };

try
{
    switch (args)
    {
        case ["prepare", var manifest, var prepared, var mode]:
        {
            var ocrOptions = new TesseractOcrOptions
            {
                TessDataPath = Environment.GetEnvironmentVariable("SGS_OCR_TESSDATA_PATH") ?? "tessdata",
                NativeLibraryDirectory = Environment.GetEnvironmentVariable("SGS_OCR_NATIVE_LIBRARY_DIRECTORY"),
                Languages = Environment.GetEnvironmentVariable("SGS_OCR_LANGUAGES") ?? "eng"
            };

            var pdfExtractor = new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance);
            var docxExtractor = new DocxDocumentTextExtractor();
        
            using var provider = new TesseractImageOcrProvider(ocrOptions);
            using var processor = new DocumentProcessor(
                [pdfExtractor, docxExtractor],
                new StructureAwareChunker(),
                ocrProvider: provider);
        
            var result = await DatasetPreparer
                .PrepareAsync(manifest, processor, mode, ocrOptions.Languages, cancellation.Token);
        
            await WriteNewJsonAsync(prepared, result, cancellation.Token);
            Console.WriteLine($"Prepared {result.Submissions.Count} cases; " +
                              $"{result.Submissions.Count(s => s.Status != SGS.Application.Models.DocumentProcessing.ExtractionStatus.Complete)} partial/unreadable. No model calls.");
            return 0;
        }
        case ["run", var input, var output, var endpoint, var pipeline, var modelLabel, var repeatText]
            when int.TryParse(repeatText, out int repetitions) && repetitions is >= 1 and <= 100:
        {
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)) ||
                !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) ||
                !string.IsNullOrEmpty(uri.Fragment))
            {
                throw new ArgumentException("Use an HTTPS service endpoint, or HTTP for a loopback service only.");
            }
            
            if (string.IsNullOrWhiteSpace(pipeline) || string.IsNullOrWhiteSpace(modelLabel))
            {
                throw new ArgumentException("Pipeline and model label are required.");
            }
            
            var key = Environment.GetEnvironmentVariable("SGS_EXPERIMENT_API_KEY");
            
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Set SGS_EXPERIMENT_API_KEY to the semantic service's API key.");
            }
            
            var bytes = await File.ReadAllBytesAsync(input, cancellation.Token);
            var dataset = JsonSerializer.Deserialize<PreparedDataset>(bytes, BenchmarkJson.Options)
                          ?? throw new InvalidDataException("Prepared dataset is empty.");
            
            DatasetPreparer.Validate(dataset.Dataset);
            
            if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
            {
                throw new IOException("Run output directory must be empty; existing results will not be overwritten.");
            }
            
            Directory.CreateDirectory(output);
            await WriteNewJsonAsync(Path.Combine(output, "run.json"), new
            {
                dataset.Dataset.Id,
                dataset.Dataset.Version,
                preparedSha256 = DatasetPreparer.Sha256(bytes),
                pipeline,
                modelLabel,
                repetitions,
                endpoint = uri.GetLeftPart(UriPartial.Path),
                startedUtc = DateTimeOffset.UtcNow,
                timeoutSeconds = 120,
                dataset.ExtractionMode,
                dataset.OcrLanguages,
                revision = Environment.GetEnvironmentVariable("SGS_EXPERIMENT_REVISION")
            }, cancellation.Token);

            using var channel = GrpcChannel.ForAddress(uri);
            var client = new SemanticAnalysisService.SemanticAnalysisServiceClient(channel);
            var runner = new ExperimentRunner(async (request, token) => await client
                .EvaluateDocumentAsync(
                    request,
                    headers: new Metadata { { "x-api-key", key } },
                    deadline: DateTime.UtcNow.AddSeconds(120),
                    cancellationToken: token));
            
            var attempts = new List<EvaluationAttempt>();
            await using (var stream = new FileStream(Path.Combine(output, "results.jsonl"), FileMode.CreateNew, FileAccess.Write))
            await using (var writer = new StreamWriter(stream))
            {
                await foreach (var attempt in runner.RunAsync(dataset, pipeline, repetitions, cancellation.Token))
                {
                    attempts.Add(attempt);
                    await writer.WriteLineAsync(JsonSerializer.Serialize(attempt, BenchmarkJson.Lines).AsMemory(), cancellation.Token);
                    await writer.FlushAsync(cancellation.Token);
                    Console.WriteLine($"{attempt.CaseId} [{attempt.Repetition}/{repetitions}]: {attempt.Status}");
                }
            }
            
            await WriteNewJsonAsync(Path.Combine(output, "summary.json"), ExperimentMetrics.Calculate(attempts), cancellation.Token);
            Console.WriteLine($"Recorded {attempts.Count} attempts; {attempts.Count(a => a.Status == AttemptStatus.Scored)} scored.");
            return attempts.Any(a => a.Status == AttemptStatus.Scored) ? 0 : 2;
        }
        default:
            Console.WriteLine("Usage:\n  prepare <dataset.json> <prepared.json> <native|ocr>\n" +
                              "  run <prepared.json> <output-directory> <service-url> <pipeline> <model-label> <repetitions:1-100>");
            return 1;
    }
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
    Console.Error.WriteLine("Cancelled. Completed result lines remain on disk.");
    return 130;
}
catch (Exception ex) when (ex is ArgumentException or IOException or JsonException or RpcException or InvalidOperationException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static async Task WriteNewJsonAsync<T>(string file, T value, CancellationToken token)
{
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file))!);
    await using var stream = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
    await JsonSerializer.SerializeAsync(stream, value, BenchmarkJson.Options, token);
}
