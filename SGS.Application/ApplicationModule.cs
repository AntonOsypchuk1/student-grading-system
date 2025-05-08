using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGS.Application.Common.Options;
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
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddServices();
        services.AddAutoMapper();
        services.AddStorageServices(configuration);
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IGradingCriteriaService, GradingCriteriaService>();
        services.AddScoped<IGradingService, GradingService>();
        services.AddScoped<IAIService, SemanticKernelAiService>(); // TODO
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    }

    private static void AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(options =>
        {
            configuration.GetSection("Storage").Bind(options);
        });
        
        var providerSettings = configuration.GetSection("Storage");
        var provider = providerSettings["Provider"] ?? "Disk";

        if (provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IFileStorageService, BlobStorageService>();
        }
        else
        {
            services.AddSingleton<IFileStorageService, DiskFileStorageService>();
        }
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
    }
}