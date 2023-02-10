using AdjuvatorTransductorumRCor.PluginCommonInterface;
using AdjuvatorTransductorumRCor.Model;
using System.Text;
using System.Text.RegularExpressions;
using AdjuvatorTransductorumRCor.ViewDescriber;

namespace ReactJS_i18next_BackendJSON_localAccessor
{
    // For extracting translation files from locales folder of i18next
    public class Rjsi18NJsonLocProvider : IDataProvider
    {
        public string Name => "JSON i18next";
        public string Description => "Manages JSON files to use with i18next backend library in ReactJS";
        public string Author => "Reolight Miene";
        public int CorVersion => 1;

        private static List<string> SupportedFiles { get; } = new() { "json" };

        private static readonly Regex JsonKeyParser = new Regex("(?<=\\\")(.*?)(?=\\\"\\s*:)");
        private static readonly Regex JsonValueParser = new Regex("(?<=:\\s*\\\")(.*?)(?=\\\")");

        public ViewDefinition ExtractionViewDescription { get; }
            = new ViewDefinition("Extraction", new ViewForm[]
            {
                new (ViewTypes.Label)
                {
                    Content = new ViewProp("Select folder to extract from"),
                    HorizontalAlign = ViewContentHorizontalAlign.Left,
                    Width = 200,
                    Height = 25,
                    Margin = 4
                },
                new (ViewTypes.FolderSelection)
                {
                    Name = "folderSelector",
                    Content = new ViewProp(string.Empty),
                    HorizontalAlign = ViewContentHorizontalAlign.Left,
                    Width = 300,
                    Height = 30,
                    Margin = 4
                },
                new (ViewTypes.Button)
                {
                    Content =  new ViewProp("Confirm"),
                    Width = 100,
                    Height = 25,
                    Margin = 12,
                    HorizontalAlign = ViewContentHorizontalAlign.Center,
                    ExecuteEvent = (o, args) =>
                        {
                            if (o is not ViewDefinition definition) return;
                            if (definition.FindViewByName("folderSelector")?.Content is not { } pathProp
                                || pathProp.Property?.ToString() is not { } path) return;
                            args.DataExtracted = ExtractData(path.ToString());
                        },
                    CanExecuteEvent = (o, args) =>
                        {
                            if (o is not ViewDefinition definition) return;
                            if (definition.FindViewByName("folderSelector")?.Content is not { } path) return;
                            args.CanExecute = path.Property?.ToString()?.Length > 0;
                        }
                }
            });

        public ViewDefinition InjectionViewDescription { get; }
            = new ViewDefinition("Injection", new ViewForm[]
            {
                new (ViewTypes.Label)
                {
                    Content = new ViewProp("Select folder to extract from"),
                    HorizontalAlign = ViewContentHorizontalAlign.Left,
                    Width = 200,
                    Height = 25
                },
                new (ViewTypes.FolderSelection)
                {
                    Name = "folderSelector",
                    Content = new ViewProp(string.Empty),
                    HorizontalAlign = ViewContentHorizontalAlign.Left,
                    Width = 300,
                    Height = 30
                },
                new (ViewTypes.Button)
                {
                    Content =  new ViewProp("Confirm"),
                    Width = 100,
                    Height = 25,
                    Margin = 12,
                    CanExecuteEvent = (o, args) =>
                    {
                        if (o is not ViewDefinition definition) return;
                        if (definition.FindViewByName("folderSelector")?.Content is not { } path) return;
                        args.CanExecute = path.Property?.ToString()?.Length > 0;
                    },
                    ExecuteEvent = (o, args) =>
                    {
                        if (o is not ViewDefinition definition) return;
                        if (definition.FindViewByName("folderSelector")?.Content?.Property?.ToString() is not { } path) return;
                        InjectData(definition.Data!, path);
                        args.Injected = true;
                    }
                }
            });
        
        private static void ParseJson(DataBuilder builder, string json, string lang)
        {
            MatchCollection keys = JsonKeyParser.Matches(json);
            MatchCollection vals = JsonValueParser.Matches(json);

            if (vals.Count == 0 || keys.Count == 0)
                return;
            if (vals.Count != keys.Count)
                throw new ArgumentException("Json is broken");
            for (int i = 0; i < keys.Count; i++) {
                builder.AddValue(keys[i].Value, lang, vals[i].Value);
            }
        }
        
        private static void GetDataFromFile(FileInfo file, DataBuilder builder, string lang)
        {
            string json = File.ReadAllText(file.FullName, Encoding.UTF8);
            ParseJson(builder, json, lang);
        }
 
        private static bool CreateVm(DataBuilder builder, DirectoryInfo dir, string lang)
        {
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                builder.AddNode(d.Name);
                _ = CreateVm(builder, d, lang);
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

        private static void LangInit(DataBuilder builder, DirectoryInfo dir)
        {
            foreach (DirectoryInfo lang in dir.GetDirectories()) {
                builder.AddLanguage(lang.Name);
                CreateVm(builder, lang, lang.Name);
            }
        }

        public static DataModel ExtractData(string path)
        {
            const string locales = "locales";
            DirectoryInfo dir = new DirectoryInfo(path);
            DataModel model = new DataModel(dir.FullName, locales, SupportedFiles);
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

        private static void WriteInFile(FileSystemInfo file, byte[] content)
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
        private static bool UnloadInFile(DataModel model, FileSystemInfo parent, string lang)
        {            
            List<string> strings = new List<string>();
            foreach (var dataModelBase in model.Redactor.ActiveNode.GetNodes())
            {
                var leaf = (DataModelLeaf)dataModelBase;
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

        private static bool UnloadNodes(DataModel model, DataModelNode node, FileSystemInfo parent, string lang)
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

        private static bool Unload(DataModel dm, FileSystemInfo parent, string lang)
        {
            foreach (var node in dm.Redactor.ActiveNode.GetNodes())
            {
                if (node is DataModelNode nd) 
                    _ = UnloadNodes(dm, nd, parent, lang);
            }

            return true;
        }
        
        private static void LangCreate(DataModel dm, DirectoryInfo root)
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

        private static void InjectData(DataModel dm, string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Name != "locales" || !dir.Exists)
                dir.Create();
            LangCreate(dm, dir);
        }

        public List<string> GetSupportedFiles()
        {
            return new List<string>(SupportedFiles);
        }
    }
}