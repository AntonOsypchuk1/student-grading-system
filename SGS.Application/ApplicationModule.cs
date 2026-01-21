using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGS.Application.Common.Options;
using SGS.Application.Interfaces;
using SGS.Application.Mappings;
using SGS.Application.Services.Analysis;
using SGS.Application.Services.Assignment;
using SGS.Application.Services.Auth;
using SGS.Application.Services.Course;
using SGS.Application.Services.Document;
using SGS.Application.Services.Document.Extraction;
using SGS.Application.Services.Document.Storage;
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
        services.AddDocumentStorageServices(configuration);
        services.AddDocumentServices();
        services.AddLocalization();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IGradingCriteriaService, GradingCriteriaService>();
        services.AddScoped<IGradingService, GradingService>();
        services.AddScoped<IAnalysisService, AnalysisService>();
        services.AddScoped<IAiService, SemanticKernelAiService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStudentsGroupService, StudentsGroupService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    }

    private static void AddLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options =>
            options.ResourcesPath = "Resources");

        // 2) (Optional) Configure supported cultures
        // services.Configure<RequestLocalizationOptions>(opts =>
        // {
        //     var supported = configureOptions;
        //     opts.SetDefaultCulture("en")
        //         .AddSupportedCultures(supported)
        //         .AddSupportedUICultures(supported);
        // });
    }

    private static void AddDocumentStorageServices(this IServiceCollection services, IConfiguration configuration)
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

    private static void AddDocumentServices(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentTextExtractor, PdfDocumentTextExtractor>();
        services.AddSingleton<IDocumentTextExtractor, TxtDocumentTextExtractor>();

        services.AddSingleton<IDocumentTextExtractionManager, DocumentTextExtractionManager>();

        services.AddScoped<IDocumentService, DocumentService>();
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
    }
}
