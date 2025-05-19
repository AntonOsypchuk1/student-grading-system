using Microsoft.Extensions.Logging;
using SGS.SemanticAnalysis.RetryPolicies;
using SGS.SemanticAnalysis.Services.Bases;

namespace SGS.SemanticAnalysis.Services;

public class PromptServiceExecutor(
    IServiceProvider serviceProvider,
    ISemanticKernelRetryPolicy retryPolicy,
    ILogger<PromptServiceExecutor>? logger = null)
    : IPromptServiceExecutor
{
    public async Task<TResult?> ExecuteAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
    {
        var promptService = GetPromptService<TRequest, TResult>();
        
        logger?.LogInformation("Executing prompt service {ServiceType}", promptService.GetType().Name);

        var result = await retryPolicy.CreateRetryPolicy<TResult>().ExecuteAsync(() => promptService.ExecuteAsync(request, cancellationToken));

        if (result == null)
        {
            logger?.LogWarning("Prompt service {ServiceType} returned null result", 
                promptService.GetType().Name);
        }
            
        return result;
    }
    
    /// <summary>
    /// Resolves the prompt service from the service provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no matching prompt service is registered.</exception>
    private IPromptService<TRequest, TResult> GetPromptService<TRequest, TResult>()
    {
        var service = serviceProvider.GetService(typeof(IPromptService<TRequest, TResult>));
        
        if (service is not null) 
            return (IPromptService<TRequest, TResult>)service;

        logger?.LogError("No prompt service registered for request type {RequestType} and result type {ResultType}", 
            typeof(TRequest).Name, typeof(TResult).Name);
        throw new InvalidOperationException(
            $"No prompt service registered for request type {typeof(TRequest).Name} and result type {typeof(TResult).Name}");
    }
}