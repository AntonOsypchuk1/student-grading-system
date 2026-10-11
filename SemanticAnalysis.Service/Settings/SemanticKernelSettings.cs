namespace SemanticAnalysis.Service.Settings;

public sealed class SemanticKernelSettings
{
    public ModelProvider Provider { get; set; } = ModelProvider.AzureOpenAI;
    public string Language { get; set; } = "en";
    public ModelProviderSettings AzureOpenAI { get; set; } = new();
    public ModelProviderSettings OpenAI { get; set; } = new();
    public ModelProviderSettings OpenRouter { get; set; } = new();
}

public enum ModelProvider
{
    AzureOpenAI,
    OpenAI,
    OpenRouter
}

public sealed class ModelProviderSettings
{
    public string? Model { get; set; }
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? HttpReferer { get; set; }
    public string? AppTitle { get; set; }
    public bool RequireJsonObject { get; set; } = true;
    public int MaxTokens { get; set; } = 8000;
}
