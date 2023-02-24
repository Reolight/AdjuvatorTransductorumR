using System.Reflection;
using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.PluginCommonInterface;
using AdjuvatorTransductorumRCor.ViewDescriber;

namespace AdjuvatorTransductorumRCor
{
    public class Core
    {
        public const int CorVersion = 1;
        
        private List<IDataProvider> _dataProviders; //Use dictionary with Assemblies Names??
        private List<IDataProviderInfo> _notSupportedProviders = new();

        public bool HasSupportedPlugins => (_dataProviders.Count + _notSupportedProviders.Count) > 0;
        /// <summary>
        /// Contains computed list of plugins info. Computed upon first call
        /// </summary>
        public Lazy<IEnumerable<PluginInfo>> PluginsList =>
            new(() =>
            _dataProviders.Select(plugin =>
                new PluginInfo
                {
                    Name = plugin.Name,
                    Author = plugin.Author,
                    Description = plugin.Description,
                    Version = plugin.GetType().Assembly.GetName().Version,
                    CorVersion = plugin.CorVersion,
                    IsSupported = true
                }).Concat(
                _notSupportedProviders.Select(unsupported =>
                    new PluginInfo
                    {
                        Name = unsupported.Name,
                        Author = unsupported.Author,
                        Description = unsupported.Description,
                        Version = unsupported.GetType().Assembly.GetName().Version,
                        CorVersion = unsupported.CorVersion,
                        IsSupported = false
                    })), LazyThreadSafetyMode.None);

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
                // NoPluginException?
                return;
            }

            GetPlugins(dir);
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
        private void CreateInstances(Assembly assembly)
        {
            var types = TryGetTypes(assembly);
            if (types is null) return;
            
            foreach (Type type in types)
            {
                if (!typeof(IDataProviderInfo).IsAssignableFrom(type)) continue;
                var activated = Activator.CreateInstance(type) as IDataProviderInfo;
                if (activated is { CorVersion: CorVersion })
                {
                    if (Activator.CreateInstance(type) is IDataProvider provider)
                        _dataProviders.Add(provider);
                }
                else
                {
                    if (activated != null)
                        _notSupportedProviders.Add(activated);
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
                CreateInstances(assembly);
            }

            PluginsCount = _dataProviders.Count;
        }

        public IEnumerable<string> GetPluginInfo()
        {
            foreach (var plugin in _dataProviders)
            {
                yield return $"{plugin.Author} - {plugin.Name}";
            }
        }

        public ViewDefinition CallPluginExtractionWindow(string pluginName)
        {
            var plugin = _dataProviders?.FirstOrDefault(plugin => plugin.Name == pluginName);
            if (plugin is null)
                throw new NullReferenceException("[internal|Core] There is no plugin with such name");
            return plugin.ExtractionViewDescription;
        }
        
        public ViewDefinition CallPluginInjectionWindow(string pluginName)
        {
            var plugin = _dataProviders?.FirstOrDefault(plugin => plugin.Name == pluginName);
            if (plugin is null)
                throw new NullReferenceException("[internal|Core] There is no plugin with such name");
            return plugin.InjectionViewDescription;
        }
    }
}