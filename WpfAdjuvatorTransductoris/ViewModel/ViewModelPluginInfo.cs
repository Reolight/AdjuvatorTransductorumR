using System.Collections.Generic;

namespace WpfAdjuvatorTransductoris.ViewModel
{
    public class ViewModelPluginInfo
    {
        public string Name { get; set; } 
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        public ViewModelPluginInfo(Dictionary<string, string> info)
        {
            Name = info["N"];
            Author = info["A"];
            Description = info["D"];
            Version = info["V"];
        }
    }
}
