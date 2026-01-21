using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SGS.DbContext.Data;
using SGS.DbContext.Repositories;
using SGS.DbContext.Repositories.Interfaces;
using SGS.DbContext.Repositories.UnitOfWork;

namespace SGS.DbContext;

public static class InfrastructureModule
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment  environment)
    {
        services.AddContext(configuration, environment);
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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddContext(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            if (env.EnvironmentName == "Testing")
            {
                options.UseInMemoryDatabase("TestDatabase")
                    .ConfigureWarnings(warning =>
                        warning.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }
            else
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            }
        });
        services.AddScoped<AppDbContext>();
    }
}
