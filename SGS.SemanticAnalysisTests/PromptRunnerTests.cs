using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SemanticAnalysis.Service.Evaluation.Models;
using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Prompts;
using SemanticAnalysis.Service.Settings;

namespace SGS.SemanticAnalysisTests;

public sealed class PromptRunnerTests
{
    [Fact]
    public async Task ActualConnectorAndYamlParseResponseWithoutExternalCalls()
    {
        using var handler = new ModelHandler([ValidOutput]);
        var output = await Runner(handler).RunAsync<RubricResponse>("EvaluateRubric", new
        {
            DocumentText = "Independent encoding.", Question = "Explain bi-encoders.",
            Criteria = new[] { new { Id = "x", Description = "Encoding", MaxScore = 10 } }
        }, default);
        Assert.Equal(8, output.Value.Criteria.Single().Score);
        Assert.False(output.HadRetry);
        Assert.Equal(1, handler.Calls);
        Assert.Contains("Independent encoding.", handler.LastBody);
    }

    [Fact]
    public async Task MalformedJsonRetriesOnceAndRecordsRetry()
    {
        using var handler = new ModelHandler(["not json", ValidOutput]);
        var output = await Runner(handler).RunAsync<RubricResponse>("EvaluateRubric", new { DocumentText = "answer" }, default);
        Assert.True(output.HadRetry);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task OpenRouterPoolUsesOpenAiCompatibleEndpointAndModel()
    {
        using var handler = new ModelHandler([ValidOutput]);
        var output = await Runner(handler, useOpenRouter: true).RunAsync<RubricResponse>(
            "EvaluateRubric", new { DocumentText = "answer" }, default);

        Assert.Equal(8, output.Value.Criteria.Single().Score);
        Assert.Contains("/chat/completions", handler.LastUri);
        Assert.Contains("openrouter/test-model", handler.LastBody);
    }

    [Fact]
    public async Task MissingRequiredResponseFieldsFailsAfterBoundedRetry()
    {
        using var handler = new ModelHandler(["{}", "{}"]);
        await Assert.ThrowsAsync<JsonException>(() => Runner(handler).RunAsync<RubricResponse>(
            "EvaluateRubric", new { DocumentText = "answer" }, default));
        Assert.Equal(2, handler.Calls);
    }

    private const string ValidOutput = "{\"criteria\":[{\"id\":\"x\",\"score\":8,\"explanation\":\"Good explanation.\",\"quotes\":[\"Independent encoding.\"]}]}";
    private static SemanticKernelPromptRunner Runner(ModelHandler handler, bool useOpenRouter = false)
    {
        var options = Options.Create(new SemanticKernelSettings
        {
            Provider = useOpenRouter ? ModelProvider.OpenRouter : ModelProvider.OpenAI,
            OpenAI = new ModelProviderSettings
            {
                Endpoint = "https://model.invalid/v1", ApiKey = "test-key", Model = "test-model"
            },
            OpenRouter = new ModelProviderSettings
            {
                Endpoint = "https://openrouter.invalid/api/v1", ApiKey = "test-key",
                Model = "openrouter/test-model"
            }
        });

        return new(
            new SemanticKernelFactory(options, new TestClientFactory(handler)),
            new TestEnvironment(),
            NullLogger<SemanticKernelPromptRunner>.Instance);
    }

    private sealed class TestClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }
    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
    private sealed class ModelHandler(string[] outputs) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string LastBody { get; private set; } = "";
        public string LastUri { get; private set; } = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri!.ToString();
            LastBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            var content = outputs[Calls++];
            var response = JsonSerializer.Serialize(new
            {
                id = "test", @object = "chat.completion", created = 1, model = "test-model",
                choices = new[] { new { index = 0, message = new { role = "assistant", content }, finish_reason = "stop" } },
                usage = new { prompt_tokens = 1, completion_tokens = 1, total_tokens = 2 }
            });
            return new(HttpStatusCode.OK) { Content = new StringContent(response, Encoding.UTF8, "application/json") };
        }
    }
}
