using System.Net;
using System.Runtime.CompilerServices;
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

    private DataModel Load()
    {
        _document = XDocument.Load(new FileStream($"{ProjectSavesDir}\\{_name}.xml", FileMode.Open));
        var root = _document.Element(nameof(DataModel));
        if (root is null) throw new Exception("Root node in xml is empty");
        _model.OriginalAddress = root.Attribute(nameof(_model.OriginalAddress))?.Value ?? string.Empty;
        root.Attribute(nameof(_model.Languages))?.Value
            .Split(',').ToList()
            .ForEach(lang => _model.Redactor.AddLanguage(lang));
        root.Attribute(nameof(_model.DefaultFileFormat))?.Value
            .Split(",").ToList()
            .ForEach(_model.DefaultFileFormat.Add);
        _languages = new List<string>(_model.Languages);
        _model.MainFolder = root.Attribute(nameof(_model.MainFolder))?.Value ?? string.Empty;
        _ = ParseNode(_model.Redactor, root.Element("Root")!);
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

    /// <summary>
    /// By this method execution GetElement of active writer instance will be called.
    /// </summary>
    /// <param name="address">Relative address of element in data model</param>
    /// <returns>New or found XElement</returns>
    /// <exception cref="InvalidOperationException">Current writer is not defined</exception>
    private static XElement GetElementByAddress(string address)
    {
        if (_currentWriter is null)
            throw new InvalidOperationException("There is no active save writer");
        var node = _currentWriter.GetElement(
            _currentWriter._document.Root!.Element("Root")!,
            new Queue<string>(PathFormatter.Matches(address).Select(match => match.Value).ToList())
        );

        return node;
    }

    public static void CommitNewNode(string address) => GetElementByAddress(address);

    public static void CommitRemoveNode(string address)
    {
        var node = GetElementByAddress(address);
        node.Remove();
    }

    public static void CommitNewValue(string address, string lang, string value)
    {
        if (_currentWriter is null) return;
        var node = GetElementByAddress(address);
        _currentWriter.CommitNewValue(node, lang, value);
    }

    private bool DoLangOperation(XElement node, string lang, bool? isDelete = null, string? newLangName = null)
    {
        if (!string.IsNullOrWhiteSpace(newLangName)) //renaming language, if is not null
        {
            var value = node.Attribute(lang)?.Value;
            node.Attribute(lang)?.Remove();
            node.SetAttributeValue(newLangName, value ?? string.Empty);
            return true;
        }
        
        if (isDelete == false) //newLangName must be null
        {
            node.SetAttributeValue(lang, string.Empty);
            return true;
        }

        node.Attribute(lang)?.Remove(); //isDelete null ar true
        return true;
    }

    private bool UpdateLangValues(XElement node, string lang, bool? isDelete = null, string? newLangName = null)
    {
        foreach (var child in node.Elements("Node"))
        {
            if (child.Attribute("NodeType")!.Value != "Key")
            {
                _model.Redactor.Down(child.Attribute("Name")!.Value);
                _ = UpdateLangValues(child, lang, isDelete, newLangName);
                _model.Redactor.Up();
            }
            else
            {
                DoLangOperation(child, lang, isDelete, newLangName);
            }
        }

        return true;
    }
    
    /// <summary>
    /// Updates XDocument by creating, removing or renaming language
    /// </summary>
    /// <param name="lang">The language on which action is performed</param>
    /// <param name="isDelete">When true or null, language will be deleted, if false - created </param>
    /// <param name="newLangName">When defined language will be renamed to this name ignoring isDelete existence</param>
    /// <exception cref="InvalidOperationException">Throws when there is no active document writer.
    /// Tip: try to check if saving or loading were called before this method call</exception>
    public static void CommitAddRemoveRenameLanguage(string lang, bool? isDelete = null, string? newLangName = null)
    {
        if (_currentWriter is null) throw new InvalidOperationException("Current save is not active");
        var rootDataElement = _currentWriter._document.Root;
        if (rootDataElement is not { } root) return;
        root.SetAttributeValue(nameof(_currentWriter._model.Languages), string.Join(',',_currentWriter._model.Languages));
        var dataModelElement = root.Element("Root")!;
        _currentWriter._model.Redactor.Reset();
        _ = _currentWriter.UpdateLangValues(dataModelElement, lang, isDelete, newLangName);
    }

    private static XElement CreateXmlNode(string name, NodeTypes nodeType)
        => new("Node", new XAttribute("Name", name), new XAttribute("NodeType", nodeType));


    /// <summary>
    /// Navigates through XDocument in search of XElement located at given address. 
    /// If XElement doesn't exists, creates new
    /// </summary>
    /// <param name="elem">Relative element</param>
    /// <param name="address">rest of address, navigates relatively to elem</param>
    /// <returns>XElement located at address</returns>
    private XElement GetElement(XElement elem, Queue<string> address)
    {
        var key = address.Dequeue();
        if (string.IsNullOrEmpty(key)) return elem;
        
        var child = elem
            .Elements("Node")
            .FirstOrDefault(e => e.Attribute("Name")?.Value == key);
        if (child == null) {
            child = CreateXmlNode(key,
                key.Contains('.') ? NodeTypes.File :
                    (elem.Attribute("Name")!.Value.Contains('.') ? NodeTypes.Key : NodeTypes.Folder));
            elem.Add(child);
            ActiveProjectHasChanges = true;
        }

        if (address.Count == 0 || elem.Attribute("Name")!.Value.Contains('.'))
            return child!;
        return GetElement(child, address);
    }

    private void CommitNewValue(XElement elem, string lang, string value) {
        elem.SetAttributeValue(lang, value); 
        ActiveProjectHasChanges = true;
    }

    #endregion
}