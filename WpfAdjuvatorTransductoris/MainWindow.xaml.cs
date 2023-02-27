using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAdjuvatorTransductoris.Helpers;
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
        private ViewModelDataProvider DataProvider;
        private ViewModelTabControl TabController = new();
        private ViewModelExplorer ViewExplorer;
        private AppInfo _appInfo = AppInfo.GetInstance();
        private Core? _core;

        public MainWindow(string viewModelName, bool load = false)
        {            
            InitializeComponent();
            projName = viewModelName;
            ViewExplorer = new();
            ListBoxExplorer.ItemsSource = ViewExplorer.Nodes;
            FileTabControl.ItemsSource = TabController.Tabs;
            DataProvider = new ViewModelDataProvider(load?
                DataModelXmlReader.LoadProject(projName) :
                DataModelFabric.CreateNewDataModel(projName)
            );

            DataProvider.Connect(ViewExplorer);
            DataProvider.Connect(TabController);
            PopulateRecentProjects();
        }

        private void PopulateRecentProjects()
        {
            var recentProjects = _appInfo.GetRecent(projName);
            RecentProjectsMenuItem.ItemsSource = recentProjects
                .Select(recentProject =>
                    new MenuItem
                    {
                        Command = Commands.LoadRecentProject,
                        Header = recentProject
                    });
            RecentProjectsMenuItem.IsEnabled = RecentProjectsMenuItem.Items.Count > 0;
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

        private void OpenPluginFolder_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", System.IO.Directory.GetCurrentDirectory() + "\\Plugin");
        }
    }
}
