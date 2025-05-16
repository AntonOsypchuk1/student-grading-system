namespace SGS.SemanticAnalysis.Services.Bases;

public interface IPromptServiceExecutor
{
    Task<TResult?> ExecuteAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default);
}