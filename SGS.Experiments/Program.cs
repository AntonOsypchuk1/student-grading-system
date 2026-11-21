using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging.Abstractions;
using SemanticAnalysis.Service.Protos;
using SGS.Application.Services.Document.Extraction;
using SGS.Application.Services.Document.Processing;
using SGS.Experiments;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };

try
{
    if (!ExperimentConfiguration.IsSupportedCommand(args))
    {
        Console.WriteLine("Configure SGS.Experiments/appsettings.json or .env, then run:\n  prepare\n  run\n  compare");
        return 1;
    }

    var settings = ExperimentConfiguration.Load(args, AppContext.BaseDirectory);
    switch (args[0])
    {
        case "compare":
        {
            var comparison = await StrategyComparison.CompareAsync(settings.PreparedPath,
                settings.ComparisonSourceDirectory, cancellation.Token);
            await WriteNewJsonAsync(settings.ComparisonOutputPath, comparison, cancellation.Token);
            Console.WriteLine($"Compared {comparison.Methods.Count} strategies on {comparison.PairedAttempts}/{comparison.PlannedAttempts} shared evidence sets. No model calls.");
            return comparison.PairedAttempts > 0 ? 0 : 2;
        }
        case "prepare":
        {
            var ocrOptions = settings.Ocr;

            var pdfExtractor = new PdfDocumentTextExtractor(NullLogger<PdfDocumentTextExtractor>.Instance);
            var docxExtractor = new DocxDocumentTextExtractor();
        
            using var provider = new TesseractImageOcrProvider(ocrOptions);
            using var processor = new DocumentProcessor(
                [pdfExtractor, docxExtractor],
                new StructureAwareChunker(),
                ocrProvider: provider);
        
            var result = await DatasetPreparer
                .PrepareAsync(settings.DatasetPath, processor, settings.ExtractionMode, ocrOptions.Languages,
                    cancellation.Token, settings.Chunking, ocrOptions.PageSegmentationMode);
        
            await WriteNewJsonAsync(settings.PreparedPath, result, cancellation.Token);
            Console.WriteLine($"Prepared {result.Submissions.Count} cases; " +
                              $"{result.Submissions.Count(s => s.Status != SGS.Application.Models.DocumentProcessing.ExtractionStatus.Complete)} partial/unreadable. No model calls.");
            return 0;
        }
        case "run":
        {
            var output = settings.OutputDirectory;
            var pipeline = settings.Pipeline;
            var modelLabel = settings.ModelLabel;
            var repetitions = settings.Repetitions;
            var key = settings.ApiKey!;
            if (!Uri.TryCreate(settings.ServiceUrl, UriKind.Absolute, out var uri) ||
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
            
            var bytes = await File.ReadAllBytesAsync(settings.PreparedPath, cancellation.Token);
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
                timeoutSeconds = settings.TimeoutSeconds,
                delayBetweenRequestsMs = settings.DelayBetweenRequestsMs,
                dataset.ExtractionMode,
                dataset.OcrLanguages,
                dataset.Chunking,
                dataset.OcrPageSegmentationMode,
                revision = string.IsNullOrWhiteSpace(settings.Revision) ? null : settings.Revision
            }, cancellation.Token);

            using var channel = GrpcChannel.ForAddress(uri);
            var client = new SemanticAnalysisService.SemanticAnalysisServiceClient(channel);
            var runner = new ExperimentRunner(async (request, token) => await client
                .EvaluateDocumentAsync(
                    request,
                    headers: new Metadata { { "x-api-key", key } },
                    deadline: DateTime.UtcNow.AddSeconds(settings.TimeoutSeconds),
                    cancellationToken: token));
            
            var attempts = new List<EvaluationAttempt>();
            await using (var stream = new FileStream(Path.Combine(output, "results.jsonl"), FileMode.CreateNew, FileAccess.Write))
            await using (var writer = new StreamWriter(stream))
            {
                await foreach (var attempt in runner.RunAsync(dataset, pipeline, repetitions, cancellation.Token,
                    settings.DelayBetweenRequestsMs))
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
            return 1;
    }
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
    Console.Error.WriteLine("Cancelled. Completed result lines remain on disk.");
    return 130;
}
catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or JsonException or RpcException or InvalidOperationException)
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
