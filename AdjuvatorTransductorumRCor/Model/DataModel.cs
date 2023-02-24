namespace AdjuvatorTransductorumRCor.Model
{
    public sealed class DataModel
    {
        public string Name;

        public void InitWriter(string name)
        {
            Name = name;
            Redactor.ModelXmlWriter.InitXDocument(name);
        }

        public List<string> DefaultFileFormat { get; internal set; } = new();

        /// <summary>
        /// IsEmpty true if Root is null or contains no children elements
        /// </summary>
        public bool IsEmpty => !Root.HasChildren();
        /// <summary>
        /// Common root for relative keys
        /// </summary>
        public DataModelBase Root;

        /// <summary>
        /// True if DataModel tree has tracked changes
        /// </summary>
        public bool DataHasChanges => Redactor.ChangeTracker.HasChanges;
        /// <summary>
        /// Builder constructing Root and redacting after
        /// </summary>
        public DataBuilder Redactor { get; set; }

        public event EventHandler? LanguagesChanged;

        private void OnLanguageChanged()
        {
            LanguagesChanged?.Invoke(this, EventArgs.Empty);
        }

        private List<string> _languages = new();

        /// <summary>
        /// This list is set by Redactor. Doesn't contain anything if created without
        /// existent DataModel source.
        /// How to change languages count: use Redactor for this only! 
        /// </summary>
        public List<string> Languages
        {
            get => new(_languages);
            internal set
            {
                _languages = value;
                OnLanguageChanged();                
            }
        }

        public DataModel(string name, List<string> defFormat) : this(name)
        {
            DefaultFileFormat = defFormat;
        }

        /// <summary>
        /// Creates DataModel with temporal name.
        /// </summary>
        /// <param name="mainFolder"></param>
        /// <param name="defFormat"></param>
        public DataModel(List<string> defFormat) : 
            this(DateTime.Now.ToLongTimeString()) 
        {
            DefaultFileFormat = defFormat;
        }

        public DataModel(string name)
        {
            Name = name;
            Root = new DataModelNode {
                Name = "root",
                NodeType = NodeTypes.Root 
            };

            var modelXmlWriter = new DataModelXmlWriter(this);
            Redactor = new DataBuilder(this, modelXmlWriter);
        }
        
        internal bool AddLanguage(string name)
        {
            if (_languages.Contains(name))
                return false;
            _languages.Add(name);
            OnLanguageChanged();
            return true;
        }

        internal bool RemoveLanguage(string name)
        {
            var isRemoved = _languages.Remove(name);
            if (isRemoved)
                OnLanguageChanged();
            return isRemoved;
        }

        internal bool RenameLanguage(string oldName, string newName)
            => _languages.Contains(oldName) && _languages.Remove(oldName) | AddLanguage(newName);
    }
}
