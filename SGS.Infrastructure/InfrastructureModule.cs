using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure;

public static class InfrastructureModule
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContext(configuration);
        services.AddPersistence();
    }

    private static void AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IAnalysisResultsRepository, AnalysisResultsRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IGradingCriteriaRepository, GradingCriteriaRepository>();
        services.AddScoped<IStudentsGroupRepository, StudentsGroupRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    private static void AddContext(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Use in-memory database for testing
        // For now just comment it out
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<AppDbContext>();
    }
}