namespace AdjuvatorTransductorumRCor.Model;

public class DataModelChangeTracker
{
    // Should I store commits lice version control does?

    // Current changes;
    private List<DataModelChange> _changes = new();
    // Must be emptied before _changes
    private Queue<DataModelRename> _renamingQueue = new(); 

    // if false, tracking doesn't work (should be false upon DM init from XML).
    public bool HasChanges => _changes.Count > 0 || _renamingQueue.Count > 0;
    
    private bool _isTracking = true;

    internal void StartTracking() => _isTracking = true;

    internal void StopTracking() => _isTracking = false;
    
    // Rename must be committed through another method
    internal void AddChange(string address, string node, DataModelChangeType operationType)
    {
        if (!_isTracking) return;
        var change = new DataModelChange(address, node, operationType);
        var parentChange = _changes.FirstOrDefault(storedChange =>
        {
            // here we should check if change above current is exists. If it does, skip it;
            var fullAddressOfStoredChange = string.IsNullOrEmpty(storedChange.Address)
                ? storedChange.NodeName
                : $"{storedChange.Address}:{storedChange.NodeName}";
            return change.Address.Contains(fullAddressOfStoredChange);
        });
        
        if (parentChange == null)
            _changes.Add(change);
    }

    internal void AddRename(string address, string node, string oldName)
    {
        _changes.ForEach(change => 
            change.Address = change.Address.Replace($"{address}:{oldName}", $"{address}:{node}"));
        _renamingQueue.Enqueue(new DataModelRename(address, node, oldName, DataModelChangeType.Rename));
    }

    public void CommitChanges()
    {
        while (_renamingQueue.TryDequeue(out var renameInst))
        {
            
        }
        
        _changes.ForEach(change =>
        {
            
        });
    }
}