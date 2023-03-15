using System.Xml.Serialization;

namespace AdjuvatorTransductorumRCor.Model
{
    public abstract class DataModelBase
    {
        // TODO: Cached address
        protected string? CachedAddress;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            internal set
            {
                _name = value;
                CachedAddress = null;
            }
        }

        public DataModelBase? Parent { get; internal set; }
        public NodeTypes NodeType { get; internal set; }

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
        
        // those string is here for the future. Main goal of it - indexing;
        // Should solve 2 problems: redundant GetAddress and long GetNodes queries
        
        // internal abstract void Indexing(); 
        public bool HasParent() => Parent is not null;
    }
}
