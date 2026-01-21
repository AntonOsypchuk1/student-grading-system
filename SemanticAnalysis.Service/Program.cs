using SemanticAnalysis.Service.Helpers;
using SemanticAnalysis.Service.Interceptors;
using SemanticAnalysis.Service.Kernels;
using SemanticAnalysis.Service.RetryPolicies;
using SemanticAnalysis.Service.Services;
using SemanticAnalysis.Service.Services.Bases;
using SemanticAnalysis.Service.Settings;
using SemanticAnalysis.Service.Structures.PromptRequests;
using SemanticAnalysis.Service.Structures.PromptResponses;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ApiKeyInterceptor>();
});

builder.Services.Configure<SemanticKernelSettings>(options =>
{
    builder.Configuration.GetSection("SemanticKernelSettings").Bind(options);
});

builder.Services.AddScoped<SemanticKernelFactory>();
builder.Services.AddScoped<SemanticKernelBuilder>();

builder.Services.AddScoped<ISemanticKernelRetryPolicy, SemanticKernelRetryPolicy>();

builder.Services.AddScoped<IPromptBuilder, PromptBuilder>();
builder.Services.AddScoped<IPromptServiceExecutor, PromptServiceExecutor>();
builder.Services.AddScoped<IPromptService<EvaluateCriteriaRequest, EvaluateCriteriaResponse>, EvaluateCriterionService>();
builder.Services.AddScoped<IPromptService<GenerateSummaryRequest, GenerateSummaryResponse>, GenerateSummaryService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<SemanticAnalysisService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
