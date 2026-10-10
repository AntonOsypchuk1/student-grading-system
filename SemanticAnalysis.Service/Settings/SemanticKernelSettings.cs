namespace SemanticAnalysis.Service.Settings;

public class SemanticKernelSettings
{
    public string Provider { get; set; } = "AzureOpenAI";
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public string? DeploymentName { get; set; }
    public string? Language { get; set; }
}