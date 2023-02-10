using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;
using System;
using System.Windows;
using WpfAdjuvatorTransductoris.Providers;
using WpfAdjuvatorTransductoris.ViewModel;
using MessageBox = System.Windows.MessageBox;

namespace WpfAdjuvatorTransductoris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string projName;
        public ViewModelDataProvider DataProvider;
        private ViewModelTabControl TabController = new();
        public ViewModelExplorer ViewExplorer;
        
        private Core? _core;

        public MainWindow(string viewModelName, bool load = false)
        {            
            InitializeComponent();
            projName = viewModelName;

            ViewExplorer ??= new ViewModelExplorer();
            
            ListBoxExplorer.ItemsSource = ViewExplorer.Nodes;
            FileTabControl.ItemsSource = TabController.Tabs;
            
            DataProvider = new ViewModelDataProvider(load?
                DataModelXmlWriter.LoadDataModelFromXml(viewModelName) :
                new DataModel(viewModelName)
            );
            
            DataProvider.Connect(ViewExplorer);
            DataProvider.Connect(TabController);

            if (!load)
                DataModelXmlWriter.SaveDataModelAsXml(DataProvider.Data, projName);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _core = new Core();
        }

        private void PluginList(object sender, RoutedEventArgs e)
        {
            if (_core is null) return;
            string info = string.Join('\n', _core.GetPluginInfo());
            MessageBox.Show(info, "Loaded plugins");
        }

        private void AboutClicked(object sender, RoutedEventArgs e)
            => MessageBox.Show("Adjuvator transductoris is an application for providing an assistance in text translation. \n\n Developed by Reolight", "Adjuvator transductoris per creationem transductionum perfecta");
    }
}
