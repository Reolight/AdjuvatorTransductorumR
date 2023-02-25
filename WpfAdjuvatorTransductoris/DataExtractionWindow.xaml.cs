using System;
using System.Windows.Input;
using AdjuvatorTransductorumRCor;

namespace WpfAdjuvatorTransductoris
{
    /// <summary>
    /// Interaction logic for DataExtractionWindow.xaml
    /// </summary>
    public partial class DataExtractionWindow
    {
        public event Action<string>? Confirmed;

        public DataExtractionWindow(Core core)
        {
            InitializeComponent();
            pluginList.DataContext = core.PluginsList.Value;
        }

        private void ExtractionConfirmed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataExtractionWindow window = (DataExtractionWindow)sender;
            Confirmed?.Invoke(((PluginInfo)window.pluginList.SelectedItem).Name);
            Close();
        }

        private void ExtractionConfirmed_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DataExtractionWindow window = (DataExtractionWindow)sender;
            e.CanExecute = window.pluginList.SelectedItem is PluginInfo { IsSupported: true }; // window.pluginList.SelectedIndex >= 0;
        }
    }
}
