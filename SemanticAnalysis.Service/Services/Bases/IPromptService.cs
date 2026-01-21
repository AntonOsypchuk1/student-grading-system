namespace SemanticAnalysis.Service.Services.Bases;

public interface IPromptService<in TRequest, TResponse>
{
    Task<TResponse?> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
