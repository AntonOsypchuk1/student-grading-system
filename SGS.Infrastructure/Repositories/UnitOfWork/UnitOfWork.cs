using Microsoft.EntityFrameworkCore.Storage;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public IAnalysisResultsRepository AnalysisResults { get; } = new AnalysisResultsRepository(context);
    public IAssignmentRepository Assignments { get; } = new AssignmentRepository(context);
    public ICourseRepository Courses { get; } = new CourseRepository(context);
    public IDocumentRepository Documents { get; } = new DocumentRepository(context);
    public IGradeRepository Grades { get; } = new GradeRepository(context);
    public IGradingCriteriaRepository GradingCriteria { get; } = new GradingCriteriaRepository(context);
    public IRefreshTokenRepository RefreshTokens { get; } = new RefreshTokenRepository(context);
    public IStudentsGroupRepository StudentsGroups { get; } = new StudentsGroupRepository(context);
    public IUserRepository Users { get; } = new UserRepository(context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await _transaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _transaction?.RollbackAsync(cancellationToken)!;
        _transaction?.Dispose();
        _transaction = null;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            context.Dispose();
            _transaction?.Dispose();
        }

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await context.DisposeAsync();
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}