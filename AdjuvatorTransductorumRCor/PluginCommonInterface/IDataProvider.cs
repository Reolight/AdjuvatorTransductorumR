using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.ViewDescriber;

namespace AdjuvatorTransductorumRCor.PluginCommonInterface
{
    /// <summary>
    /// This interface defines behaviour for 'data providers' plugins. 
    /// Data providers are used for extracting raw data to data model and vice a verse.
    /// </summary>
    public interface IDataProvider : IDataProviderInfo
    {
        /// <summary>
        /// Returns description for extraction window. Extraction window must trigger
        /// internal logic of plugin and return DataModel in args.DataExtracted
        /// </summary>
        public ViewDefinition ExtractionViewDescription { get; }
        
        /// <summary>
        /// Returns description for injection window. Injection window must trigger
        /// internal logic of plugin and return true in args.Injected
        /// </summary>
        public ViewDefinition InjectionViewDescription { get; }
    }
}
