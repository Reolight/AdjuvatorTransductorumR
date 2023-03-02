using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AdjuvatorTransductorumRCor.Model
{
    public sealed class DataModelNode : DataModelBase
    {
        /// <summary>
        /// Should not be accessed directly: addition of new nodes is linked with setting of parential bonds
        /// </summary>
        internal Dictionary<string, DataModelBase> nodes = new();
        public override bool HasChildren() => nodes.Count > 0;

        public override DataModelBase GetNode() => this;
        public override DataModelBase GetNode(string address) 
            => string.IsNullOrEmpty(address)?
                GetNode() 
                : GetNode(DataAddress.Split(address));

        public override DataModelBase GetNode(Queue<string> address)
        {
            if (address.Count > 0 && address.TryDequeue(out string? key) && !string.IsNullOrWhiteSpace(key))
            {
                return nodes[key].GetNode(address);
            }

            return GetNode();
        }

        public override IEnumerable<DataModelBase> GetNodes()
        {
            return nodes.Keys.Select(key => nodes[key].GetNode());
        }

        public override IEnumerable<DataModelBase> GetNodes(string address) => GetNodes(DataAddress.Split(address));

        public override IEnumerable<DataModelBase> GetNodes(Queue<string> address)
        {
            if (address.Count > 0 && address.TryDequeue(out string? key) && !string.IsNullOrWhiteSpace(key))
            {
                return nodes[key].GetNodes(address);
            }
            else return GetNodes();
        }

        public override string GetValue(string address) => GetValue(DataAddress.Split(address));

        public override string GetValue(Queue<string> address)
        {
            if (address.Count > 0 && address.TryDequeue(out string? key) && !string.IsNullOrWhiteSpace(key))
            {
                return nodes[key].GetValue(address);
            }

            return string.Empty;
        }

        public override Stack<string> GetAddress(Stack<string>? address = null)
        {
            if (NodeType == NodeTypes.Root)
            {
                return address ?? new Stack<string>();
            }
            if (address is null)
                address = new Stack<string>(new List<string> { Name });
            else
                address.Push(Name);
            
            return Parent is not null? Parent.GetAddress(address) : address;
        }
    }
}
