using SGS.Experiments;

namespace SGS.ServicesTests.Documents;

public sealed class ExperimentConfigurationTests : IDisposable
{
    private readonly DirectoryInfo _project = Directory.CreateTempSubdirectory("sgs-experiment-config-");
    private readonly string _applicationDirectory;

    public ExperimentConfigurationTests()
    {
        _applicationDirectory = Directory.CreateDirectory(Path.Combine(_project.FullName, "bin", "Debug", "net9.0")).FullName;
        File.WriteAllText(Path.Combine(_project.FullName, "SGS.Experiments.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(_project.FullName, "appsettings.json"), """
            {
              "Experiment": {
                "DatasetPath": "data/dataset.json",
                "PreparedPath": "data/prepared.json",
                "OutputDirectory": "runs/baseline",
                "ServiceUrl": "http://localhost:5042",
                "Pipeline": "open-answer-v1",
                "ModelLabel": "test-model",
                "Repetitions": 3,
                "Chunking": { "MaxEstimatedTokens": 256, "OverlapEstimatedTokens": 32 },
                "Ocr": { "Languages": "eng", "PageSegmentationMode": 6 }
              }
            }
            """);
    }

    [Fact]
    public void Prepare_reads_project_configuration_without_a_key()
    {
        var settings = ExperimentConfiguration.Load(["prepare"], _applicationDirectory, new Dictionary<string, string?>());

        Assert.Equal(Path.Combine(_project.FullName, "data", "dataset.json"), settings.DatasetPath);
        Assert.Equal(Path.Combine(_project.FullName, "data", "prepared.json"), settings.PreparedPath);
        Assert.Equal(256, settings.Chunking.MaxEstimatedTokens);
        Assert.Equal(32, settings.Chunking.OverlapEstimatedTokens);
        Assert.Equal(6, settings.Ocr.PageSegmentationMode);
        Assert.Null(settings.ApiKey);
    }

    [Fact]
    public void Env_file_and_environment_override_json_and_legacy_names()
    {
        File.WriteAllText(Path.Combine(_project.FullName, ".env"), """
            # local settings
            Experiment__ApiKey="local=key#literal"
            Experiment__OutputDirectory='runs/my run'
            Experiment__Repetitions=5
            SGS_OCR_LANGUAGES=ukr
            """);

        var settings = ExperimentConfiguration.Load(["run"], _applicationDirectory, new Dictionary<string, string?>
        {
            ["SGS_EXPERIMENT_API_KEY"] = "legacy-key",
            ["Experiment__ApiKey"] = "environment-key",
            ["Experiment__Repetitions"] = "7",
            ["Experiment__Chunking__MaxEstimatedTokens"] = "384"
        });

        Assert.Equal("environment-key", settings.ApiKey);
        Assert.Equal(7, settings.Repetitions);
        Assert.Equal(Path.Combine(_project.FullName, "runs", "my run"), settings.OutputDirectory);
        Assert.Equal("ukr", settings.Ocr.Languages);
        Assert.Equal("test-model", settings.ModelLabel);
        Assert.Equal(384, settings.Chunking.MaxEstimatedTokens);
        Assert.Equal("local=key#literal", ExperimentConfiguration.Load(["run"], _applicationDirectory,
            new Dictionary<string, string?>()).ApiKey);
    }

    [Fact]
    public void Legacy_positional_arguments_use_current_working_directory_and_take_precedence()
    {
        var settings = ExperimentConfiguration.Load(
            ["run", "old/input.json", "old/output", "http://localhost:1234", "legacy-pipeline", "legacy-model", "2"],
            _applicationDirectory, new Dictionary<string, string?>
            {
                ["SGS_EXPERIMENT_API_KEY"] = "test",
                ["Experiment__Repetitions"] = "9"
            });

        Assert.Equal(Path.GetFullPath("old/input.json"), settings.PreparedPath);
        Assert.Equal(Path.GetFullPath("old/output"), settings.OutputDirectory);
        Assert.Equal("legacy-pipeline", settings.Pipeline);
        Assert.Equal(2, settings.Repetitions);
    }

    [Fact]
    public void Standalone_application_reads_configuration_next_to_executable()
    {
        File.Delete(Path.Combine(_project.FullName, "SGS.Experiments.csproj"));
        File.Copy(Path.Combine(_project.FullName, "appsettings.json"), Path.Combine(_applicationDirectory, "appsettings.json"));
        var settings = ExperimentConfiguration.Load(["prepare"], _applicationDirectory, new Dictionary<string, string?>());
        Assert.Equal(Path.Combine(_applicationDirectory, "data", "dataset.json"), settings.DatasetPath);
    }

    [Fact]
    public void Invalid_configuration_fails_before_running()
    {
        Assert.Throws<ArgumentException>(() => ExperimentConfiguration.Load(["run"], _applicationDirectory,
            new Dictionary<string, string?>()));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExperimentConfiguration.Load(["prepare"], _applicationDirectory,
            new Dictionary<string, string?> { ["Experiment__Chunking__OverlapEstimatedTokens"] = "256" }));
        File.WriteAllText(Path.Combine(_project.FullName, ".env"), "a malformed secret");
        var error = Assert.Throws<InvalidDataException>(() => ExperimentConfiguration.Load(["prepare"], _applicationDirectory,
            new Dictionary<string, string?>()));
        Assert.DoesNotContain("secret", error.Message);
    }

    public void Dispose() => _project.Delete(recursive: true);
}
