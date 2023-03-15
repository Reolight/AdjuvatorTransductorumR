using System.Reflection;
using AdjuvatorTransductorumRCor.PluginCommonInterface;
using AdjuvatorTransductorumRCor.ViewDescriber;

namespace AdjuvatorTransductorumRCor
{
    public class Core
    {
        public const int CorVersion = 2;

        // 'List' of all commands available for Core constructor
        private static readonly Dictionary<string, Action<string>> Commands = new()
        {
            { 
                "plugfold",
                value =>
                {
                    _pluginFolder = new DirectoryInfo(value);
                    if (!_pluginFolder.Exists) 
                        Console.WriteLine($"[internal|{nameof(Core)}] Directory {value} not found");
                    Console.WriteLine($"[internal|{nameof(Core)}] Plugin folder set to {_pluginFolder.FullName}");
                } 
            }
        };

        private static readonly DirectoryInfo _defPluginFolder = new("Plugins");
        private static DirectoryInfo? _pluginFolder;
        
        private List<IDataProvider> _dataProviders; //Use dictionary with Assemblies Names??
        private List<IDataProviderInfo> _notSupportedProviders = new();
        public IDataProvider? GetProvider(string name) => 
            _dataProviders.FirstOrDefault(prov => prov.Name == name);

        public bool HasSupportedPlugins => (_dataProviders.Count + _notSupportedProviders.Count) > 0;

        public string PluginFolderName => _pluginFolder?.FullName ?? _defPluginFolder.FullName; 
        
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

        private bool ParseLineForCommand(string[] args, string command, out int paramIndex)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() != command) continue;
                
                // To ensure that command name is not the last word
                if (args.Length <= ++i) break;
                paramIndex = i;
                return true;
            }

            paramIndex = -1;
            return false;
        }

        private void CheckArgumentsLine(string[] args)
        {
            foreach (var command in Commands.Keys)
            {
                if (!ParseLineForCommand(args, command, out int paramIndex)) continue;
                Console.WriteLine($"[internal|{nameof(Core)}] Command {command} found. Execution.");
                Commands[command].Invoke(args[paramIndex]);
            }
        }

        /// <summary>
        /// Creates instance of Core, loads plugins
        /// </summary>
        /// <param name="args">Additional parameters: plugFold ./doc/plugins - sets plugin folder in indicated folder.
        /// This is mostly necessary for testing, when proj is replaced to another place with dependencies (thus plugins are
        /// not moved cuz they not have explicit dependencies)</param>
        public Core(params string[] args)
        {
            if (args.Length > 0) Console.WriteLine("Rus with args: " + string.Join(' ', args));
            _dataProviders = new List<IDataProvider>();
            CheckArgumentsLine(args);
            SetUpInstance();
            Console.WriteLine("Plugins active: " + string.Join(", ", _dataProviders.Select(prov => prov.Name)));
        }

        private void SetUpInstance()
        {
            if (!_defPluginFolder.Exists)
                _defPluginFolder.Create();
            GetPlugins(_pluginFolder ?? _defPluginFolder);
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

        public ViewDefinition RetrievePluginExtractionWindowDescription(string pluginName)
        {
            var plugin = _dataProviders.FirstOrDefault(plugin => plugin.Name == pluginName);
            if (plugin is null)
                throw new NullReferenceException("[internal|Core] There is no plugin with such name");
            return plugin.ExtractionViewDescription;
        }
        
        public ViewDefinition RetrievePluginInjectionWindowDescription(string pluginName)
        {
            var plugin = _dataProviders.FirstOrDefault(plugin => plugin.Name == pluginName);
            if (plugin is null)
                throw new NullReferenceException("[internal|Core] There is no plugin with such name");
            return plugin.InjectionViewDescription;
        }
    }
}