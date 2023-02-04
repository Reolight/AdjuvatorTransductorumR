using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace AdjuvatorTransductorumRCor
{
    class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string plugin_path)
        {
            _resolver= new AssemblyDependencyResolver(plugin_path);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assembler_path = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assembler_path is string path) 
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }
    }
}
