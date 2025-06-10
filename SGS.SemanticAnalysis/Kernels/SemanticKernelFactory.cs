using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SGS.SemanticAnalysis.Settings;

namespace SGS.SemanticAnalysis.Kernels;

public class SemanticKernelFactory(IOptions<SemanticKernelSettings> settings, IServiceProvider services)
{
    public readonly string PromptLanguage = settings.Value.Language ?? "en";
    
    public virtual Kernel CreateBasic()
    {
        return new SemanticKernelBuilder(settings)
            .AddLogging()
            .AddOpenAiChatCompletion()
            .Build();
    }
}