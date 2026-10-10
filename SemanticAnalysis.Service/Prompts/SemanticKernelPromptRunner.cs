using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticAnalysis.Service.Settings;

namespace SemanticAnalysis.Service.Prompts;

public sealed class SemanticKernelPromptRunner(
    IOptions<SemanticKernelSettings> settings,
    IHostEnvironment environment,
    IHttpClientFactory httpClients,
    ILogger<SemanticKernelPromptRunner> logger) : IStructuredPromptRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken)
    {
        var config = settings.Value;
        if (string.IsNullOrWhiteSpace(config.DeploymentName) || string.IsNullOrWhiteSpace(config.Endpoint) ||
            string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("Configure SemanticKernelSettings deployment, endpoint and API key.");
        if (prompt is not ("EvaluateRubric" or "GenerateSummary"))
            throw new ArgumentException("Unknown prompt.", nameof(prompt));
        var builder = Kernel.CreateBuilder();
        var client = httpClients.CreateClient("GradingModel");
        if (config.Provider == "AzureOpenAI")
            builder.AddAzureOpenAIChatCompletion(config.DeploymentName, config.Endpoint, config.ApiKey, httpClient: client);
        else if (config.Provider == "OpenAI")
            builder.AddOpenAIChatCompletion(config.DeploymentName, new Uri(config.Endpoint), config.ApiKey, httpClient: client);
        else
            throw new InvalidOperationException("Supported model providers: AzureOpenAI, OpenAI.");
        var kernel = builder.Build();
        var yaml = await File.ReadAllTextAsync(Path.Combine(environment.ContentRootPath, "Prompts", prompt + ".prompt.yaml"),
            cancellationToken);
        var function = kernel.CreateFunctionFromPromptYaml(yaml, new HandlebarsPromptTemplateFactory());
        var arguments = new KernelArguments(new OpenAIPromptExecutionSettings
        {
            Temperature = 0, ResponseFormat = "json_object", MaxTokens = 8000
        })
        {
            ["InputJson"] = JsonSerializer.Serialize(input, JsonOptions),
            ["Language"] = config.Language is "ua" or "uk" ? "Ukrainian" : "English"
        };
        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await function.InvokeAsync(kernel, arguments, cancellationToken);
                var value = JsonSerializer.Deserialize<T>(response.GetValue<string>() ?? "", JsonOptions)
                    ?? throw new JsonException("The model returned null.");
                return new(value, attempt > 0);
            }
            catch (JsonException) when (attempt == 0)
            {
                logger.LogWarning("Invalid JSON for {Prompt}; retrying once", prompt);
            }
        }
    }
}
