using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticAnalysis.Service.Settings;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Prompts.Models;

namespace SemanticAnalysis.Service.Prompts;

public sealed class SemanticKernelFactory(
    IOptions<SemanticKernelSettings> settings,
    IHttpClientFactory httpClients) : ISemanticKernelFactory
{
    public string Language => settings.Value.Language;

    public SemanticKernelRuntime Create()
    {
        var root = settings.Value;
        var config = GetProviderSettings(root);

        var builder = Kernel.CreateBuilder();
        var client = httpClients.CreateClient("GradingModel");

        switch (root.Provider)
        {
            case ModelProvider.AzureOpenAI:
                builder.AddAzureOpenAIChatCompletion(
                    config.Model!, config.Endpoint!, config.ApiKey!, httpClient: client);
                break;
            case ModelProvider.OpenAI or ModelProvider.OpenRouter:
            {
                if (!string.IsNullOrWhiteSpace(config.HttpReferer))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", config.HttpReferer);
                }

                if (!string.IsNullOrWhiteSpace(config.AppTitle))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", config.AppTitle);
                }

                builder.AddOpenAIChatCompletion(
                    config.Model!, new Uri(config.Endpoint!), config.ApiKey!, httpClient: client);
                    break;
            }
        }
        return new SemanticKernelRuntime(
            builder.Build(),
            new OpenAIPromptExecutionSettings
            {
                Temperature = 0,
                ResponseFormat = config.RequireJsonObject ? "json_object" : null,
                MaxTokens = config.MaxTokens
            });
    }

    private static ModelProviderSettings GetProviderSettings(SemanticKernelSettings settings)
    {
        var config = settings.Provider switch
        {
            ModelProvider.AzureOpenAI => settings.AzureOpenAI,
            ModelProvider.OpenAI => settings.OpenAI,
            ModelProvider.OpenRouter => settings.OpenRouter,
            _ => throw new InvalidOperationException($"Unsupported model provider '{settings.Provider}'.")
        };

        Validate(config, settings.Provider);
        return config;
    }

    private static void Validate(ModelProviderSettings config, ModelProvider provider)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey)
            || string.IsNullOrWhiteSpace(config.Model))
        {
            throw new InvalidOperationException(
                $"Configure SemanticKernelSettings:{provider}:Model and ApiKey.");
        }

        if (string.IsNullOrWhiteSpace(config.Endpoint))
        {
            throw new InvalidOperationException(
                $"Configure SemanticKernelSettings:{provider}:Endpoint.");
        }

        if (!Uri.TryCreate(config.Endpoint, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("SemanticKernelSettings provider endpoint must be an absolute URI.");
        }
    }
}
