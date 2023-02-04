namespace AdjuvatorTransductorumRCor.Model
{
    public class DataModelLeaf : DataModelBase
    {
        /// <summary>
        /// Dictionary with lang key and value as value.
        /// E.g.: this.Name = "hello", this.Values["en"] = "world", this.Values["ru"] = "мир";
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new();

        public DataModelLeaf()
        {
            NodeType = NodeTypes.Key;
        }

        public override Stack<string> GetAddress(Stack<string>? address = null)
        {
            if (address is null)
                address = new Stack<string>(new List<string> { Name });
            else
                address.Push(Name);
            return Parent is not null ? Parent.GetAddress(address) : address;            
        }

        public override DataModelBase GetNode() => this;

        public override DataModelBase GetNode(string address) => this;

        public override DataModelBase GetNode(Queue<string> address) => this;

        public override IEnumerable<DataModelBase> GetNodes()
        {
            throw new ArgumentNullException("You've tried to get nodes. It is a child. There is no nodes");
        }

        public override IEnumerable<DataModelBase> GetNodes(string address) => GetNodes();

        public override IEnumerable<DataModelBase> GetNodes(Queue<string> address) => GetNodes();

        public override string GetValue(string address)
        {
            if (address.Contains(':'))
                throw new ArgumentException("Language determination expected, address was gotten. " +
                    $"check address variable: [{address}]. It must contain here smth like 'ru' or 'en'!");
            return Values.ContainsKey(address)? Values[address] : string.Empty;
        }

        public override string GetValue(Queue<string> address)
        {
            if (address.Count > 1 || address.Count == 0)
                throw new ArgumentException("Language determination expected, address or empty string was gotten. " +
                    $"check address variable: [{DataAddress.Compress(address)}]. " +
                    $"It must contain here smth like 'ru', 'en' or etc!");
            string lang = address.Dequeue();
            return Values.ContainsKey(lang)? Values[lang] : string.Empty;
        }

        public override bool HasChildren() => false;
    }
}
