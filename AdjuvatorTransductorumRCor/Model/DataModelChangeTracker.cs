namespace AdjuvatorTransductorumRCor.Model;

internal class DataModelChangeTracker
{
    // Should I store commits like version control does?
    
    // Current changes;
    private readonly List<DataModelChange> _changes = new();
    // Must be emptied before _changes
    private readonly Queue<DataModelRename> _renamingQueue = new(); 

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
            FullAddress(change).Contains(FullAddress(storedChange)));
        
        if (parentChange == null)
            _changes.Add(change);
    }

    internal void AddRename(string address, string node, string oldName)
    {
        _changes.ForEach(change => 
            change.Address = change.Address.Replace($"{address}:{oldName}", $"{address}:{node}"));
        _renamingQueue.Enqueue(new DataModelRename(address, node, oldName, DataModelChangeType.Rename));
    }

    internal void CommitChanges(DataBuilder builder)
    {
        if (builder.ModelXmlWriter is not { } writer) 
            throw new NullReferenceException("[internal|Core:Tracker] Can not save without created writer!");
        
        while (_renamingQueue.TryDequeue(out var renameInst))
            writer.CommitChange(renameInst);
        
        _changes.ForEach(change => writer.CommitChange(change));
        _changes.Clear();
        builder.ModelXmlWriter.PushChanges();
    }
}