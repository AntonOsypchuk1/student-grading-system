using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SGS.SemanticAnalysis.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace SGS.SemanticAnalysis.Kernels;

public class SemanticKernelBuilder(IOptions<SemanticKernelSettings> settings)
{
    private readonly SemanticKernelSettings _settings = settings.Value;
    private readonly IKernelBuilder _builder = Kernel.CreateBuilder();

    public SemanticKernelBuilder AddLogging()
    {
        // _builder.Services.AddLogging(configure => 
        // {
        //     configure.AddConsole();
        //     configure.SetMinimumLevel(LogLevel.Information);
        // });
        
        return this;
    }

    public SemanticKernelBuilder AddOpenAiChatCompletion()
    {
        if (string.IsNullOrEmpty(_settings.DeploymentName) ||
            string.IsNullOrEmpty(_settings.Endpoint))
        {
            return this;
        }

        _builder.AddOpenAIChatCompletion(
            modelId: _settings.DeploymentName,
            apiKey: _settings.ApiKey,
            endpoint: new Uri(_settings.Endpoint));
        
        return this;
    }

    public Kernel Build() => _builder.Build();
}