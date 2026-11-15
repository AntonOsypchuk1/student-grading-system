using SemanticAnalysis.Service.Prompts.Models;

namespace SemanticAnalysis.Service.Prompts.Abstractions;

public interface ISemanticKernelFactory
{
    string Language { get; }
    SemanticKernelRuntime Create();
}
