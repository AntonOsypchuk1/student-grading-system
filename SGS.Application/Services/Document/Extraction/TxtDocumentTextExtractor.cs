using System.Text;
using SGS.Application.Interfaces;

namespace SGS.Application.Services.Document.Extraction;

public class TxtDocumentTextExtractor : IDocumentTextExtractor
{
    public bool CanExtract(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        var encoding = DetectEncoding(fileBytes);
        var text = encoding.GetString(fileBytes);
    
        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text.Substring(1);
        }
    
        return Task.FromResult(text);
    }

    /// <summary>
    /// Checks the encoding of the file based on its byte order mark (BOM).
    /// For now, it's only checking for UTF-8, UTF-16 LE, and UTF-16 BE.
    ///
    /// The default is UTF-8 if no BOM is found.
    /// </summary>
    /// <param name="fileBytes"></param>
    /// <returns></returns>
    private static Encoding DetectEncoding(byte[] fileBytes)
    {
        return fileBytes.Length switch
        {
            >= 3 when fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF => Encoding.UTF8,
            >= 2 when fileBytes[0] == 0xFE && fileBytes[1] == 0xFF => Encoding.BigEndianUnicode,
            >= 2 when fileBytes[0] == 0xFF && fileBytes[1] == 0xFE => Encoding.Unicode,
            _ => Encoding.UTF8
        };
    }
}