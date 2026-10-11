using SemanticAnalysis.Service.Evaluation;
using SemanticAnalysis.Service.Interceptors;
using SemanticAnalysis.Service.Services;
using SemanticAnalysis.Service.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc(options => options.Interceptors.Add<ApiKeyInterceptor>());
builder.Services.Configure<SemanticKernelSettings>(builder.Configuration.GetSection("SemanticKernelSettings"));
builder.Services.AddEvaluation();
var app = builder.Build();
app.MapGrpcService<SemanticAnalysisService>();
app.MapGet("/", () => "SGS semantic evaluation service. Use a gRPC client.");
app.Run();

public partial class Program;
