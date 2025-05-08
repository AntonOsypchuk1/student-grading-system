using SGS.API.Controllers;

namespace SGS.API.Extensions;

public static class FileValidatorExtensions
{
    private static readonly string[] AllowedExtensions = [".pdf"];

    public static bool IsValidFileType(this DocumentController controller, IFormFile file, out string errorMessage)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            errorMessage = $"Invalid file type. Allowed types are: {string.Join(", ", AllowedExtensions)}";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}