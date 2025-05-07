using Microsoft.Extensions.DependencyInjection;
using SGS.Application.Interfaces;
using SGS.Application.Mappings;
using SGS.Application.Services.Analisys;
using SGS.Application.Services.Assignment;
using SGS.Application.Services.Auth;
using SGS.Application.Services.Course;
using SGS.Application.Services.Document;
using SGS.Application.Services.Grading;
using SGS.Application.Services.User;

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

        // TODO: Something here to handle file storage
        services.AddScoped<IFileStorageService>(_ =>
            new DiskFileStorageService(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")));
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
    }
}