using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Infrastructure;

public static class InfrastructureModule
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
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
        IWebHostEnvironment env)
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