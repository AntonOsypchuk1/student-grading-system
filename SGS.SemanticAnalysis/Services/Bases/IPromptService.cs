namespace SGS.SemanticAnalysis.Services.Bases;

public interface IPromptService<TRequest, TResponse>
{
    Task<TResponse?> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}