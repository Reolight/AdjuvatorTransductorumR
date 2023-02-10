namespace AdjuvatorTransductorumRCor;

public class PluginInfo
{
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Version? Version { get; set; } = null;
    public int CorVersion { get; set; }
    public bool IsSupported { get; set; }
}