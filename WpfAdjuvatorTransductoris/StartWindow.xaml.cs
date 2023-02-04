using System;
using System.Windows;
using System.Windows.Input;
using WpfAdjuvatorTransductoris.Helpers;
using WpfAdjuvatorTransductoris.ViewModel;

namespace WpfAdjuvatorTransductoris;

public partial class StartWindow : Window
{
    private AppInfo _appInfo;
    public StartWindow()
    {
        InitializeComponent();
        _appInfo = AppInfo.GetInstance();
        ProjectListBox.DataContext = _appInfo.ProjectInfos;
    }
    
    private void NewProject_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        NewProject np = new NewProject();
        np.Confirmed += s =>
        {
            MainWindow mainWindow = new MainWindow(s);
            mainWindow.Show();
            Close();
        };
            
        np.Show();
    }

    private void LoadProject_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (ProjectListBox.SelectedIndex < 0) return;
        MainWindow mainWindow = new MainWindow(_appInfo.ProjectInfos[ProjectListBox.SelectedIndex].Name, true);
        mainWindow.Show();
        Close();
    }
    
    private void LoadProject_CanExecuted(object sender, CanExecuteRoutedEventArgs e) 
        => e.CanExecute = ProjectListBox.SelectedIndex >= 0;
}