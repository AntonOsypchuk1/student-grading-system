using SGS.SemanticAnalysis.Services.Bases;

namespace SGS.SemanticAnalysis.Services;

public class PromptServiceExecutor(IServiceProvider serviceProvider) : IPromptServiceExecutor
{
    public async Task<TResult?> ExecuteAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
    {
        var service = Get<TRequest, TResult>();
        return await service.ExecuteAsync(request, cancellationToken);
    }
    
    private IPromptService<TRequest, TResult> Get<TRequest, TResult>()
    {
        var service = serviceProvider.GetService(typeof(IPromptService<TRequest, TResult>));
        if (service is null)
        {
            throw new InvalidOperationException($"Service of type {typeof(IPromptService<TRequest, TResult>)} not found.");
        }

        return (IPromptService<TRequest, TResult>)service;
    }
}