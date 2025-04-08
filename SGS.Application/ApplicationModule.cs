using Microsoft.Extensions.DependencyInjection;
using SGS.Application.Interfaces;
using SGS.Application.Mappings;
using SGS.Application.Services;

namespace SGS.Application;

public static class ApplicationModule
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddServices();
        services.AddAutoMapper();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IGradingCriteriaService, GradingCriteriaService>();
        services.AddScoped<IGradingService, GradingService>();
        services.AddScoped<IAIService, SemanticKernelAiService>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IFileStorageService>(provider =>
            new DiskFileStorageService(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")));
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
    }
}