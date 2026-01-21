using Polly;
using Polly.Retry;

namespace SemanticAnalysis.Service.RetryPolicies;

public class SemanticKernelRetryPolicy(ILogger<SemanticKernelRetryPolicy>? logger = null) : ISemanticKernelRetryPolicy
{
    // TODO: move to application settings
    private const int MaxRetryAttempts = 3;
    private const double RetryBackoffFactor = 2;
    
    /// <summary>
    /// Creates a retry policy with exponential backoff.
    /// </summary>
    public AsyncRetryPolicy<TResult?> CreateRetryPolicy<TResult>()
    {
        return Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .OrResult<TResult?>(r => r == null)
            .WaitAndRetryAsync(
                MaxRetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(RetryBackoffFactor, retryAttempt - 1)),
                OnRetryAction
            );
    }
    
    /// <summary>
    /// Action to execute on each retry attempt.
    /// </summary>
    private void OnRetryAction<TResult>(DelegateResult<TResult?> outcome, TimeSpan timeSpan, int retryCount, Context _)
    {
        if (outcome.Exception != null)
        {
            logger?.LogWarning(outcome.Exception, 
                "Attempt {RetryCount} failed with exception. Retrying in {Delay}s...", 
                retryCount, timeSpan.TotalSeconds);
        }
        else
        {
            logger?.LogWarning(
                "Attempt {RetryCount} returned null result. Retrying in {Delay}s...", 
                retryCount, timeSpan.TotalSeconds);
        }
    }
}