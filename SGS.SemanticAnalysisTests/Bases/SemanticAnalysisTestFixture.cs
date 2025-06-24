using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Moq;
using SGS.SemanticAnalysis;
using SGS.SemanticAnalysis.Kernels;
using SGS.SemanticAnalysis.Settings;
using Xunit.Abstractions;

namespace SGS.SemanticAnalysisTests.Bases;

public class SemanticAnalysisTestFixture : IDisposable
{
    private IServiceProvider ServiceProvider { get; set; }
    private readonly ServiceCollection _services;
    private ITestOutputHelper? _testOutput;
    private bool _useMockKernel;

    public SemanticAnalysisTestFixture()
    {
        LoadEnvironmentVariables();
        
        _services = new ServiceCollection();
        SetupServices();
    }
    
    private static void LoadEnvironmentVariables()
    {
        string? envPath = FindEnvFile();
        if (envPath != null && File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
            Console.WriteLine($"Loaded environment variables from {envPath}");
        }
        else
        {
            Console.WriteLine("No .env file found");
        }
    }
    
    private static string? FindEnvFile()
    {
        var currentDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "");
        
        while (currentDir != null)
        {
            var envPath = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envPath))
            {
                return envPath;
            }
            
            var solutionEnvPath = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(solutionEnvPath))
            {
                return solutionEnvPath;
            }
            
            currentDir = currentDir.Parent;
        }
        
        return null;
    }

    public void ConfigureForTest(ITestOutputHelper testOutput, bool useMockKernel = false)
    {
        _testOutput = testOutput;
        _useMockKernel = useMockKernel;
        
        SetupServices();
    }

    private void SetupServices()
    {
        var configValues = BuildTestConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _services.Clear();
                
        _services.AddSingleton<IConfiguration>(configuration);

        ConfigureLogging(_services);
        ConfigureSemanticKernelSettings(_services, configuration);
        
        _services.AddSemanticKernel(configuration);

        if (_useMockKernel)
        {
            RegisterMockKernel(_services);
        }

        ServiceProvider = _services.BuildServiceProvider();

        LogConfigurationInfo();
    }

    /// <summary>
    /// Creates a new service scope for isolated test execution
    /// </summary>
    public IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }

    /// <summary>
    /// Gets a service from the container
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Builds the test configuration with environment variables and fallback values
    /// </summary>
    private static Dictionary<string, string> BuildTestConfiguration()
    {
        return new Dictionary<string, string>
        {
            ["SemanticKernelSettings:ApiKey"] = Environment.GetEnvironmentVariable("SEMANTIC_KERNEL_API_KEY") ?? "",
            ["SemanticKernelSettings:Endpoint"] = Environment.GetEnvironmentVariable("SEMANTIC_KERNEL_ENDPOINT") ?? "http://localhost:11434/v1/",
            ["SemanticKernelSettings:DeploymentName"] = Environment.GetEnvironmentVariable("SEMANTIC_KERNEL_DEPLOYMENT_NAME") ?? "",
            ["SemanticKernelSettings:Language"] = Environment.GetEnvironmentVariable("LANGUAGE") ?? "ua",
            ["Logging:LogLevel:Default"] = "Debug",
            ["Logging:LogLevel:System"] = "Information",
            ["Logging:LogLevel:Microsoft"] = "Information"
        };
    }

    /// <summary>
    /// Configures logging with test output if available
    /// </summary>
    private void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Debug);
            
            // Add console logging
            // builder.AddConsole();
            
            if (_testOutput != null)
            {
                builder.AddProvider(new XunitLoggerProvider(_testOutput));
            }
        });
    }

    /// <summary>
    /// Configures the semantic kernel settings
    /// </summary>
    private static void ConfigureSemanticKernelSettings(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelSettings>(options => 
        {
            options.ApiKey = configuration["SemanticKernelSettings:ApiKey"];
            options.Endpoint = configuration["SemanticKernelSettings:Endpoint"];
            options.DeploymentName = configuration["SemanticKernelSettings:DeploymentName"];
            options.Language = configuration["SemanticKernelSettings:Language"];
        });
    }

    /// <summary>
    /// Registers a mock kernel for testing
    /// </summary>
    private static void RegisterMockKernel(IServiceCollection services)
    {
        var mockKernel = new Mock<Kernel>();
        
        services.AddScoped<SemanticKernelFactory>(sp => 
        {
            var settings = sp.GetRequiredService<IOptions<SemanticKernelSettings>>();
            return new MockSemanticKernelFactory(settings, sp, mockKernel.Object);
        });
    }

    /// <summary>
    /// Logs configuration information
    /// </summary>
    private void LogConfigurationInfo()
    {
        var registeredSettings = ServiceProvider.GetRequiredService<IOptions<SemanticKernelSettings>>().Value;
        var logger = ServiceProvider.GetService<ILogger<SemanticAnalysisTestFixture>>();
        
        if (logger != null)
        {
            logger.LogInformation("SemanticKernelSettings: " +
                                  $"ApiKey={!string.IsNullOrEmpty(registeredSettings.ApiKey)}, " +
                                  $"Endpoint={registeredSettings.Endpoint}, " +
                                  $"DeploymentName={registeredSettings.DeploymentName}, " +
                                  $"Language={registeredSettings.Language}, " +
                                  $"Using mock kernel: {_useMockKernel}");
        }
        else
        {
            Console.WriteLine($"SemanticKernelSettings: " +
                              $"ApiKey={!string.IsNullOrEmpty(registeredSettings.ApiKey)}, " +
                              $"Endpoint={registeredSettings.Endpoint}, " +
                              $"DeploymentName={registeredSettings.DeploymentName}, " +
                              $"Language={registeredSettings.Language}, " +
                              $"Using mock kernel: {_useMockKernel}");
        }
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// A mock factory for testing that returns a pre-configured mock kernel
    /// </summary>
    private class MockSemanticKernelFactory(
        IOptions<SemanticKernelSettings> settings,
        IServiceProvider services,
        Kernel mockKernel)
        : SemanticKernelFactory(settings, services)
    {
        public override Kernel CreateBasic()
        {
            return mockKernel;
        }
    }
}