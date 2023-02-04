using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AdjuvatorTransductorumRCor.Model;
using WpfAdjuvatorTransductoris.Helpers;
using WpfAdjuvatorTransductoris.ViewModel;
using MessageBox = System.Windows.MessageBox;

namespace WpfAdjuvatorTransductoris
{
    public partial class MainWindow
    {
        private void NewProj(object sender, ExecutedRoutedEventArgs e)
        {
            NewProject np = new NewProject();
            np.Confirmed += s =>
            {
                MessageBoxResult result =
                    MessageBox.Show("Another project instance found.\nCreate new project anyway?",
                        "Attention", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    Title = s;
                else
                    MessageBox.Show("New project creation cancelled");
            };

            np.Show();
        }

        private void ExtractData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_core is null)
            {
                MessageBox.Show("Core is not initialized");
                return;
            }

            DataExtractionWindow extractionWindow = new DataExtractionWindow(_core);
            extractionWindow.ExtractionConfirmed += (path, pluginName) =>
            {
                try
                {
                    if (_core.GetDataModel(path, pluginName) is { } data)
                        DataProvider.Data = data;
                    TabController.PluginName = pluginName;
                    DataModelXmlWriter.SaveDataModelAsXml(DataProvider.Data, projName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);
#endif
                }
            };

            extractionWindow.Show();
        }

        private void ExtractData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _core is not null;
        }

        private void Navigate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not MainWindow window) return;
            var node = ViewExplorer.Nodes[window.ListBoxExplorer.SelectedIndex];
            if (node.IsFolder)
            {
                if (node.Name == "...")
                    ViewExplorer.Address.Dequeue();
                else
                    ViewExplorer.Address.Enqueue($"{node.Name}");
            }
            else
            {
                var address = ViewExplorer.Address.Address;
                address.Enqueue(node.Name);
                var tabItem = TabController.Tabs
                    .FirstOrDefault(t =>
                        t.Name == Regexes.NameCompressorRestriction.Replace(DataAddress.Compress(address), "_")
                    );
                if (tabItem != null)
                    tabItem.IsSelected = true;
                else
                    TabController.CreateTab(address);
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var activeTab = TabController.Tabs[FileTabControl.SelectedIndex].DataContext as ViewModelTab;
            if (activeTab is { } tab)
            {
                TabController.SaveTab(tab);
            }
        }

        private void Save_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = FileTabControl.SelectedIndex >= 0 && 
                ((ViewModelTab)TabController.Tabs[FileTabControl.SelectedIndex].DataContext).HasChanges;
        }

        private void SaveProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabController.SaveMultipleTabs(TabController.Tabs
                    .Select(tab => tab.DataContext as ViewModelTab)
                    .Where(tab => tab is not null && tab.HasChanges).ToArray());
        }

        private void SaveProject_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            foreach(var tab in TabController.Tabs.Select(t => t.DataContext as ViewModelTab))
            {
                if (tab is null) continue;
                e.CanExecute = tab.HasChanges;
                if (e.CanExecute) break;
            }
        }

        private void AddFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NameGetterWindow namer = new NameGetterWindow(false, DataProvider.Data.DefaultFileFormat);
            namer.NameChanged += ViewExplorer.AddNode;
            namer.ShowDialog();
        }

        private void AddFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NameGetterWindow namer = new NameGetterWindow(true);
            namer.NameChanged += ViewExplorer.AddNode;
            namer.ShowDialog();
        }

        private void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not MainWindow window) return;
            var node = ViewExplorer.Nodes[window.ListBoxExplorer.SelectedIndex];
            if (node is null) return;
            var ConfirmDelete = MessageBox.Show($"Do you want to delete \"{node.Name}\"?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (ConfirmDelete == MessageBoxResult.Yes)
            {
                TabController.ForcedClose($"{ViewExplorer.Address}:{node.Name}");
                ViewExplorer.RemoveNode(node.Name);
            }
        }

        private void Remove_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = (ListBoxExplorer is not null) && (ListBoxExplorer.SelectedIndex >= 0) && !(ViewExplorer.Nodes.Count == 0 || 
                (ViewExplorer.Nodes[0].Name == "..." && ViewExplorer.Nodes.Count == 1));
        
        private void OpenLanguageManager_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var win = Application.Current.Windows.OfType<LanguageManagerWindow>().FirstOrDefault();
            if (win is not null)
                win.Close();
            else
            {
                LanguageManagerWindow lmWin = new LanguageManagerWindow(DataProvider.Data);
                lmWin.Show();
            }
        }
    }
}