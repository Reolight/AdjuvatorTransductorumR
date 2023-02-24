using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using WpfAdjuvatorTransductoris.Helpers;

namespace WpfAdjuvatorTransductoris.ViewModel
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProject
    {
        public event Action<string>? Confirmed; 
        private readonly Regex _nameChecker = new Regex("^[a-zA-ZА-Яа-я0-9-_]*$");
        private readonly AppInfo appInfo = AppInfo.GetInstance();
        
        public NewProject()
        {
            InitializeComponent();
        }

        private void CreateProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewProject window = (NewProject)sender;
            if (Confirmed is not null) Confirmed(window.ProjectNameText.Text);
            window.Close();
        }

        private void CreateProject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is NewProject np)
            {
                e.CanExecute = 
                    ProjectNameText.Text.Length > 2 && 
                    _nameChecker.IsMatch(ProjectNameText.Text) &&
                    !appInfo.ProjectInfos.Select(pi => pi.Name).Contains(ProjectNameText.Text);
            }
        }
    }
}
