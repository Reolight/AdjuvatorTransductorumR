using System.Reflection;
using AdjuvatorTransductorumRCor.Model;

namespace AdjuvatorTransductorumRCor
{
    public class Core : IMainCore
    {
        private List<IDataProvider> _dataProviders; //Use dictionary with Assemblies Names??
        private IDataProvider? _activeDataProvider;
        public int PluginsCount { get; set; }


        public Core()
        {
            _dataProviders = new List<IDataProvider>();
            SetUpInstance();
        }

        private void SetUpInstance()
        {
            DirectoryInfo dir = new DirectoryInfo("Plugin");
            if (!dir.Exists)
            {
                dir.Create();
                Console.WriteLine("There is no plugins yet");
                return;
            }

            GetPlugins(dir);
            if (_dataProviders.Count > 0) _activeDataProvider = _dataProviders[0];
        }

        private Type[]? TryGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }
        private IEnumerable<T>? CreateInstances<T>(Assembly assembly) where T : class
        {
            var types = TryGetTypes(assembly);
            if (types is null) yield break;
            
            foreach (Type type in types)
            {
                if (!typeof(T).IsAssignableFrom(type)) continue;
                var activated = Activator.CreateInstance(type) as T;
                if (activated is { } result)
                {
                    yield return result;
                }
            }
        }

        private void GetPlugins(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files.Where(f => f.Extension == ".dll"))
            {
                PluginLoadContext loadContext = new PluginLoadContext(file.FullName);
                Assembly assembly = loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(file.FullName));

                if (CreateInstances<IDataProvider>(assembly) is {} providers)
                    _dataProviders.AddRange(providers);
            }

            PluginsCount = _dataProviders.Count;
        }


        /// <exception cref="NullReferenceException">Throws when either Provider or Accessor are null</exception>
        public DataModel GetDataModel(string path, string pluginName)
        {
            var found = _dataProviders.Find(p => p.Name == pluginName);
            if (found is { } provider)
            {
                _activeDataProvider = provider;
                return _activeDataProvider.ExtractData(path);
            } 
            
            throw new NullReferenceException("There is no such provider");
        }

        /// <summary>
        /// Saves view model by passing 2 stepls: converting view wmodel to raw data and saving raw data.
        /// </summary>
        /// <param name="viewModel">View model</param>
        /// <param name="path">Path to save location</param>
        /// <returns>Error code: 0 - no errors. 100 - gotten object has wrong format, 
        /// 200 - wrong path, 201 - path is OK, but files can not be written</returns>
        /// <exception cref="NullReferenceException">Throws when either Provider or Accessor are null</exception>
        public int SaveDataModel(DataModel viewModel, string path)
        {
            if (_activeDataProvider == null)
                throw new NullReferenceException(string.Format(
                    "Current data ${0} is null. Make sure you installed at least one",
                    _activeDataProvider == null ? "provider" : "accessor"));
            return 0;
        }

        public int InjectDataModel(DataModel data, string path, string pluginName)
        {
            var found = _dataProviders.Find(p => p.Name == pluginName);
            if (found is { } provider)
            {
                _activeDataProvider = provider;
                return _activeDataProvider.InjectData(data, path);
            }

            throw new NullReferenceException("There is no such provider");
        }

        public IEnumerable<string> GetPluginInfo()
        {
            foreach (var plugin in _dataProviders)
            {
                yield return $"{plugin.Author} - {plugin.Name}";
            }
        }

        public IEnumerable<Dictionary<string, string>> GetFullPluginInfo()
        {
            foreach (IDataProvider prov in _dataProviders)
            {
                yield return prov.GetPluginInfo();
            }
        }
    }
}