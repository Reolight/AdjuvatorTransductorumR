using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AdjuvatorTransductorumRCor.Model;

public sealed class DataModelXmlWriter
{
    #region STATIC

    private const string ProjectSavesDir = "Project";
    private const string DataModelElementName = "DataModel";

    private static DataModelXmlWriter? _currentWriter; //can be opened only 1 project
    private static readonly Regex PathFormatter = new Regex(@"[\w.]+(?=($|:))");
    public static bool ActiveProjectHasChanges { get; private set; } = false;

    public static DataModel LoadDataModelFromXml(string name)
    {
        if (!Directory.Exists(ProjectSavesDir))
            Directory.CreateDirectory(ProjectSavesDir);
        _currentWriter = new DataModelXmlWriter(name);
        return _currentWriter.Load();
    }

    public static bool SaveDataModelAsXml(DataModel model, string name)
    {
        if (!Directory.Exists(ProjectSavesDir))
            Directory.CreateDirectory(ProjectSavesDir);
        if (_currentWriter?._name != name)
            _currentWriter = new DataModelXmlWriter(model, name);

        try
        {
            _currentWriter.Save();
            ActiveProjectHasChanges = false;
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
#endif
            throw;
        }

        return true;
    }

    public static void SaveData()
    {
        if (_currentWriter is null)
            throw new InvalidOperationException("Cannot save, because current save writer is null");
        _currentWriter.Save();
    }

    #endregion

    #region CONSTRUCTOR

    private readonly DataModel _model;
    private readonly string _name;
    private XDocument _document = new();
    private List<string> _languages;
    
    private DataModelXmlWriter(DataModel model, string name)
    {
        _model = model;
        _name = name;
        _languages = new List<string>(model.Languages);
    }

    #endregion

    #region SAVE

    private XElement Switcher(DataModelBase node)
        => node switch
        {
            DataModelLeaf leaf => CreateXmlValue(leaf),
            DataModelNode nod => CreateXmlNode(nod),
            _ => throw new TypeAccessException("Node type is unknown")
        };

    private void Save()
    {
        if (_document.Element(nameof(DataModel)) is null)
        {
            _document.Add(
                new XElement(nameof(DataModel),
                    new XAttribute(nameof(_model.OriginalAddress), _model.OriginalAddress),
                    new XAttribute(nameof(_model.MainFolder), _model.MainFolder),
                    new XAttribute(nameof(_model.Languages), string.Join(',', _model.Languages)),
                    new XAttribute(nameof(_model.DefaultFileFormat), string.Join(',', _model.DefaultFileFormat)),

                    _model.Root is { }
                        ? new XElement(nameof(_model.Root),
                            new XAttribute(nameof(_model.Root.Name), _model.Root.Name),
                            _model.Root.GetNodes().Select(Switcher))
                        : null
                ));
        }

        _document.SaveAsync(
            new FileStream($"{ProjectSavesDir}/{_name}.xml", FileMode.Create),
            SaveOptions.None,
            CancellationToken.None
        );
    }

    private XElement CreateXmlNode(DataModelNode node)
        => new("Node", new object[]
            {
                new XAttribute(nameof(node.Name), node.Name),
                new XAttribute(nameof(node.NodeType), node.NodeType)
            }
            .Concat(node.GetNodes().Select(Switcher))
        );

    private XElement CreateXmlValue(DataModelLeaf leaf)
        => new("Node", new object[]
            {
                new XAttribute(nameof(leaf.Name), leaf.Name),
                new XAttribute(nameof(leaf.NodeType), leaf.NodeType),
            }
            .Concat(_languages
                .Select(lang => new XAttribute(lang, leaf.GetValue(lang)))
            ));

    private bool CreateRootXmlNode(string name, IEnumerable<string> languages)
    {
        return true;
    }

    #endregion

    #region LOAD

    private DataModelXmlWriter(string name)
    {
        _name = name;
        _model = new DataModel(_name);
        _languages = new List<string>();
    }

    // Loads list from comma separated string, skips empty strings. "addItem" is an Add method of a list
    private void LoadCommaSeparatedList(XElement node, string attributeName, Action<string> addItem)
    {
        node.Attribute(attributeName)?.Value
            .Split(',').ToList()
            .ForEach(item =>
            {
                if (!string.IsNullOrWhiteSpace(item))
                    addItem(item);
            });
    }
    
