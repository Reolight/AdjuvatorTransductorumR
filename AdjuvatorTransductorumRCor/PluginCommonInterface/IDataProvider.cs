using AdjuvatorTransductorumRCor.Model;

namespace AdjuvatorTransductorumRCor
{
    /// <summary>
    /// This interface defines behaviour for 'data providers' plugins. 
    /// Data providers are used for extracting raw data to data model and vice a verse.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Name of plugin. Name it simple
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// List of file formats supported by your plugin by default (e.g.: json, xml).
        /// </summary>
        public List<string> SupportedFiles { get; }
        /// <summary>
        /// Description. Should include: Source of data and way of obtaining.
        /// </summary>
        public string Description { get; }
        public string Author { get; }
        public int InjectData(DataModel vm, string path);
        public DataModel ExtractData(string path);
        public Dictionary<string, string> GetPluginInfo();
    }
}
