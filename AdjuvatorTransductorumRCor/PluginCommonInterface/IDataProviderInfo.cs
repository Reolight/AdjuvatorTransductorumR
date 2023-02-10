namespace AdjuvatorTransductorumRCor.PluginCommonInterface;

/// <summary>
/// This is basic plugin interface. There is a high probability that it will be unchangeable.
/// Thus it is used for receiving plugin info and, which is more importantly, for determining if
/// plugin is supported by current Core API version (just a comparison, nothing more).
/// </summary>
public interface IDataProviderInfo
{
    /// <summary>
    /// Name of plugin. Name it simple
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Core API version for which plugin was created. Upon Core is changed, version just increasing by one.
    /// Supported plugin is those having equal version with core.
    /// </summary>
    public int CorVersion { get; }
    /// <summary>
    /// List of file formats supported by your plugin by default (e.g.: json, xml).
    /// </summary>
    public List<string> GetSupportedFiles();
    /// <summary>
    /// Description. Should include: Source of data and way of obtaining.
    /// </summary>
    public string Description { get; }
    
    public string Author { get; }
}