    private DataModel Load()
    {
        _document = XDocument.Load(new FileStream($"{ProjectSavesDir}\\{_name}.xml", FileMode.Open));
        var root = _document.Element(nameof(DataModel));
        if (root is null) throw new Exception("Root node in xml is empty");
        
        // Loading should not be tracked as changes.
        _model.Redactor.ChangeTracker.StopTracking();
        _model.OriginalAddress = root.Attribute(nameof(_model.OriginalAddress))?.Value ?? string.Empty;
        LoadCommaSeparatedList(root, nameof(_model.Languages), 
            (lang) => _model.Redactor.AddLanguage(lang));
        LoadCommaSeparatedList(root, nameof(_model.DefaultFileFormat), 
            (format) => _model.DefaultFileFormat.Add(format));
        _languages = new List<string>(_model.Languages);
        _model.MainFolder = root.Attribute(nameof(_model.MainFolder))?.Value ?? string.Empty;
        _ = ParseNode(_model.Redactor, root.Element("Root")!);
        
        // After loading completion starting to track changes again
        _model.Redactor.ChangeTracker.StartTracking();
        return _model;
    }

    private bool ParseNode(DataBuilder builder, XElement node)
    {
        foreach (var child in node.Elements("Node"))
        {
            var type = child.Attribute("NodeType")?.Value ?? string.Empty;
            NodeTypes nodeType = (NodeTypes)Enum.Parse(typeof(NodeTypes), type);
            var name = child.Attribute("Name")?.Value;
            if (name is null) continue;

            if (nodeType == NodeTypes.Key)
            {
                _languages.ForEach(lang =>
                    builder.AddValue(name, lang, child.Attribute(lang)?.Value)
                );
            }
            else
            {
                builder.AddNode(name, nodeType);
                ParseNode(builder, child);
                builder.Up();
            }
        }

        return true;
    }

    #endregion

    #region COMMIT

    private XElement GetElement(string address)
        => GetElement(DataAddress.Split(address));
    
    private XElement GetElement(Queue<string> address)
    {
        // should be way to bypass null val of element?
        var parentElement = _document.Root?.Element("Root");
        // Just to ensure, if element is initialized. If not, bypass will be provided
        if (parentElement == null) throw new NullReferenceException("Root element is null");
        while (address.TryDequeue(out var childName))
        {
            var child = parentElement.Element(childName);
            parentElement = child ??
                            throw new NullReferenceException($"Child element with name {childName} is null");
        }

        return parentElement;
    }

    private void CommitNodeNew(XElement parent, DataModelBase node)
    {
        parent.Add(node.GetNodes().Select(Switcher));
    }

    private void CommitValueChange(XElement parent, DataModelLeaf node)
    {
        foreach (var lang in _languages)
        {
            var value = node.Values.ContainsKey(lang) ? node.Values[lang] : string.Empty;
            parent.SetAttributeValue(lang, value);
        }
    }
    
    public void CommitChange(DataModelChange changeInstance)
    {
        XElement parentElement;
        switch (changeInstance.ChangeType)
        {
            case DataModelChangeType.Add when _model.Root?.GetNode(changeInstance.NodeName) is { } node:
                parentElement = GetElement(changeInstance.Address);
                CommitNodeNew(parentElement, node);
                break;
            case DataModelChangeType.Remove:
                parentElement = GetElement(changeInstance.Address);
                parentElement.Remove();
                break;
            case DataModelChangeType.Rename when changeInstance is DataModelRename renameInstance:
                parentElement = GetElement(renameInstance.Address);
                parentElement.SetAttributeValue("Name", renameInstance.NodeName);
                break;
            case DataModelChangeType.Edit when 
                    _model.Root?.GetNode(changeInstance.NodeName) is DataModelLeaf { NodeType: NodeTypes.Key } leafNode:
                parentElement = GetElement(changeInstance.Address);
                CommitValueChange(parentElement, leafNode);
                break;
            default:
                throw new InvalidOperationException(
                    "Commit error: Operation can not be performed with given parameters");
        }
    }
    
    #endregion
}