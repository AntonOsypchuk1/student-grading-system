using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemanticAnalysis.Service.Protos;

namespace SGS.SemanticAnalysis.Client;

public static class SemanticAnalysisClientModule
{
    public static void AddSemanticAnalysisClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGrpcClient<SemanticAnalysisService.SemanticAnalysisServiceClient>(o =>
            {
                o.Address = new Uri(configuration["SemanticAnalysisService:Url"] ?? "http://localhost:5000");
            })
            .AddCallCredentials((context, metadata, serviceProvider) =>
            {
                var apiKey = configuration["SemanticAnalysisService:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    metadata.Add("x-api-key", apiKey);
                }
                return Task.CompletedTask;
            });

        services.AddScoped<IPromptServiceExecutor, PromptServiceExecutor>();
    }
}
