using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AdjuvatorTransductorumRCor;
using WpfAdjuvatorTransductoris.ViewModel;

namespace WpfAdjuvatorTransductoris
{
    /// <summary>
    /// Interaction logic for DataExtractionWindow.xaml
    /// </summary>
    public partial class DataExtractionWindow : Window
    {
        private List<ViewModelPluginInfo> _pluginInfos = new();
        public event Action<string, string>? ExtractionConfirmed;
        public DataExtractionWindow(IMainCore core)
        {
            InitializeComponent();
            core.GetFullPluginInfo()
                .ToList()
                .ForEach(
                    info =>
                    {
                        _pluginInfos.Add(new ViewModelPluginInfo(info));
                    }
                );

            pluginList.ItemsSource = _pluginInfos;
        }

        private void ExtractionConfirmed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataExtractionWindow window = (DataExtractionWindow)sender;
            if (ExtractionConfirmed is not null)
                ExtractionConfirmed(window.PathText.Text,
                    ((ViewModelPluginInfo)window.pluginList.SelectedItem).Name);
            Close();
        }

        private void ExtractionConfirmed_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DataExtractionWindow window = (DataExtractionWindow)sender;
            e.CanExecute = 
                window.PathText.Text.Length > 2 &&
                new DirectoryInfo(window.PathText.Text).Exists &&
                window.pluginList.SelectedIndex >= 0;
        }

        private void PathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderOpen = new FolderBrowserDialog();
            var result = folderOpen.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                PathText.Text = folderOpen.SelectedPath;
            }
        }
    }
}
