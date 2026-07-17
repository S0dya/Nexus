namespace Nexus.Options;

public class CloudSaveOptions
{
    public int MaxSaveSizeKb { get; set; }
    public string DefaultSaveData { get; set; } = "{}";
}