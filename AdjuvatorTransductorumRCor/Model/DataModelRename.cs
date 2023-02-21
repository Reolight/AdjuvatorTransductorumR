namespace AdjuvatorTransductorumRCor.Model;

public class DataModelRename : DataModelChange
{
    internal string OldName;
    
    internal DataModelRename(string address, string node, string oldName, DataModelChangeType changeType)
        : base(address, node, changeType)
    {
        OldName = oldName;
    }
}