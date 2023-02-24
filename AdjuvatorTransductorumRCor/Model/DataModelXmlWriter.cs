﻿using System.Xml.Linq;

namespace AdjuvatorTransductorumRCor.Model;

public sealed class DataModelXmlWriter
{
    #region CONSTRUCTOR

    private const string ProjectSavesDir = "Project";

    private DataModel Model { get; set; }
    private string _name = string.Empty;
    private XDocument _document = new();
    private XElement? _root;
    private List<string> _languages; // ? Does it have any sense ?

    public DataModelXmlWriter(DataModel model){
        _languages = model.Languages;
        Model = model;
    }

    private XElement RootInit()
    {
        var rootElement = _document.Root?.Element("Root");
        return rootElement ?? throw new NullReferenceException("[internal|Core:writer] Root is null");
    }
    
    #endregion

    #region SAVE

    public bool SaveProject(DataModel model, string name)
    {
        if (!Directory.Exists(ProjectSavesDir))
            Directory.CreateDirectory(ProjectSavesDir);
        try
        {
            PushChanges();
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

    public void InitXDocument(XDocument document)
    {
        _document = document;
        _root = RootInit();
    }
    
    public void InitXDocument(string? name = null){
         _document.Add(
                new XElement(nameof(DataModel),
                    new XAttribute(nameof(Model.Languages), string.Join(',', Model.Languages)),
                    new XAttribute(nameof(Model.DefaultFileFormat), string.Join(',', Model.DefaultFileFormat)),

                    new XElement(nameof(Model.Root),
                        new XAttribute(nameof(Model.Root.Name), Model.Root.Name))
                ));

         _name = name ?? string.Empty;
         _root = RootInit();
    }

    private XElement Switcher(DataModelBase node)
        => node switch
        {
            DataModelLeaf leaf => CreateXmlValue(leaf),
            DataModelNode nod => CreateXmlNode(nod),
            _ => throw new TypeAccessException("[internal|Core:Writer] Node type is unknown")
        };
    
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
            )
        );

    #endregion

    #region COMMIT

    internal void PushChanges()
    {
        _document.SaveAsync(
            new FileStream($"{ProjectSavesDir}/{_name}.xml", FileMode.Create),
            SaveOptions.None,
            CancellationToken.None
        );
    }
    
    public void CommitChange(DataModelChange changeInstance)
    {
        XElement parentElement;
        switch (changeInstance.ChangeType)
        {
            case DataModelChangeType.Add when Model.Root.GetNode(changeInstance.NodeName) is { } node:
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
                    Model.Root.GetNode(changeInstance.NodeName) is DataModelLeaf { NodeType: NodeTypes.Key } leafNode:
                parentElement = GetElement(changeInstance.Address);
                CommitValueChange(parentElement, leafNode);
                break;
            default:
                throw new InvalidOperationException("[internal|Core:Writer] "+
                    "Commit error: Operation can not be performed with given parameters");
        }
    }

    private void CommitNodeNew(XElement parent, DataModelBase node)
        => parent.Add(Switcher(node));

    private void CommitValueChange(XElement parent, DataModelLeaf node)
    {
        foreach (var lang in _languages)
        {
            var value = node.Values.ContainsKey(lang) ? node.Values[lang] : string.Empty;
            parent.SetAttributeValue(lang, value);
        }
    }

    #endregion

    #region NAVIGATOR

    private XElement GetElement(string address)
        => GetElement(DataAddress.Split(address));
    
    private XElement GetElement(Queue<string> address)
    {
        if (_root == null) throw new NullReferenceException("[internal|Core:Writer] root is not defined");
        var parentElement = _root;
        while (address.TryDequeue(out var childName))
        {
            if (string.IsNullOrEmpty(childName)) return parentElement;
            var child = parentElement.Element(childName);
            parentElement = child ??
                            throw new NullReferenceException($"[internal|Core:Writer] Child element with name {childName} is null");
        }

        return parentElement;
    }

    #endregion
}