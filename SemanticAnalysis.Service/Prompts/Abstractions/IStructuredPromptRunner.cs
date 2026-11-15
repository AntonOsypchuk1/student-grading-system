using SemanticAnalysis.Service.Prompts.Models;

namespace SemanticAnalysis.Service.Prompts.Abstractions;

public interface IStructuredPromptRunner
{
    Task<PromptOutput<T>> RunAsync<T>(string prompt, object input, CancellationToken cancellationToken);
}
