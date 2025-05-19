using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SGS.SemanticAnalysis.Services.Bases;

public abstract class BasePromptService(ILogger<BasePromptService>? logger = null)
{
    private const string FolderName = "Prompts";
    private const int TimeoutInMilliseconds = 30000;

    private string? _promptYaml;

    protected string PromptYaml
    {
        get
        {
            _promptYaml ??= ReadPrompt(GetYamlFileName());

            return _promptYaml;
        } 
    }
    
    protected abstract string GetYamlFileName();
    
    protected T? DeserializeJsonFromResponse<T>(string? response)
    {
        if (string.IsNullOrEmpty(response))
            return default;
        
        logger?.LogDebug("Request received with {CharCount} characters", response.Length);

        try
        {
            var regexTimeout = TimeSpan.FromMilliseconds(TimeoutInMilliseconds);
            var regex = new Regex(@"\{(.|\s)*\}", RegexOptions.None, regexTimeout);
            
            return JsonConvert.DeserializeObject<T>(regex.Match(response).Value);
        }
        catch (RegexMatchTimeoutException)
        {
            return default;
        }
    }

    private static string ReadPrompt(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));
    
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var dir = Path.GetDirectoryName(assemblyLocation)!;
        var file = Path.Combine(dir, FolderName, fileName);
    
        if (File.Exists(file))
        {
            using StreamReader reader = new(file);
            return reader.ReadToEnd();
        }
    
        var solutionDir = FindSolutionDirectory(dir);
        if (solutionDir != null)
        {
            var mainProjectPromptsPath = Path.Combine(solutionDir, "SGS.SemanticAnalysis", FolderName, fileName);
            if (File.Exists(mainProjectPromptsPath))
            {
                using StreamReader reader = new(mainProjectPromptsPath);
                return reader.ReadToEnd();
            }
        }
    
        throw new FileNotFoundException(
            $"Prompt file '{fileName}' not found in {file} or in the main project directory. " +
            "Make sure the prompt files are present in the Prompts folder of either the test project or the main project.",
            fileName);
    }
    
    private static string? FindSolutionDirectory(string startDir)
    {
        var directory = new DirectoryInfo(startDir);
    
        while (directory is { Parent: not null })
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "SGS.SemanticAnalysis")))
            {
                return directory.FullName;
            }
        
            directory = directory.Parent;
        }
    
        return null;
    }
}