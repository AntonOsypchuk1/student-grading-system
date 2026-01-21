namespace SGS.SemanticAnalysis.Client;

public interface IPromptServiceExecutor
{
    Task<TResult?> ExecuteAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default);
}
