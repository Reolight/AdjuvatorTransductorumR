using System.Xml.Serialization;

namespace AdjuvatorTransductorumRCor.Model
{
    public abstract class DataModelBase
    {
        public string Name { get; set; }
        
        public DataModelBase? Parent { get; set; }
        public NodeTypes NodeType { get; set; }

        public abstract DataModelBase GetNode();

        public abstract DataModelBase GetNode(string address);
        public abstract DataModelBase GetNode(Queue<string> address);
        public abstract IEnumerable<DataModelBase> GetNodes();
        public abstract IEnumerable<DataModelBase> GetNodes(string address);
        public abstract IEnumerable<DataModelBase> GetNodes(Queue<string> address);
        public abstract string GetValue(string address);
        public abstract string GetValue(Queue<string> address);
        public abstract Stack<string> GetAddress(Stack<string>? address = null);
        public abstract bool HasChildren();
        public bool HasParent() => Parent is not null;
    }
}
