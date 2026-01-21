using SGS.DbContext.Repositories.Interfaces;

namespace SGS.DbContext.Repositories.UnitOfWork;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IAnalysisResultsRepository AnalysisResults { get; }
    IAssignmentRepository Assignments { get; }
    ICourseRepository Courses { get; }
    IDocumentRepository Documents { get; }
    IGradeRepository Grades { get; }
    IGradingCriteriaRepository GradingCriteria { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IStudentsGroupRepository StudentsGroups { get; }
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}