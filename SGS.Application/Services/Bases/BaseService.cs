using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Bases;

/// <summary>
/// Base service class that provides transaction management capabilities for service operations.
/// Handles the transaction lifecycle (begin, commit, rollback) and provides logging and mapping support.
/// </summary>
public abstract class BaseService(
    IUnitOfWork unitOfWork,
    ILogger logger,
    IMapper mapper)
{
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;
    protected IMapper Mapper { get; } = mapper;
    protected ILogger Logger { get; } = logger;
    
    /// <summary>
    /// Executes an operation within a database transaction that returns a result.
    /// Automatically manages the transaction lifecycle (begin, commit, rollback).
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation</typeparam>
    /// <param name="operation">The operation to execute within the transaction</param>
    /// <param name="cancellationToken">Cancellation token for canceling the operation</param>
    /// <returns>The result of the operation</returns>
    /// <exception cref="Exception">Rethrows any exceptions that occur during the operation after rollback</exception>
    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await UnitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation();
            await UnitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing transaction");
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
    
    /// <summary>
    /// Executes an operation within a database transaction that doesn't return a result.
    /// Automatically manages the transaction lifecycle (begin, commit, rollback).
    /// </summary>
    /// <param name="operation">The operation to execute within the transaction</param>
    /// <param name="cancellationToken">Cancellation token for canceling the operation</param>
    /// <exception cref="Exception">Rethrows any exceptions that occur during the operation after rollback</exception>
    protected async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await UnitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await operation();
            await UnitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing transaction");
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}