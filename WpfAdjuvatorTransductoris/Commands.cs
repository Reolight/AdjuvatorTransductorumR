using System.Windows;
using System.Windows.Input;
using WpfAdjuvatorTransductoris.ViewModel;

namespace WpfAdjuvatorTransductoris
{
    internal class Commands
    {
        public static RoutedCommand NewProject { get; set; }
        public static RoutedCommand OpenProject { get; set; }
        public static RoutedCommand ExtractData { get; set; }
        public static RoutedCommand InjectData { get; set; }
        public static RoutedCommand SaveProject { get; set; }
        public static RoutedCommand Save { get; set; }
        public static RoutedCommand LoadProject { get; set; }
        public static RoutedCommand Navigate { get; set; }
        public static RoutedCommand AddFile { get; set; }
        public static RoutedCommand RemoveNode { get; set; }
        public static RoutedCommand AddDir { get; set; }
        public static RoutedCommand OpenLanguageManager { get; set; }

        //StartWindow
        public static RoutedCommand StartWindowNewProject { get; set; }
        //ExtractData
        public static RoutedCommand ExtractionConfirmed { get; set; }
        //NewProject
        public static RoutedCommand CreateProject { get; set; }
        //Common
        public static RoutedCommand Confirm { get; set; }
        public static RoutedCommand Abort { get; set; }
        //LanguageManager
        public static RoutedCommand AddLanguage { get; set; }
        public static RoutedCommand RenameLanguage { get; set; }
        public static RoutedCommand DeleteLanguage { get; set; }
        static Commands()
        {
            NewProject = new RoutedCommand("NewProject", typeof(MainWindow));
            OpenProject = new RoutedCommand("OpenProject", typeof(MainWindow));
            ExtractData = new RoutedCommand("ExtractData", typeof(MainWindow));
            InjectData = new RoutedCommand("InjectData", typeof(MainWindow));
            SaveProject = new RoutedCommand("SaveProject", typeof(MainWindow));
            Save = new RoutedCommand("Save", typeof(MainWindow));
            LoadProject = new RoutedCommand("LoadProject", typeof(Window));
            AddFile = new RoutedCommand("AddFile", typeof(MainWindow));
            RemoveNode = new RoutedCommand("RemoveNode", typeof(MainWindow));
            AddDir = new RoutedCommand("AddDir", typeof(MainWindow));
            OpenLanguageManager = new RoutedCommand("langMan", typeof(MainWindow));

            Navigate = new RoutedCommand("Navigate", typeof(MainWindow));
            
            StartWindowNewProject = new RoutedCommand("SWMewProject", typeof(StartWindow));
            
            ExtractionConfirmed = new RoutedCommand("ExtractionConfirmed", typeof(DataExtractionWindow));
            
            CreateProject = new RoutedCommand("CreateProject", typeof(NewProject));

            Confirm = new RoutedCommand("Confirm", typeof(Window));
            Abort = new RoutedCommand("Abort", typeof(Window));

            AddLanguage = new RoutedCommand();
            DeleteLanguage = new RoutedCommand();
            RenameLanguage = new RoutedCommand();
        }
    }
}
