using System.Collections;
using Microsoft.Extensions.Configuration;

namespace SGS.Experiments;

public static class ExperimentConfiguration
{
    private const string Section = "Experiment";

    private static readonly Dictionary<string, string> LegacyKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SGS_EXPERIMENT_API_KEY"] = "Experiment:ApiKey",
        ["SGS_EXPERIMENT_REVISION"] = "Experiment:Revision",
        ["SGS_OCR_TESSDATA_PATH"] = "Experiment:Ocr:TessDataPath",
        ["SGS_OCR_NATIVE_LIBRARY_DIRECTORY"] = "Experiment:Ocr:NativeLibraryDirectory",
        ["SGS_OCR_LANGUAGES"] = "Experiment:Ocr:Languages"
    };

    public static bool IsSupportedCommand(string[] args) =>
        args is ["prepare"] or ["prepare", _, _, _]
            or ["run"] or ["run", _, _, _, _, _, _] or ["compare"];

    public static ExperimentSettings Load(
        string[] args,
        string applicationDirectory,
        IReadOnlyDictionary<string, string?>? environment = null)
    {
        if (!IsSupportedCommand(args))
            throw new ArgumentException("Expected prepare, run or compare.");

        var directory = FindConfigurationDirectory(applicationDirectory);
        var variables = environment ?? Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string?)entry.Value);

        var config = new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddInMemoryCollection(NormalizeVariables(ReadDotEnv(Path.Combine(directory, ".env"))))
            .AddInMemoryCollection(NormalizeVariables(variables))
            .Build();

        var settings = config.GetRequiredSection(Section).Get<ExperimentSettings>()
            ?? throw new InvalidOperationException("Experiment configuration is missing.");

        settings.DatasetPath = ResolvePath(settings.DatasetPath, directory);
        settings.PreparedPath = ResolvePath(settings.PreparedPath, directory);
        settings.OutputDirectory = ResolvePath(settings.OutputDirectory, directory);
        settings.ComparisonSourceDirectory = ResolvePath(settings.ComparisonSourceDirectory, directory);
        settings.ComparisonOutputPath = ResolvePath(settings.ComparisonOutputPath, directory);

        // Keep previous positional commands working, with their original CWD-relative paths.
        switch (args)
        {
            case ["prepare", var manifest, var prepared, var mode]:
                settings.DatasetPath = Path.GetFullPath(manifest);
                settings.PreparedPath = Path.GetFullPath(prepared);
                settings.ExtractionMode = mode;
                break;
            case ["run", var input, var output, var endpoint, var pipeline, var modelLabel, var repeatText]:
                settings.PreparedPath = Path.GetFullPath(input);
                settings.OutputDirectory = Path.GetFullPath(output);
                settings.ServiceUrl = endpoint;
                settings.Pipeline = pipeline;
                settings.ModelLabel = modelLabel;
                settings.Repetitions = int.TryParse(repeatText, out var count) ? count : 0;
                break;
        }

        Validate(settings, args[0]);
        return settings;
    }

    private static string FindConfigurationDirectory(string applicationDirectory)
    {
        // dotnet run / IDE: read the editable project configuration, not a stale bin copy.
        for (var directory = new DirectoryInfo(applicationDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SGS.Experiments.csproj")))
                return directory.FullName;
        }

        // Published / standalone executable: configuration next to the application.
        return Path.GetFullPath(applicationDirectory);
    }

    private static string ResolvePath(string path, string directory) =>
        string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path, directory);

    private static void Validate(ExperimentSettings settings, string command)
    {
        if (string.IsNullOrWhiteSpace(settings.PreparedPath))
            throw new ArgumentException("Experiment:PreparedPath is required.");

        if (command == "compare")
        {
            if (string.IsNullOrWhiteSpace(settings.ComparisonSourceDirectory) ||
                string.IsNullOrWhiteSpace(settings.ComparisonOutputPath))
                throw new ArgumentException("ComparisonSourceDirectory and ComparisonOutputPath are required.");
            return;
        }

        if (command == "prepare")
        {
            if (string.IsNullOrWhiteSpace(settings.DatasetPath))
            {
                throw new ArgumentException("Experiment:DatasetPath is required.");
            }
            if (settings.ExtractionMode is not ("native" or "ocr"))
            {
                throw new ArgumentException("Experiment:ExtractionMode must be native or ocr.");
            }
            
            settings.Chunking.Validate();
            if (settings.ExtractionMode == "ocr" &&
                (settings.Ocr.PageSegmentationMode is < 0 or > 13 ||
                 settings.Ocr.ImageScaleFactor is < 1 or > 3 ||
                 string.IsNullOrWhiteSpace(settings.Ocr.Languages) ||
                 string.IsNullOrWhiteSpace(settings.Ocr.TessDataPath)))
            {
                throw new ArgumentException(
                    "Invalid Experiment:Ocr settings (languages, tessdata path, page segmentation mode or image scale factor 1–3).");
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
        {
            throw new ArgumentException("Experiment:OutputDirectory is required.");
        }
        if (settings.Repetitions is < 1 or > 100)
        {
            throw new ArgumentException("Experiment:Repetitions must be between 1 and 100.");
        }
        if (settings.TimeoutSeconds <= 0)
        {
            throw new ArgumentException("Experiment:TimeoutSeconds must be positive.");
        }
        if (settings.DelayBetweenRequestsMs is < 0 or > 60000)
            throw new ArgumentException("Experiment:DelayBetweenRequestsMs must be between 0 and 60000.");
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new ArgumentException(
                "Set Experiment__ApiKey in .env or the environment to the semantic service's API key.");
        }
    }

    private static Dictionary<string, string?> NormalizeVariables(IEnumerable<KeyValuePair<string, string?>> variables)
    {
        var entries = variables.ToArray();
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in entries)
            if (LegacyKeys.TryGetValue(key, out var target))
                values[target] = value;
        foreach (var (key, value) in entries)
            if (key.StartsWith(Section + "__", StringComparison.OrdinalIgnoreCase))
                values[key.Replace("__", ":")] = value;
        return values;
    }

    private static Dictionary<string, string?> ReadDotEnv(string path)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
            return values;

        var lineNumber = 0;
        foreach (var raw in File.ReadLines(path))
        {
            lineNumber++;
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                throw new InvalidDataException($"Invalid .env assignment on line {lineNumber}.");
            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (key.Length == 0 || key.Any(char.IsWhiteSpace))
                throw new InvalidDataException($"Invalid .env key on line {lineNumber}.");
            if (value.StartsWith('"') || value.StartsWith('\''))
            {
                if (value.Length < 2 || value[^1] != value[0])
                    throw new InvalidDataException($"Unclosed .env quote on line {lineNumber}.");
                value = value[1..^1];
            }
            values[key] = value;
        }
        return values;
    }
}
