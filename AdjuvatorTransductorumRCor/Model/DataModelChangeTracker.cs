namespace AdjuvatorTransductorumRCor.Model;

internal class DataModelChangeTracker
{
    // Should I store commits like version control does?
    
    // Current changes;
    private readonly List<DataModelChange> _changes = new();
    // Must be emptied before _changes
    private readonly Queue<DataModelRename> _renamingQueue = new();
    private const string ClassNameLog = "Tracker";
    // if false, tracking doesn't work (should be false upon DM init from XML).
    public bool HasChanges => _changes.Count > 0 || _renamingQueue.Count > 0;
    
    private bool _isTracking = true;
    internal void StartTracking() => _isTracking = true;
    internal void StopTracking() => _isTracking = false;

    private string FullAddress(DataModelChange change)
        =>  string.IsNullOrEmpty(change.Address)
                ? change.NodeName
                : $"{change.Address}:{change.NodeName}";    
    
    internal void AddChange(string address, string node, DataModelChangeType operationType)
    {
        if (!_isTracking) return;
        var change = new DataModelChange(address, node, operationType);
        var parentChange = _changes.FirstOrDefault(storedChange => 
            DataAddress.Contains(FullAddress(change), FullAddress(storedChange)));

        if (parentChange == null)
        {
#if DEBUG
            Console.WriteLine($"[{ClassNameLog}] {operationType} > {address}:{node}");
#endif
            _changes.Add(change);
        }
    }

    internal void AddRename(string address, string node, string oldName)
    {
        _changes.ForEach(change => {
#if DEBUG
            Console.WriteLine($"[{ClassNameLog}] Renaming: {address} >> {oldName} to {node}");
#endif
            change.Address = change.Address.Replace($"{address}:{oldName}", $"{address}:{node}");
        });
        _renamingQueue.Enqueue(new DataModelRename(address, node, oldName, DataModelChangeType.Rename));
    }

    internal void CommitChanges(DataBuilder builder)
    {
        if (builder.ModelXmlWriter is not { } writer) 
            throw new NullReferenceException($"[internal|Core:{ClassNameLog}] Can not save without created writer!");
#if DEBUG
        if (_renamingQueue.Count > 0)
            Console.WriteLine($"[{ClassNameLog}] Executing {_renamingQueue.Count} renames...");
#endif
        while (_renamingQueue.TryDequeue(out var renameInst))
            writer.CommitChange(renameInst);
        
        _changes.ForEach(change => writer.CommitChange(change));
#if DEBUG
        Console.WriteLine($"[{ClassNameLog}] Changes applied: Added {_changes.Count(change => change.ChangeType == DataModelChangeType.Add)}, " +
            $"Removed {_changes.Count(change => change.ChangeType == DataModelChangeType.Remove)}, " +
            $"Edited {_changes.Count(change => change.ChangeType == DataModelChangeType.Edit)}");
#endif
        _changes.Clear();
        builder.ModelXmlWriter.SaveProject();
    }
}