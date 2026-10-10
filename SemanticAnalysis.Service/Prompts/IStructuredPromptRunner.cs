namespace SemanticAnalysis.Service.Prompts;

public sealed record PromptOutput<T>(T Value, bool HadRetry);
public interface IStructuredPromptRunner
{
    Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken);
}
