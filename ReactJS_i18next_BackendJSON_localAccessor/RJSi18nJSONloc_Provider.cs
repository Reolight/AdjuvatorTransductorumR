using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ReactJS_i18next_BackendJSON_localAccessor
{
    // For extracting translation files from locales folder of i18next
    public class RJSi18nJSONloc_Provider : IDataProvider
    {
        public string Name { get; } = "JSON i18next";
        public string Description { get; } = "Manages JSON files to use with i18next backend library in ReactJS";
        public string Author { get; } = "Reolight Miene";
        public List<string> SupportedFiles { get; } = new List<string> { "json" };

        private readonly Regex _jsonKeyParser = new Regex("(?<=\\\")(.*?)(?=\\\"\\s*:)");
        private readonly Regex _jsonValueParser = new Regex("(?<=:\\s*\\\")(.*?)(?=\\\")");

        private void ParseJson(DataBuilder builder, string json, string lang)
        {
            MatchCollection keys = _jsonKeyParser.Matches(json);
            MatchCollection vals = _jsonValueParser.Matches(json);

            if (vals.Count == 0 || keys.Count == 0)
                return;
            if (vals.Count != keys.Count)
                throw new ArgumentException("Json is broken");
            for (int i = 0; i < keys.Count; i++) {
                builder.AddValue(keys[i].Value, lang, vals[i].Value);
            }
        }
        
        private void GetDataFromFile(FileInfo file, DataBuilder builder, string lang)
        {
            string json = File.ReadAllText(file.FullName, System.Text.Encoding.UTF8);
            ParseJson(builder, json, lang);
        }

        private bool CreateVm(DataBuilder builder, DirectoryInfo dir, string lang)
        {
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                builder.AddNode(d.Name, NodeTypes.Folder);
                CreateVm(builder, d, lang);
                builder.Up();
            }

            foreach (FileInfo f in dir.GetFiles())
            {
                builder.AddNode(f.Name, NodeTypes.File); 
                GetDataFromFile(f, builder, lang); 
                builder.Up();
            }

            return builder.Up();
        }

        private void LangInit(DataBuilder builder, DirectoryInfo dir)
        {
            foreach (DirectoryInfo lang in dir.GetDirectories()) {
                builder.AddLanguage(lang.Name);
                CreateVm(builder, lang, lang.Name);
            }
        }

        public DataModel ExtractData(string path)
        {
            const string locales = "locales";
            DirectoryInfo dir = new DirectoryInfo(path);
            DataModel? model = new DataModel(dir.FullName, locales, SupportedFiles);
            if (!dir.Exists || dir.Name != locales)
            {
                if (dir.Name != locales) Console.WriteLine("There is no folder 'locales'. Creating dir...");
                if (!dir.Exists) Console.WriteLine("There is no such. Creating dir");
                dir.Create();
                return model;
            }

            LangInit(model.Redactor, dir);
            return model;
        }

        private void WriteInFile(FileSystemInfo file, byte[] content)
        {
            FileStream fs = new FileStream(file.FullName, FileMode.Create);
            fs.Write(Encoding.UTF8.GetBytes("{\n"));
            fs.Write(content);
            fs.Write(Encoding.UTF8.GetBytes("\n}"));
            fs.Close();
#if DEBUG
            Console.WriteLine($"JSON in {file.FullName} is written\n\n{content}\n");
#endif
        }
        private bool UnloadInFile(DataModel model, FileSystemInfo parent, string lang)
        {            
            List<string> strings = new List<string>();
            foreach (DataModelLeaf leaf in model.Redactor.ActiveNode.GetNodes())
            {
                var val = leaf.GetValue(lang);
                
                if (!string.IsNullOrEmpty(val))
                {
                    strings.Add($"\t\"{leaf.Name}\" : \"{val}\"");
                }
            }

            if (strings.Count > 0)
                WriteInFile(parent, Encoding.UTF8.GetBytes(string.Join(",\n", strings)));            

            return true;
        }

        private bool UnloadNodes(DataModel model, DataModelNode node, FileSystemInfo parent, string lang)
        {
            switch (node.NodeType)
            {
                case NodeTypes.Folder:
                    DirectoryInfo child = new DirectoryInfo($"{parent.FullName}/{node.Name}");
                    child.Create();
#if DEBUG
                    Console.WriteLine("dir created: " + child.FullName);
#endif
                    model.Redactor.Down(node.Name);
                    _ = Unload(model, child, lang);
                    model.Redactor.Up();
                    break;
                case NodeTypes.File:
                    FileInfo file = new FileInfo(($"{parent.FullName}/{node.Name}"));
                    model.Redactor.Down(node.Name);
                    _ = UnloadInFile(model, file, lang);
                    model.Redactor.Up();
                    break;
            }

            return true;
        }

        private bool Unload(DataModel dm, FileSystemInfo parent, string lang)
        {
            foreach (var node in dm.Redactor.ActiveNode.GetNodes())
            {
                if (node is DataModelNode nd) 
                    _ = UnloadNodes(dm, nd, parent, lang);
            }

            return true;
        }
        
        private void LangCreate(DataModel dm, DirectoryInfo root)
        {
            foreach (var language in dm.Languages)
            {
                DirectoryInfo l = new DirectoryInfo($"{root.FullName}\\{language}");
#if DEBUG
                Console.WriteLine(l.FullName);
#endif
                l.Create();
                _ = Unload(dm, l, language);
            }
        }

        public int InjectData(DataModel dm, string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Name != "locales" || !dir.Exists)
                dir.Create();
            LangCreate(dm, dir);
            return 0;
        }

        public Dictionary<string, string> GetPluginInfo()
            => new()
                {
                    { "N", $"{Name}" },
                    { "A", $"{Author}"},
                    { "V", $"{Assembly.GetExecutingAssembly().GetName().Version}" },
                    { "D", $"{Description}" }
                };
    }
}