namespace AdjuvatorTransductorumRCor.Model;

/// This class encapsulates changes made in tree in a handy object.
/// Address of changed node, it's name and type of change are stored here.
/// 
public class DataModelChange
{
    // Address of changed node. It does not include name of changed Node!
    internal string Address;

    internal string NodeName;

    internal DataModelChangeType ChangeType;

    internal DataModelChange(string address, string name, DataModelChangeType changeType)
    {
        Address = address;
        NodeName = name;
        ChangeType = changeType;
    }
}