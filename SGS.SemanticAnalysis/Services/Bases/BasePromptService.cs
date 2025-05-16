using System.Reflection;
using System.Text.RegularExpressions;
using HandlebarsDotNet.IO.Formatters.DefaultFormatters;
using Newtonsoft.Json;

namespace SGS.SemanticAnalysis.Plugins.Bases;

public abstract class BasePromptService
{
    private const string FolderName = "Prompts";
    private const int TimeoutInMilliseconds = 30000;

    private string? _promptYaml;
    public string PromptYaml
    {
        get
        {
            _promptYaml ??= ReadPrompt(GetYamlFileName());

            return _promptYaml;
        } 
    }
    
    protected abstract string GetYamlFileName();

    protected virtual string ReadPrompt(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));
        
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var file = Path.Combine(dir, FolderName, fileName);
        
        using StreamReader reader = new(file);
        
        return reader.ReadToEnd();
    }
    
    protected static T? DeserializeJsonFromResponse<T>(string? response)
    {
        if (string.IsNullOrEmpty(response))
            return default;

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
}