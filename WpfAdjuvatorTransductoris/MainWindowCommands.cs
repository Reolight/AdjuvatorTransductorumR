using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.ViewDescriber;
using WpfAdjuvatorTransductoris.Helpers;
using WpfAdjuvatorTransductoris.ViewModel;
using Application = System.Windows.Application;
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
                var result = MessageBox.Show("Another project instance found.\nCreate new project anyway?", 
                    "Attention", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var mainWindowWithNewProject = new MainWindow(s);
                    
                    // new main window with new project
                    mainWindowWithNewProject.Show();
                    
                    // closing current window
                    Close();
                    Title = s;
                }
                else
                    MessageBox.Show("New project creation cancelled");
            };

            np.ShowDialog();
        }

        private void ExtractData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Null suppression (CanExecute makes sure it is not null) 
            if (_core == null) return; 
            DataExtractionWindow extractionWindow = new DataExtractionWindow(_core);
            extractionWindow.Confirmed += (pluginName) =>
            {
                try
                {
                    ViewDefinition pluginExtractionWindow = _core.RetrievePluginExtractionWindowDescription(pluginName);
                    
                    var window = PluginViewTranslator.GetWindow(pluginExtractionWindow);
                    window.Closed += (closedObj, _) =>
                    {
                        if (closedObj is not PluginDataCarrierWindow dataCarrierWindow) return;
                        if (dataCarrierWindow.Data == null)
                        {
                            MessageBox.Show("Data extraction cancelled");
                            return;
                        }
                        
                        DataProvider.Data = dataCarrierWindow.Data;
                        DataProvider.Data.InitWriter(projName);
                    };
                    
                    window.ShowDialog();
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

            extractionWindow.ShowDialog();
        }

        private void ExtractData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _core is { HasSupportedPlugins: true };
        }

        private void InjectData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_core == null) return; 
            DataExtractionWindow extractionWindow = new DataExtractionWindow(_core);
            extractionWindow.Confirmed += (pluginName) =>
            {
                try
                {
                    ViewDefinition pluginInjectorWindow = _core.RetrievePluginInjectionWindowDescription(pluginName);
                    var window = PluginViewTranslator.GetWindow(pluginInjectorWindow, DataProvider.Data);
                    window.ShowDialog();
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

            extractionWindow.ShowDialog();
        }

        private void InjectData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _core is { HasSupportedPlugins: true } && DataProvider.Data is { IsEmpty: false };
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
                var address = ViewExplorer.Address.AddressQueue;
                address.Enqueue(node.Name);
                var tabItem = TabController.GetTab(DataAddress.Compress(address));
                if (tabItem != null)
                    tabItem.IsSelected = true;
                else
                    TabController.CreateTab(address);
            }
        }

        private void Load_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.OriginalSource is not MenuItem { Header: string { } headerText }) return;
            MainWindow mainWindow = new(headerText, true);
            mainWindow.Show();
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
            => e.CanExecute = DataProvider.Data.DataHasChanges;

        private void AddFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NameGetterWindow nameGiver = new NameGetterWindow(false, DataProvider.Data.DefaultFileFormat);
            nameGiver.NameChanged += ViewExplorer.AddNode;
            nameGiver.ShowDialog();
        }

        private void AddFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NameGetterWindow nameGiver = new NameGetterWindow(true);
            nameGiver.NameChanged += ViewExplorer.AddNode;
            nameGiver.ShowDialog();
        }

        private void RenameNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.OriginalSource is not ListBoxItem { DataContext: ViewModelExplorer.Node { } explorerNode}) return;
            NameGetterWindow nameEditorWindow = new(explorerNode.IsFolder, DataProvider.Data.DefaultFileFormat, explorerNode.Name);
            nameEditorWindow.NameChanged += newName =>
            {
                var (parentAddress, newAddress) = (ViewExplorer.Address.AddressQueue, ViewExplorer.Address.AddressQueue);
                parentAddress.Enqueue(explorerNode.Name);
                newAddress.Enqueue(newName);
                TabController.RenameNestedTabs(parentAddress, newAddress);
                ViewExplorer.RenameNode(explorerNode.Name, newName);
            };

            nameEditorWindow.ShowDialog();
        }

        private void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not MainWindow window) return;
            var node = ViewExplorer.Nodes[window.ListBoxExplorer.SelectedIndex];
            var confirmDelete = MessageBox.Show($"Do you want to delete \"{node.Name}\"?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmDelete == MessageBoxResult.Yes)
            {
                TabController.ForcedClose(DataAddress.SmartCompress(ViewExplorer.Address, node.Name));
                ViewExplorer.RemoveNode(node.Name);
            }
        }

        // Finds if node can be deleted, renamed or copied
        private void NodeOperation_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = (ListBoxExplorer is not null) && (ListBoxExplorer.SelectedIndex >= 0) && !(ViewExplorer.Nodes.Count == 0 || 
                (ViewExplorer.Nodes[0].Name == "..." && ViewExplorer.Nodes.Count == 1));
        
        private void OpenLanguageManager_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var win = Application.Current.Windows.OfType<LanguageManagerWindow>().FirstOrDefault();
            if (win is not null)
                win.Activate();
            else
            {
                LanguageManagerWindow lmWin = new LanguageManagerWindow(DataProvider.Data);
                lmWin.Show();
            }
        }
    }
}