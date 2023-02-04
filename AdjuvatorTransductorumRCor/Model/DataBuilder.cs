using System.Xml.Linq;

namespace AdjuvatorTransductorumRCor.Model
{
    public class DataBuilder
    {
        /// <summary>
        /// Here is Root node without physical representation
        /// </summary>
        private readonly DataModelBase _root;
        /// <summary>
        /// Here active node is stored. Active node is the node under focus.
        /// Just builded node will be focused.
        /// Use methods Up to select node on upper level and Down(string address) to select node below.
        /// N.B.: Valued-type node can not be active (because it cannot have childrens). If you call Down with the name 
        /// of valued-type node you'll receive false.
        /// </summary>
        public DataModelNode ActiveNode { get; private set; }
        /// <summary>
        /// Reference to the DataModel
        /// </summary>
        private readonly DataModel _dataModel;

        public DataBuilder(DataModel dataModel)
        {
            _dataModel = dataModel;
            _root = _dataModel.Root ?? 
                    new DataModelNode {
                        Name = "root",
                        NodeType = NodeTypes.Root 
                    };

            ActiveNode = (DataModelNode)_root;
            _dataModel.Root = _root;
        }

        /// <summary>
        /// Performs deletion and renaming. Doesn't change active node!
        /// </summary>
        /// <param name="node">current node</param>
        /// <param name="oldName">The name of language on which action is performed</param>
        /// <param name="newName">New name for renaming. If null, language defined in oldName will be removed</param>
        /// <returns>true if successful.</returns>
        private bool IterateOnValues(DataModelBase node, string oldName, string? newName = null)
        {
            foreach (var child in node.GetNodes())
            {
                if (child.NodeType == NodeTypes.Key && child is DataModelLeaf leaf)
                {
                    if (!string.IsNullOrEmpty(newName))
                    {
                        var val = leaf.GetValue(oldName);
                        leaf.Values.Add(newName, val);
                    }

                    leaf.Values.Remove(oldName);
                } 
                else IterateOnValues(child, oldName, newName);
            }

            return true;
        }
        
        /// <summary>
        /// Adds language to array of languages.
        /// NB: Simple addition of language wont change model tree, because empty string is returned by
        /// default if there is no language key in node. 
        /// </summary>
        /// <param name="name">Name of lang to add</param>
        /// <returns>true, if language was created successfully</returns>
        public bool AddLanguage(string name)
            => _dataModel.AddLanguage(name);

        public bool RenameLanguage(string oldName, string newName)
            => _dataModel.RenameLanguage(oldName, newName) 
               && IterateOnValues(_root, oldName, newName);

        public bool RemoveLanguage(string name)
            => _dataModel.RemoveLanguage(name);

        public bool Up()
        {
            if (ActiveNode.Parent is not null)
                ActiveNode = (DataModelNode)ActiveNode.Parent;
            else return false;
            return true;
        }
        
        public bool Down(string address)
        {
            DataModelBase node = ActiveNode.GetNode(address);
            if (node is DataModelNode childNode)
                ActiveNode = childNode;
            else return false;
            return true;
        }
        public bool Focus(string address, bool isAbsolute = true)
        {
            if (!isAbsolute) return Down(address);
            DataModelBase absNode = _root.GetNode(address);
            if (absNode is DataModelNode node)
                ActiveNode = node;
            else return false;
            return true;
        }
        
        public void Reset() { ActiveNode = (DataModelNode)_root; }

        public bool AddNode(string name, NodeTypes type = NodeTypes.Folder, bool remainActive = false)
        {
            if (ActiveNode.nodes.ContainsKey(name))
            {
                if (!remainActive) 
                    Down(name);
                return false;
            }

            if (ActiveNode.NodeType == NodeTypes.File)
                throw new InvalidOperationException("Creation node type other from Key under File node is invalid");

            if (name.Contains(".")) 
                type = NodeTypes.File;

            DataModelNode node = new()
            { 
                Name = name, 
                NodeType = type, 
                Parent = ActiveNode 
            };

            Console.WriteLine($"{DataAddress.Compress(ActiveNode.GetAddress())} >> {node.Name} added");
            ActiveNode.nodes.Add(name, node);
            if (!remainActive) Down(name);
            return true;
        }
        public bool RemoveNode(string name) => ActiveNode.nodes.Remove(name);
        public bool RemoveActiveNode()
        {
            string name = ActiveNode.Name;
            Up();
            return RemoveNode(name);
        }
        public bool SetValue(string name, string language, string value) => AddValue(name, language, value);
        public bool AddValue(string name, string? language = null, string? value = null)
        {
            if (ActiveNode.nodes.ContainsKey(name))
            {
                DataModelLeaf leaf = (DataModelLeaf)ActiveNode.nodes[name];
                if (!string.IsNullOrWhiteSpace(language) && leaf.Values.ContainsKey(language))
                {
                    leaf.Values[language] = value ?? string.Empty;
                    Console.WriteLine($"{DataAddress.Compress(leaf.GetAddress())} >> to [{language}] [{value}] added");
                }
                else if (!string.IsNullOrWhiteSpace(language))
                {
                    leaf.Values.Add(language, value ?? string.Empty);
                    Console.WriteLine($"{DataAddress.Compress(leaf.GetAddress())} >> [{language}]:[{value}] added");
                }
                else return false;
            }
            else
            {
                DataModelLeaf leaf = new DataModelLeaf
                {
                    Name = name,
                    Parent = ActiveNode,
                    Values = new Dictionary<string, string>(),
                    NodeType = NodeTypes.Key
                };

                ActiveNode.nodes.Add(name, leaf);
                Console.WriteLine($"{DataAddress.Compress(ActiveNode.GetAddress())} >> {leaf.Name} added");
                return string.IsNullOrWhiteSpace(language) || AddValue(name, language, value);
            }

            return true;
        }
        public bool RemoveValue(string name, string? language = null)
        {
            if (string.IsNullOrWhiteSpace(language)) return ActiveNode.nodes.Remove(name);
            return ActiveNode.nodes[name] is DataModelLeaf leaf && leaf.Values.Remove(language);
        }
    }
}
