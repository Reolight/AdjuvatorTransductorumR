using System;
using AdjuvatorTransductorumRCor.Model;
using System.Collections.ObjectModel;
using System.Linq;
using WpfAdjuvatorTransductoris.Providers;

namespace WpfAdjuvatorTransductoris.ViewModel;

public class ViewModelExplorer : IDataDependable
{
    private DataModel? _data;
    // TODO: Fix reverse order! (throws exceptions "key not found" cuz need to use Stack)
    private DataAddress _address = new();
    private ObservableCollection<Node>? _nodes;

    public DataModel? Data
    {
        get => _data ?? throw new NullReferenceException("Data is null here");
        set
        {
            _data = value;
            _address.Clear(); //Recalculating starts after Address change
        }
    }

    /// <summary>
    /// Returns new instance of DataAddress. Use Down, Up for navigation and Clear for clearing data.
    /// </summary>
    public DataAddress Address
    {
        get => _address;
        set
        {
            _address = value;
            _address.Changed += RecalculateNodes;
        }
    }

    public ObservableCollection<Node> Nodes { get; set; } = new();

    private void RecalculateNodes(object? sender, EventArgs args)
    {
        if (sender is not DataAddress address) throw new InvalidCastException("Gotten object is not an address");
        if (_data is null) throw new NullReferenceException("Data is not exist");
        
        Nodes.Clear();
        var nodes = _data.Root?.GetNodes(address.AddressQueue)
            .Select(node =>
                new Node
                {
                    Name = node.Name,
                    IsFolder = node.NodeType == NodeTypes.Folder
                })
            .OrderByDescending(node => node.IsFolder)
            .ToList();

#if DEBUG 
        Console.WriteLine($"[Explorer] {(string)address}");
#endif

        if (address.Count > 0)
            Nodes.Add(new Node{Name = "...", IsFolder = true});
        nodes?.ForEach(node => Nodes.Add(node));
    }

    public void AddNode(string name)
    {
        if (Data is null) return;
        Data.Redactor.Focus(_address);
        Data.Redactor.AddNode(name);
        RecalculateNodes(_address, EventArgs.Empty);
    }

    public void RenameNode(string currentName, string newName)
    {
        if (Data is null) return;
        Data.Redactor.Focus(_address);
        Data.Redactor.RenameNode(currentName, newName);
        RecalculateNodes(_address, EventArgs.Empty);
    }

    public void RemoveNode(string name)
    {
        if (Data is null) return;
        Data.Redactor.Focus(_address);
        Data.Redactor.RemoveNode(name);
        RecalculateNodes(_address, EventArgs.Empty);
    }

    public ViewModelExplorer()
    {
        Address = new();
    }

    public class Node
    {
        public bool IsFolder { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}