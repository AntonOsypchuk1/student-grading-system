namespace SGS.Application.Common.Options;

public class StorageOptions
{
    public string Provider { get; set; } = "Disk";
    public string BasePath { get; set; } = "FileStorage";
}