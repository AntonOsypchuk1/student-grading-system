using Polly.Retry;

namespace SemanticAnalysis.Service.RetryPolicies;

public interface ISemanticKernelRetryPolicy
{
    /// <summary>
    /// Creates a retry policy with exponential backoff.
    /// </summary>
    AsyncRetryPolicy<TResult?> CreateRetryPolicy<TResult>();
}