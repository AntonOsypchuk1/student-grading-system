namespace SemanticAnalysis.Service.Prompts.Models;

public sealed record PromptOutput<T>(T Value, bool HadRetry);
