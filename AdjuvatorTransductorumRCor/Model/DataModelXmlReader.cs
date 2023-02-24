using System.Xml.Linq;

namespace AdjuvatorTransductorumRCor.Model;

public static class DataModelXmlReader
{
    private const string ProjectSavesDir = "Project";

    public static DataModel LoadProject(string name){
        if (Directory.Exists(ProjectSavesDir)) return Load(name);
        Directory.CreateDirectory(ProjectSavesDir);
        throw new FileNotFoundException($"[internal|Core:Writer] Project '{name}' can not be loaded because Project directory was empty");
    }

    private static DataModel Load(string name)
    {
        var document = XDocument.Load(new FileStream($"{ProjectSavesDir}/{name}.xml", FileMode.Open));
        DataModel model = new DataModel(name);
        var xmlRoot = document.Root;
        if (xmlRoot is null) throw new Exception("[internal|Core:Writer] Root node in xml is empty");
        
        // Loading should not be tracked as changes.
        model.Redactor.ChangeTracker.StopTracking();
        LoadCommaSeparatedList(xmlRoot, nameof(model.Languages), 
            (lang) => model.Redactor.AddLanguage(lang));
        LoadCommaSeparatedList(xmlRoot, nameof(model.DefaultFileFormat), 
            (format) => model.DefaultFileFormat.Add(format));
        if (xmlRoot.Element("Root") is { } root)
            _ = ParseNode(model.Redactor, root, model.Languages);
        model.Redactor.ModelXmlWriter.InitXDocument(document);
        
        // After loading completion starting to track changes again
        model.Redactor.ChangeTracker.StartTracking();
        return model;
    }

    private static bool ParseNode(DataBuilder builder, XElement node, List<string> languages)
    {
        foreach (var child in node.Elements("Node"))
        {
            var type = child.Attribute("NodeType")?.Value ?? string.Empty;
            NodeTypes nodeType = (NodeTypes)Enum.Parse(typeof(NodeTypes), type);
            var name = child.Attribute("Name")?.Value;
            if (name is null) 
                throw new Exception("[internal|Core:Writer] Node without name detected. Loading stopped.");

            if (nodeType == NodeTypes.Key)
            {
                languages.ForEach(lang =>
                    builder.AddValue(name, lang, child.Attribute(lang)?.Value)
                );
            }
            else
            {
                builder.AddNode(name, nodeType);
                ParseNode(builder, child, languages);
                builder.Up();
            }
        }

        return true;
    }

    private static void LoadCommaSeparatedList(XElement node, string attributeName, Action<string> addItem)
    {
        node.Attribute(attributeName)?.Value
            .Split(',').ToList()
            .ForEach(item =>
            {
                if (!string.IsNullOrWhiteSpace(item))
                    addItem(item);
            });
    }
}