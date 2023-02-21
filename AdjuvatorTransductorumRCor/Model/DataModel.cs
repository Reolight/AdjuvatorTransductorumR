namespace AdjuvatorTransductorumRCor.Model
{
    public sealed class DataModel
    {
        public List<string> DefaultFileFormat { get; internal set; }
        /// <summary>
        /// Address in a system with Main folder.
        /// If there is no Main folder in the address new will be created.
        /// </summary>
        public string OriginalAddress { get; set; }

        /// <summary>
        /// Name of folder where translations are stored
        /// </summary>
        public string MainFolder { get; set; }

        /// <summary>
        /// IsEmpty true if Root is null or contains no children elements
        /// </summary>
        public bool IsEmpty => Root == null || (Root != null && !Root.HasChildren());
        /// <summary>
        /// Common root for relative keys
        /// </summary>
        public DataModelBase? Root;

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

        public DataModel() { }

        public DataModel(string address, string mainFolder, List<string> defFormat) {
            DefaultFileFormat = defFormat;
            OriginalAddress = address;
            MainFolder = mainFolder;
            Redactor = new DataBuilder(this);
        }

        public DataModel(string name)
        {
            DefaultFileFormat = new();
            OriginalAddress = string.Empty;
            MainFolder = name;
            Redactor = new DataBuilder(this);
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
