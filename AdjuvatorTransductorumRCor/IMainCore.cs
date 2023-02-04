using AdjuvatorTransductorumRCor.Model;

namespace AdjuvatorTransductorumRCor
{
    public interface IMainCore
    {
        public DataModel? GetDataModel(string path, string pluginName);
        public int SaveDataModel(DataModel viewModel, string path);
        public IEnumerable<string> GetPluginInfo();
        public IEnumerable<Dictionary<string, string>> GetFullPluginInfo();
    }
}
