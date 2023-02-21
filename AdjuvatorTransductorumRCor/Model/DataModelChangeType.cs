namespace AdjuvatorTransductorumRCor.Model;

/// <summary>
/// The enum shows the action performed on the node. It can be added, removed or renamed.
/// Editable nodes are those that have a value to edit (DataModelLeaf).
/// </summary>
public enum DataModelChangeType
{
    Add,
    Remove,
    Rename,
    Edit
}