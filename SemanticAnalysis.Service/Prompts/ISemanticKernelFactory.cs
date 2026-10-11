using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticAnalysis.Service.Prompts;

public sealed record SemanticKernelRuntime(
    Kernel Kernel,
    OpenAIPromptExecutionSettings ExecutionSettings);

public interface ISemanticKernelFactory
{
    string Language { get; }
    SemanticKernelRuntime Create();
}
