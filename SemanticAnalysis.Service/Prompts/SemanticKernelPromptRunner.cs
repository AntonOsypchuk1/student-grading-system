using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace SemanticAnalysis.Service.Prompts;

public sealed class SemanticKernelPromptRunner(
    ISemanticKernelFactory kernels,
    IHostEnvironment environment,
    ILogger<SemanticKernelPromptRunner> logger) : IStructuredPromptRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken)
    {
        if (prompt is not ("EvaluateRubric" or "GenerateSummary"))
        {
            throw new ArgumentException("Unknown prompt.", nameof(prompt));
        }
        
        var runtime = kernels.Create();
        var yaml = await File.ReadAllTextAsync(
            Path.Combine(environment.ContentRootPath, "Prompts", prompt + ".prompt.yaml"),
            cancellationToken);
        var function = runtime.Kernel.CreateFunctionFromPromptYaml(yaml, new HandlebarsPromptTemplateFactory());
        
        var arguments = new KernelArguments(runtime.ExecutionSettings)
        {
            ["InputJson"] = JsonSerializer.Serialize(input, JsonOptions),
            ["Language"] = kernels.Language is "ua" or "uk" ? "Ukrainian" : "English"
        };
        
        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await function.InvokeAsync(runtime.Kernel, arguments, cancellationToken);
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
