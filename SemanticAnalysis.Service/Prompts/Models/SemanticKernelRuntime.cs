using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticAnalysis.Service.Prompts.Models;

public sealed record SemanticKernelRuntime(
    Kernel Kernel,
    OpenAIPromptExecutionSettings ExecutionSettings);
